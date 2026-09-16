using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using DailyQuest.Infrastructure;
using DailyQuest.Localization;
using DailyQuest.Models;
using DailyQuest.Services;
using DailyQuest.Theming;

namespace DailyQuest.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const int CurrentSchemaVersion = 4;
    private const int MaximumScheduleOffset = 8;
    private static readonly string AppVersion =
        typeof(MainViewModel).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

    private readonly IStateStore _stateStore;
    private readonly IStorageUsageService _storageUsageService;
    private readonly IThemeService _themeService;
    private readonly Func<DateTimeOffset> _now;
    private readonly AppState _state;
    private string _newItemText = string.Empty;
    private bool _alwaysOnTop;
    private bool _isHistoryView;
    private bool _isUpcomingView;
    private bool _isSettingsView;
    private int _selectedScheduleOffset;
    private bool _suppressItemPersistence;
    private string _applicationStorageText = "0 B";
    private string _dataStorageText = "0 B";
    private string _historyStorageText = "0 B";

    public MainViewModel(
        IStateStore? stateStore = null,
        Func<DateTimeOffset>? now = null,
        IStorageUsageService? storageUsageService = null,
        IThemeService? themeService = null)
    {
        _stateStore = stateStore ?? new JsonStateStore();
        _storageUsageService = storageUsageService ?? new StorageUsageService();
        _themeService = themeService ?? NullThemeService.Instance;
        _now = now ?? (() => DateTimeOffset.Now);

        var loadedState = _stateStore.Load();
        var isFirstRun = loadedState is null;
        _state = loadedState ?? CreateFirstRunState(_now());

        var needsV1HistoryMigration = _state.SchemaVersion < 2;
        var didNormalize = NormalizeState();
        _themeService.Apply(_state.Settings.ThemeCode);
        Items = new ObservableCollection<ChecklistItem>(
            _state.Items
                .OrderBy(item => item.SortOrder)
                .Select(item => new ChecklistItem(
                    item.Id,
                    item.Text,
                    item.IsCompleted,
                    item.CreatedAt)));
        HistoryEntries = [];
        UpcomingQuests = [];
        ScheduleOptions = [];

        foreach (var item in Items)
        {
            SubscribeToItem(item);
        }

        _alwaysOnTop = _state.Settings.AlwaysOnTop;

        AddItemCommand = new RelayCommand(AddItem, CanAddItem);
        RemoveItemCommand = new RelayCommand(RemoveItem, parameter => parameter is ChecklistItem);
        RemoveScheduledQuestCommand = new RelayCommand(
            RemoveScheduledQuest,
            CanRemoveScheduledQuest);
        ResetTodayCommand = new RelayCommand(ResetToday, () => CompletedCount > 0);
        ClearCompletedCommand = new RelayCommand(ClearCompleted, () => CompletedCount > 0);
        TogglePinCommand = new RelayCommand(TogglePin);
        ToggleLanguageCommand = new RelayCommand(ToggleLanguage);
        SetLanguageCommand = new RelayCommand(SetLanguage);
        SetThemeCommand = new RelayCommand(SetTheme);
        SetScheduleOffsetCommand = new RelayCommand(
            SetScheduleOffset,
            parameter => TryGetScheduleOffset(parameter, out _));
        ShowTodayCommand = new RelayCommand(ShowToday);
        ShowHistoryCommand = new RelayCommand(ShowHistory);
        ShowUpcomingCommand = new RelayCommand(ShowUpcoming);
        ShowSettingsCommand = new RelayCommand(ShowSettings);
        ClearHistoryCommand = new RelayCommand(ClearHistory, () => _state.History.Count > 0);

        if (needsV1HistoryMigration && !string.IsNullOrWhiteSpace(_state.CurrentDate))
        {
            SyncHistoryFromActiveDay(preserveCompletedOrphans: false);
        }

        var didRollOver = RollOverToCurrentDay(saveAfterReset: false);
        RefreshHistoryEntries();
        RefreshScheduleOptions();
        RefreshUpcomingQuests();

        if (isFirstRun || didNormalize || didRollOver)
        {
            Save();
        }
    }

    public ObservableCollection<ChecklistItem> Items { get; }

    public ObservableCollection<HistoryEntryViewModel> HistoryEntries { get; }

    public ObservableCollection<UpcomingQuestViewModel> UpcomingQuests { get; }

    public ObservableCollection<ScheduleOptionViewModel> ScheduleOptions { get; }

    public ICommand AddItemCommand { get; }

    public ICommand RemoveItemCommand { get; }

    public ICommand RemoveScheduledQuestCommand { get; }

    public ICommand ResetTodayCommand { get; }

    public ICommand ClearCompletedCommand { get; }

    public ICommand TogglePinCommand { get; }

    public ICommand ToggleLanguageCommand { get; }

    public ICommand SetLanguageCommand { get; }

    public ICommand SetThemeCommand { get; }

    public ICommand SetScheduleOffsetCommand { get; }

    public ICommand ShowTodayCommand { get; }

    public ICommand ShowHistoryCommand { get; }

    public ICommand ShowUpcomingCommand { get; }

    public ICommand ShowSettingsCommand { get; }

    public ICommand ClearHistoryCommand { get; }

    public WidgetWindowState SavedWindow => _state.Window;

    public UiCopy Copy => UiCopyCatalog.For(_state.Settings.LanguageCode);

    public string LanguageCode => _state.Settings.LanguageCode;

    public string LanguageBadge => LanguageCode == UiCopyCatalog.EnglishCode ? "EN" : "ID";

    public bool IsIndonesian => LanguageCode == UiCopyCatalog.IndonesianCode;

    public bool IsEnglish => LanguageCode == UiCopyCatalog.EnglishCode;

    public string ThemeCode => _state.Settings.ThemeCode;

    public bool IsLightTheme => ThemeCode == ThemeCatalog.LightCode;

    public bool IsDarkTheme => ThemeCode == ThemeCatalog.DarkCode;

    public bool IsTodayView => !_isHistoryView && !_isUpcomingView && !_isSettingsView;

    public bool IsHistoryView => _isHistoryView;

    public bool IsUpcomingView => _isUpcomingView;

    public bool IsSettingsView => _isSettingsView;

    public bool HasUpcomingQuests => UpcomingQuests.Count > 0;

    public int SelectedScheduleOffset
    {
        get => _selectedScheduleOffset;
        set
        {
            if (value is < 0 or > MaximumScheduleOffset ||
                !SetField(ref _selectedScheduleOffset, value))
            {
                return;
            }

            RefreshScheduleOptions();
            OnPropertyChanged(nameof(SelectedScheduleLabel));
        }
    }

    public string SelectedScheduleLabel => ScheduleOptions
        .FirstOrDefault(option => option.Offset == SelectedScheduleOffset)?.Label
        ?? GetScheduleLabel(SelectedScheduleOffset);

    public ChecklistItem? NextPendingItem => Items.FirstOrDefault(item => !item.IsCompleted);

    public bool HasPendingItem => NextPendingItem is not null;

    public string FooterText => string.Format(Copy.Culture, Copy.FooterFormat, AppVersion);

    public string ApplicationStorageText
    {
        get => _applicationStorageText;
        private set => SetField(ref _applicationStorageText, value);
    }

    public string DataStorageText
    {
        get => _dataStorageText;
        private set => SetField(ref _dataStorageText, value);
    }

    public string HistoryStorageText
    {
        get => _historyStorageText;
        private set => SetField(ref _historyStorageText, value);
    }

    public string NewItemText
    {
        get => _newItemText;
        set
        {
            if (!SetField(ref _newItemText, value ?? string.Empty))
            {
                return;
            }

            RaiseCommandStates();
        }
    }

    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;
        private set
        {
            if (!SetField(ref _alwaysOnTop, value))
            {
                return;
            }

            OnPropertyChanged(nameof(PinTooltip));
        }
    }

    public string PinTooltip => AlwaysOnTop
        ? Copy.PinOffTooltip
        : Copy.PinOnTooltip;

    public string Greeting
    {
        get
        {
            var hour = _now().Hour;
            return hour switch
            {
                < 11 => Copy.MorningGreeting,
                < 15 => Copy.NoonGreeting,
                < 19 => Copy.AfternoonGreeting,
                _ => Copy.EveningGreeting
            };
        }
    }

    public string FriendlyDate => FormatDate(_now().Date, includeYear: false);

    public int CompletedCount => Items.Count(item => item.IsCompleted);

    public int TotalCount => Items.Count;

    public int RemainingCount => TotalCount - CompletedCount;

    public double ProgressPercent => TotalCount == 0
        ? 0
        : (double)CompletedCount / TotalCount * 100;

    public string ProgressText => TotalCount == 0
        ? Copy.ProgressEmpty
        : string.Format(Copy.Culture, Copy.ProgressFormat, CompletedCount, TotalCount);

    public string EncouragementText =>
        TotalCount switch
        {
            0 => Copy.EncourageEmpty,
            _ when CompletedCount == TotalCount => Copy.EncourageDone,
            _ when CompletedCount == 0 => Copy.EncourageNone,
            _ => string.Format(Copy.Culture, Copy.EncourageRemainingFormat, RemainingCount)
        };

    public bool IsAllDone => TotalCount > 0 && CompletedCount == TotalCount;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool RollOverToCurrentDay(bool saveAfterReset = true)
    {
        var now = _now();
        var today = GetLocalDate(now);
        var todayKey = GetDateKey(today);
        var dateChanged = !string.Equals(
            _state.CurrentDate,
            todayKey,
            StringComparison.Ordinal);

        if (dateChanged)
        {
            // Archive the old active list before due quests are promoted. Otherwise a
            // quest scheduled for today would incorrectly appear in yesterday's history.
            if (!string.IsNullOrWhiteSpace(_state.CurrentDate))
            {
                SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
            }

            _suppressItemPersistence = true;
            try
            {
                foreach (var item in Items)
                {
                    item.IsCompleted = false;
                }
            }
            finally
            {
                _suppressItemPersistence = false;
            }

            _state.CurrentDate = todayKey;
        }

        var activatedDueQuests = ActivateDueScheduledQuests(today);
        if (!dateChanged && !activatedDueQuests)
        {
            OnPropertyChanged(nameof(Greeting));
            OnPropertyChanged(nameof(FriendlyDate));
            return false;
        }

        SyncHistoryFromActiveDay(preserveCompletedOrphans: !dateChanged);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        RefreshScheduleOptions(today);
        RefreshUpcomingQuests(today);

        if (saveAfterReset)
        {
            Save();
        }

        return true;
    }

    public void SaveWindowSize(double width, double height)
    {
        _state.Window.Left = null;
        _state.Window.Top = null;

        if (double.IsFinite(width) && width > 0)
        {
            _state.Window.Width = width;
        }

        if (double.IsFinite(height) && height > 0)
        {
            _state.Window.Height = height;
        }

        Save();
    }

    public bool MoveItem(ChecklistItem item, int destinationIndex)
    {
        var sourceIndex = Items.IndexOf(item);
        if (sourceIndex < 0 || Items.Count < 2)
        {
            return false;
        }

        destinationIndex = Math.Clamp(destinationIndex, 0, Items.Count - 1);
        if (sourceIndex == destinationIndex)
        {
            return false;
        }

        Items.Move(sourceIndex, destinationIndex);
        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        OnPropertyChanged(nameof(NextPendingItem));
        OnPropertyChanged(nameof(HasPendingItem));
        RefreshHistoryEntries();
        Save();
        return true;
    }

    public void Save() => _stateStore.Save(CreateSnapshot());

    private static AppState CreateFirstRunState(DateTimeOffset now) => new()
    {
        SchemaVersion = CurrentSchemaVersion,
        CurrentDate = GetDateKey(now),
        Items = [],
        History = [],
        ScheduledQuests = [],
        Settings = new AppSettings
        {
            AlwaysOnTop = true,
            LanguageCode = UiCopyCatalog.IndonesianCode,
            ThemeCode = ThemeCatalog.LightCode
        }
    };

    private static string GetDateKey(DateTimeOffset value) =>
        value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string GetDateKey(DateOnly value) =>
        value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static DateOnly GetLocalDate(DateTimeOffset value) =>
        DateOnly.FromDateTime(value.Date);

    private bool NormalizeState()
    {
        if (_state.SchemaVersion > CurrentSchemaVersion)
        {
            throw new NotSupportedException(
                $"State schema {_state.SchemaVersion} is newer than supported schema {CurrentSchemaVersion}.");
        }

        var needsSave = _state.SchemaVersion != CurrentSchemaVersion;

        _state.Items ??= [];
        _state.History ??= [];
        if (_state.ScheduledQuests is null)
        {
            _state.ScheduledQuests = [];
            needsSave = true;
        }

        _state.Window ??= new WidgetWindowState();
        _state.Settings ??= new AppSettings();

        if (string.IsNullOrWhiteSpace(_state.CurrentDate))
        {
            _state.CurrentDate = GetDateKey(_now());
            needsSave = true;
        }

        _state.Items = NormalizeChecklistStates(_state.Items);

        _state.History = _state.History
            .Where(entry => entry is not null && IsValidDateKey(entry.Date))
            .GroupBy(entry => entry.Date, StringComparer.Ordinal)
            .Select(group => group.Last())
            .Select(entry =>
            {
                entry.Items ??= [];
                entry.Items = NormalizeChecklistStates(entry.Items);
                return entry;
            })
            .Where(entry => entry.Items.Count > 0)
            .OrderByDescending(entry => entry.Date, StringComparer.Ordinal)
            .ToList();

        var scheduledBeforeNormalization = _state.ScheduledQuests;
        var reservedIds = _state.Items
            .Select(item => item.Id)
            .Concat(_state.History.SelectMany(entry => entry.Items.Select(item => item.Id)))
            .ToHashSet();
        var normalizedScheduledQuests = NormalizeScheduledQuestStates(
            scheduledBeforeNormalization,
            reservedIds);
        if (!ScheduledQuestStatesEqual(scheduledBeforeNormalization, normalizedScheduledQuests))
        {
            needsSave = true;
        }

        _state.ScheduledQuests = normalizedScheduledQuests;

        var normalizedLanguage = UiCopyCatalog.NormalizeLanguageCode(_state.Settings.LanguageCode);
        if (!string.Equals(_state.Settings.LanguageCode, normalizedLanguage, StringComparison.Ordinal))
        {
            _state.Settings.LanguageCode = normalizedLanguage;
            needsSave = true;
        }

        var normalizedTheme = ThemeCatalog.Normalize(_state.Settings.ThemeCode);
        if (!string.Equals(_state.Settings.ThemeCode, normalizedTheme, StringComparison.Ordinal))
        {
            _state.Settings.ThemeCode = normalizedTheme;
            needsSave = true;
        }

        _state.Window.Width = Math.Clamp(_state.Window.Width, 430, 760);
        _state.Window.Height = Math.Clamp(_state.Window.Height, 460, 1000);
        _state.SchemaVersion = CurrentSchemaVersion;
        return needsSave;
    }

    private List<ChecklistItemState> NormalizeChecklistStates(IEnumerable<ChecklistItemState> states)
    {
        var seenIds = new HashSet<Guid>();
        var result = new List<ChecklistItemState>();

        foreach (var state in states
                     .Select((state, originalIndex) => new { State = state, OriginalIndex = originalIndex })
                     .Where(entry => entry.State is not null && !string.IsNullOrWhiteSpace(entry.State.Text))
                     .OrderBy(entry => entry.State.SortOrder)
                     .ThenBy(entry => entry.OriginalIndex)
                     .Select(entry => entry.State))
        {
            var id = state.Id;
            if (id == Guid.Empty || !seenIds.Add(id))
            {
                id = Guid.NewGuid();
                seenIds.Add(id);
            }

            result.Add(new ChecklistItemState
            {
                Id = id,
                Text = state.Text.Trim(),
                IsCompleted = state.IsCompleted,
                SortOrder = result.Count,
                CreatedAt = state.CreatedAt == default ? _now() : state.CreatedAt
            });
        }

        return result;
    }

    private List<ScheduledQuestState> NormalizeScheduledQuestStates(
        IEnumerable<ScheduledQuestState> states,
        ISet<Guid> reservedIds)
    {
        var normalizationTime = _now();
        var seenIds = new HashSet<Guid>(reservedIds);
        var result = new List<ScheduledQuestState>();

        foreach (var state in states
                     .Select((state, originalIndex) => new { State = state, OriginalIndex = originalIndex })
                     .Where(entry =>
                         entry.State is not null &&
                         !string.IsNullOrWhiteSpace(entry.State.Text) &&
                         TryParseDateKey(entry.State.ScheduledDate, out _))
                     .OrderBy(entry => entry.State.ScheduledDate, StringComparer.Ordinal)
                     .ThenBy(entry => entry.State.SortOrder)
                     .ThenBy(entry => entry.OriginalIndex)
                     .Select(entry => entry.State))
        {
            var id = state.Id;
            if (id == Guid.Empty || !seenIds.Add(id))
            {
                do
                {
                    id = Guid.NewGuid();
                }
                while (!seenIds.Add(id));
            }

            var cleanText = state.Text.Trim();
            if (cleanText.Length > 120)
            {
                cleanText = cleanText[..120];
            }

            result.Add(new ScheduledQuestState
            {
                Id = id,
                Text = cleanText,
                ScheduledDate = state.ScheduledDate,
                SortOrder = result.Count,
                CreatedAt = state.CreatedAt == default ? normalizationTime : state.CreatedAt
            });
        }

        return result;
    }

    private static bool ScheduledQuestStatesEqual(
        IReadOnlyList<ScheduledQuestState> left,
        IReadOnlyList<ScheduledQuestState> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Count; index++)
        {
            var first = left[index];
            var second = right[index];
            if (first is null ||
                first.Id != second.Id ||
                !string.Equals(first.Text, second.Text, StringComparison.Ordinal) ||
                !string.Equals(first.ScheduledDate, second.ScheduledDate, StringComparison.Ordinal) ||
                first.SortOrder != second.SortOrder ||
                first.CreatedAt != second.CreatedAt)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidDateKey(string? value) =>
        TryParseDateKey(value, out _);

    private static bool TryParseDateKey(string? value, out DateOnly date) =>
        DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);

    private void AddItem()
    {
        var cleanText = NormalizeQuestText(NewItemText);
        if (cleanText.Length == 0 ||
            SelectedScheduleOffset is < 0 or > MaximumScheduleOffset)
        {
            return;
        }

        var now = _now();
        if (SelectedScheduleOffset > 0)
        {
            var targetDate = GetLocalDate(now).AddDays(SelectedScheduleOffset);
            _state.ScheduledQuests.Add(new ScheduledQuestState
            {
                Id = Guid.NewGuid(),
                Text = cleanText,
                ScheduledDate = GetDateKey(targetDate),
                SortOrder = _state.ScheduledQuests.Count,
                CreatedAt = now
            });
            SortAndRenumberScheduledQuests();

            NewItemText = string.Empty;
            SelectedScheduleOffset = 0;
            RefreshUpcomingQuests();
            Save();
            return;
        }

        var item = new ChecklistItem(Guid.NewGuid(), cleanText, false, now);
        SubscribeToItem(item);
        Items.Add(item);
        NewItemText = string.Empty;
        SelectedScheduleOffset = 0;

        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
    }

    private bool CanAddItem() => !string.IsNullOrWhiteSpace(NewItemText);

    private static string NormalizeQuestText(string? text)
    {
        var cleanText = text?.Trim() ?? string.Empty;
        return cleanText.Length > 120 ? cleanText[..120] : cleanText;
    }

    private void SetScheduleOffset(object? parameter)
    {
        if (TryGetScheduleOffset(parameter, out var offset))
        {
            SelectedScheduleOffset = offset;
        }
    }

    private static bool TryGetScheduleOffset(object? parameter, out int offset)
    {
        offset = parameter switch
        {
            ScheduleOptionViewModel option => option.Offset,
            int intValue => intValue,
            long longValue when longValue is >= 0 and <= MaximumScheduleOffset => (int)longValue,
            string text when int.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsed) => parsed,
            _ => -1
        };

        return offset is >= 0 and <= MaximumScheduleOffset;
    }

    private bool CanRemoveScheduledQuest(object? parameter) =>
        TryGetScheduledQuestId(parameter, out var id) &&
        _state.ScheduledQuests.Any(item => item.Id == id);

    private void RemoveScheduledQuest(object? parameter)
    {
        if (!TryGetScheduledQuestId(parameter, out var id))
        {
            return;
        }

        var scheduledQuest = _state.ScheduledQuests.FirstOrDefault(item => item.Id == id);
        if (scheduledQuest is null)
        {
            return;
        }

        _state.ScheduledQuests.Remove(scheduledQuest);
        SortAndRenumberScheduledQuests();
        RefreshUpcomingQuests();
        Save();
    }

    private static bool TryGetScheduledQuestId(object? parameter, out Guid id)
    {
        id = parameter switch
        {
            UpcomingQuestViewModel item => item.Id,
            ScheduledQuestState state => state.Id,
            Guid guid => guid,
            string text when Guid.TryParse(text, out var parsed) => parsed,
            _ => Guid.Empty
        };

        return id != Guid.Empty;
    }

    private void RemoveItem(object? parameter)
    {
        if (parameter is not ChecklistItem item)
        {
            return;
        }

        item.PropertyChanged -= Item_PropertyChanged;
        Items.Remove(item);
        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
    }

    private void ResetToday()
    {
        _suppressItemPersistence = true;
        try
        {
            foreach (var item in Items)
            {
                item.IsCompleted = false;
            }
        }
        finally
        {
            _suppressItemPersistence = false;
        }

        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
    }

    private void ClearCompleted()
    {
        var completedItems = Items.Where(item => item.IsCompleted).ToList();
        foreach (var item in completedItems)
        {
            item.PropertyChanged -= Item_PropertyChanged;
            Items.Remove(item);
        }

        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
    }

    private void TogglePin()
    {
        AlwaysOnTop = !AlwaysOnTop;
        _state.Settings.AlwaysOnTop = AlwaysOnTop;
        Save();
    }

    private void ToggleLanguage()
    {
        SetLanguage(LanguageCode == UiCopyCatalog.EnglishCode
            ? UiCopyCatalog.IndonesianCode
            : UiCopyCatalog.EnglishCode);
    }

    private void SetLanguage(object? parameter)
    {
        var languageCode = (parameter as string)?.Trim().ToLowerInvariant() switch
        {
            "en" or "en-us" => UiCopyCatalog.EnglishCode,
            "id" or "id-id" => UiCopyCatalog.IndonesianCode,
            _ => null
        };
        if (languageCode is null)
        {
            return;
        }

        if (string.Equals(LanguageCode, languageCode, StringComparison.Ordinal))
        {
            return;
        }

        _state.Settings.LanguageCode = languageCode;

        NotifyLanguageChanged();
        RefreshHistoryEntries();
        Save();
    }

    private void SetTheme(object? parameter)
    {
        if (!ThemeCatalog.TryNormalizeSelection(parameter as string, out var themeCode) ||
            string.Equals(ThemeCode, themeCode, StringComparison.Ordinal))
        {
            return;
        }

        _themeService.Apply(themeCode);
        _state.Settings.ThemeCode = themeCode;
        OnPropertyChanged(nameof(ThemeCode));
        OnPropertyChanged(nameof(IsLightTheme));
        OnPropertyChanged(nameof(IsDarkTheme));
        Save();
    }

    private void ShowToday()
    {
        if (IsTodayView)
        {
            return;
        }

        SetActiveView(isHistory: false, isUpcoming: false, isSettings: false);
    }

    private void ShowHistory()
    {
        RefreshHistoryEntries();
        if (_isHistoryView)
        {
            return;
        }

        SetActiveView(isHistory: true, isUpcoming: false, isSettings: false);
    }

    private void ShowUpcoming()
    {
        RefreshUpcomingQuests();
        if (_isUpcomingView)
        {
            return;
        }

        SetActiveView(isHistory: false, isUpcoming: true, isSettings: false);
    }

    private void ShowSettings()
    {
        RefreshStorageUsage();
        if (_isSettingsView)
        {
            return;
        }

        SetActiveView(isHistory: false, isUpcoming: false, isSettings: true);
    }

    private void SetActiveView(bool isHistory, bool isUpcoming, bool isSettings)
    {
        _isHistoryView = isHistory;
        _isUpcomingView = isUpcoming;
        _isSettingsView = isSettings;
        OnPropertyChanged(nameof(IsTodayView));
        OnPropertyChanged(nameof(IsHistoryView));
        OnPropertyChanged(nameof(IsUpcomingView));
        OnPropertyChanged(nameof(IsSettingsView));
    }

    private void ClearHistory()
    {
        if (_state.History.Count == 0)
        {
            return;
        }

        _state.History.Clear();
        RefreshHistoryEntries();
        Save();
        RefreshStorageUsage();
        RaiseCommandStates();
    }

    private bool ActivateDueScheduledQuests(DateOnly today)
    {
        var dueQuests = _state.ScheduledQuests
            .Select(item => new
            {
                Item = item,
                HasDate = TryParseDateKey(item.ScheduledDate, out var date),
                Date = date
            })
            .Where(entry => entry.HasDate && entry.Date <= today)
            .OrderBy(entry => entry.Date)
            .ThenBy(entry => entry.Item.SortOrder)
            .Select(entry => entry.Item)
            .ToList();
        if (dueQuests.Count == 0)
        {
            return false;
        }

        var occupiedIds = Items
            .Select(item => item.Id)
            .Concat(_state.History.SelectMany(entry => entry.Items.Select(item => item.Id)))
            .ToHashSet();

        foreach (var scheduledQuest in dueQuests)
        {
            var id = scheduledQuest.Id;
            if (id == Guid.Empty || !occupiedIds.Add(id))
            {
                do
                {
                    id = Guid.NewGuid();
                }
                while (!occupiedIds.Add(id));
            }

            var item = new ChecklistItem(
                id,
                scheduledQuest.Text,
                isCompleted: false,
                scheduledQuest.CreatedAt);
            SubscribeToItem(item);
            Items.Add(item);
        }

        var dueIds = dueQuests.Select(item => item.Id).ToHashSet();
        _state.ScheduledQuests.RemoveAll(item => dueIds.Contains(item.Id));
        SortAndRenumberScheduledQuests();
        return true;
    }

    private void SortAndRenumberScheduledQuests()
    {
        _state.ScheduledQuests = _state.ScheduledQuests
            .OrderBy(item => item.ScheduledDate, StringComparer.Ordinal)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.CreatedAt)
            .ToList();

        for (var index = 0; index < _state.ScheduledQuests.Count; index++)
        {
            _state.ScheduledQuests[index].SortOrder = index;
        }
    }

    private void RefreshScheduleOptions(DateOnly? referenceDate = null)
    {
        var today = referenceDate ?? GetLocalDate(_now());
        ScheduleOptions.Clear();

        for (var offset = 0; offset <= MaximumScheduleOffset; offset++)
        {
            var dateText = FormatUpcomingDate(today.AddDays(offset));
            var label = GetScheduleLabel(offset);
            ScheduleOptions.Add(new ScheduleOptionViewModel
            {
                Offset = offset,
                Label = label,
                DateText = dateText,
                DisplayText = string.Format(
                    Copy.Culture,
                    Copy.ScheduleOptionFormat,
                    label,
                    dateText),
                IsSelected = offset == SelectedScheduleOffset
            });
        }

        OnPropertyChanged(nameof(ScheduleOptions));
        OnPropertyChanged(nameof(SelectedScheduleLabel));
    }

    private void RefreshUpcomingQuests(DateOnly? referenceDate = null)
    {
        var today = referenceDate ?? GetLocalDate(_now());
        UpcomingQuests.Clear();

        foreach (var scheduledQuest in _state.ScheduledQuests
                     .OrderBy(item => item.ScheduledDate, StringComparer.Ordinal)
                     .ThenBy(item => item.SortOrder))
        {
            if (!TryParseDateKey(scheduledQuest.ScheduledDate, out var date))
            {
                continue;
            }

            var scheduleLabel = GetScheduleLabel(date.DayNumber - today.DayNumber);
            var dateText = FormatUpcomingDate(date);
            UpcomingQuests.Add(new UpcomingQuestViewModel
            {
                Id = scheduledQuest.Id,
                Text = scheduledQuest.Text,
                ScheduledDate = scheduledQuest.ScheduledDate,
                DateText = dateText,
                ScheduleLabel = scheduleLabel,
                DisplayDate = string.Format(
                    Copy.Culture,
                    Copy.ScheduleOptionFormat,
                    scheduleLabel,
                    dateText)
            });
        }

        OnPropertyChanged(nameof(UpcomingQuests));
        OnPropertyChanged(nameof(HasUpcomingQuests));
        (RemoveScheduledQuestCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private string GetScheduleLabel(int offset) => offset switch
    {
        <= 0 => Copy.ScheduleToday,
        1 => Copy.ScheduleTomorrow,
        _ => string.Format(Copy.Culture, Copy.ScheduleOffsetFormat, offset)
    };

    private string FormatUpcomingDate(DateOnly date)
    {
        var formatted = date
            .ToDateTime(TimeOnly.MinValue)
            .ToString(Copy.UpcomingDateFormat, Copy.Culture);
        return Copy.Culture.TextInfo.ToTitleCase(formatted);
    }

    private void SubscribeToItem(ChecklistItem item) => item.PropertyChanged += Item_PropertyChanged;

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ChecklistItem.IsCompleted) || _suppressItemPersistence)
        {
            return;
        }

        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
    }

    private void SyncHistoryFromActiveDay(bool preserveCompletedOrphans)
    {
        var dateKey = _state.CurrentDate;
        if (!IsValidDateKey(dateKey))
        {
            return;
        }

        var existing = _state.History.FirstOrDefault(entry =>
            string.Equals(entry.Date, dateKey, StringComparison.Ordinal));
        var activeItems = Items
            .Select((item, index) => ToState(item, index))
            .ToList();

        List<ChecklistItemState> snapshotItems;
        if (existing is not null && preserveCompletedOrphans)
        {
            var activeIds = activeItems.Select(item => item.Id).ToHashSet();
            var activeQueue = new Queue<ChecklistItemState>(
                activeItems.Select(CloneItemState));
            snapshotItems = [];

            foreach (var oldItem in existing.Items.OrderBy(item => item.SortOrder))
            {
                if (activeIds.Contains(oldItem.Id) && activeQueue.Count > 0)
                {
                    snapshotItems.Add(activeQueue.Dequeue());
                }
                else if (oldItem.IsCompleted)
                {
                    snapshotItems.Add(CloneItemState(oldItem));
                }
            }

            while (activeQueue.Count > 0)
            {
                snapshotItems.Add(activeQueue.Dequeue());
            }
        }
        else
        {
            snapshotItems = activeItems.Select(CloneItemState).ToList();
        }

        for (var index = 0; index < snapshotItems.Count; index++)
        {
            snapshotItems[index].SortOrder = index;
        }

        if (snapshotItems.Count == 0)
        {
            if (existing is not null)
            {
                _state.History.Remove(existing);
            }
        }
        else if (existing is null)
        {
            _state.History.Add(new DailyHistoryState
            {
                Date = dateKey,
                Items = snapshotItems
            });
        }
        else
        {
            existing.Items = snapshotItems;
        }

        _state.History = _state.History
            .OrderByDescending(entry => entry.Date, StringComparer.Ordinal)
            .ToList();
    }

    private void RefreshHistoryEntries()
    {
        HistoryEntries.Clear();

        foreach (var entry in _state.History
                     .Where(entry => entry.Items.Count > 0 && IsValidDateKey(entry.Date))
                     .OrderByDescending(entry => entry.Date, StringComparer.Ordinal))
        {
            var date = DateTime.ParseExact(
                entry.Date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None);
            var orderedItems = entry.Items
                .OrderBy(item => item.SortOrder)
                .ToList();
            var completedCount = orderedItems.Count(item => item.IsCompleted);
            var totalCount = orderedItems.Count;

            HistoryEntries.Add(new HistoryEntryViewModel
            {
                DateText = FormatDate(date, includeYear: true),
                SummaryText = string.Format(
                    Copy.Culture,
                    Copy.HistorySummaryFormat,
                    completedCount,
                    totalCount),
                ProgressPercent = totalCount == 0
                    ? 0
                    : (double)completedCount / totalCount * 100,
                Items = orderedItems
                    .Select(item => new HistoryItemViewModel
                    {
                        Text = item.Text,
                        IsCompleted = item.IsCompleted
                    })
                    .ToList()
            });
        }

        OnPropertyChanged(nameof(HistoryEntries));
    }

    private string FormatDate(DateTime value, bool includeYear)
    {
        var format = includeYear ? "dddd, d MMMM yyyy" : "dddd, d MMMM";
        var formatted = value.ToString(format, Copy.Culture);
        return Copy.Culture.TextInfo.ToTitleCase(formatted);
    }

    private AppState CreateSnapshot() => new()
    {
        SchemaVersion = CurrentSchemaVersion,
        CurrentDate = string.IsNullOrWhiteSpace(_state.CurrentDate)
            ? GetDateKey(_now())
            : _state.CurrentDate,
        Items = Items.Select((item, index) => ToState(item, index)).ToList(),
        History = _state.History
            .Select(entry => new DailyHistoryState
            {
                Date = entry.Date,
                Items = entry.Items.Select(CloneItemState).ToList()
            })
            .ToList(),
        ScheduledQuests = _state.ScheduledQuests
            .Select(CloneScheduledQuestState)
            .ToList(),
        Window = new WidgetWindowState
        {
            Left = _state.Window.Left,
            Top = _state.Window.Top,
            Width = _state.Window.Width,
            Height = _state.Window.Height
        },
        Settings = new AppSettings
        {
            AlwaysOnTop = AlwaysOnTop,
            LanguageCode = LanguageCode,
            ThemeCode = ThemeCode
        }
    };

    private static ChecklistItemState ToState(ChecklistItem item, int index) => new()
    {
        Id = item.Id,
        Text = item.Text,
        IsCompleted = item.IsCompleted,
        SortOrder = index,
        CreatedAt = item.CreatedAt
    };

    private static ChecklistItemState CloneItemState(ChecklistItemState item) => new()
    {
        Id = item.Id,
        Text = item.Text,
        IsCompleted = item.IsCompleted,
        SortOrder = item.SortOrder,
        CreatedAt = item.CreatedAt
    };

    private static ScheduledQuestState CloneScheduledQuestState(ScheduledQuestState item) => new()
    {
        Id = item.Id,
        Text = item.Text,
        ScheduledDate = item.ScheduledDate,
        SortOrder = item.SortOrder,
        CreatedAt = item.CreatedAt
    };

    private void NotifyProgressChanged()
    {
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(RemainingCount));
        OnPropertyChanged(nameof(ProgressPercent));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(EncouragementText));
        OnPropertyChanged(nameof(IsAllDone));
        OnPropertyChanged(nameof(NextPendingItem));
        OnPropertyChanged(nameof(HasPendingItem));
        RaiseCommandStates();
    }

    private void NotifyLanguageChanged()
    {
        OnPropertyChanged(nameof(Copy));
        OnPropertyChanged(nameof(LanguageCode));
        OnPropertyChanged(nameof(LanguageBadge));
        OnPropertyChanged(nameof(IsIndonesian));
        OnPropertyChanged(nameof(IsEnglish));
        OnPropertyChanged(nameof(PinTooltip));
        OnPropertyChanged(nameof(Greeting));
        OnPropertyChanged(nameof(FriendlyDate));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(EncouragementText));
        OnPropertyChanged(nameof(FooterText));
        RefreshScheduleOptions();
        RefreshUpcomingQuests();
    }

    private void RaiseCommandStates()
    {
        (AddItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetTodayCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ClearCompletedCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ClearHistoryCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RemoveScheduledQuestCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void RefreshStorageUsage()
    {
        try
        {
            var usage = _storageUsageService.Measure(_state.History);
            ApplicationStorageText = FormatBytes(usage.ApplicationBytes);
            DataStorageText = FormatBytes(usage.DataBytes);
            HistoryStorageText = FormatBytes(usage.HistoryBytes);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or JsonException)
        {
            ApplicationStorageText = "—";
            DataStorageText = "—";
            HistoryStorageText = "—";
        }
    }

    private static string FormatBytes(long bytes)
    {
        var safeBytes = Math.Max(0, bytes);
        string[] units = ["B", "KB", "MB", "GB"];
        var value = (double)safeBytes;
        var unitIndex = 0;

        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return unitIndex == 0
            ? $"{safeBytes} {units[unitIndex]}"
            : $"{value:0.#} {units[unitIndex]}";
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
