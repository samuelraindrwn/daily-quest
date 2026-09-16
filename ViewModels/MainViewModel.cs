using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DailyQuest.Infrastructure;
using DailyQuest.Localization;
using DailyQuest.Models;
using DailyQuest.Services;

namespace DailyQuest.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const int CurrentSchemaVersion = 2;
    private readonly IStateStore _stateStore;
    private readonly Func<DateTimeOffset> _now;
    private readonly AppState _state;
    private string _newItemText = string.Empty;
    private bool _alwaysOnTop;
    private bool _isHistoryView;
    private bool _suppressItemPersistence;

    public MainViewModel(IStateStore? stateStore = null, Func<DateTimeOffset>? now = null)
    {
        _stateStore = stateStore ?? new JsonStateStore();
        _now = now ?? (() => DateTimeOffset.Now);

        var loadedState = _stateStore.Load();
        var isFirstRun = loadedState is null;
        _state = loadedState ?? CreateFirstRunState(_now());

        var wasLegacySchema = _state.SchemaVersion < CurrentSchemaVersion;
        var didNormalize = NormalizeState();
        Items = new ObservableCollection<ChecklistItem>(
            _state.Items
                .OrderBy(item => item.SortOrder)
                .Select(item => new ChecklistItem(
                    item.Id,
                    item.Text,
                    item.IsCompleted,
                    item.CreatedAt)));
        HistoryEntries = [];

        foreach (var item in Items)
        {
            SubscribeToItem(item);
        }

        _alwaysOnTop = _state.Settings.AlwaysOnTop;

        AddItemCommand = new RelayCommand(AddItem, CanAddItem);
        RemoveItemCommand = new RelayCommand(RemoveItem, parameter => parameter is ChecklistItem);
        ResetTodayCommand = new RelayCommand(ResetToday, () => CompletedCount > 0);
        ClearCompletedCommand = new RelayCommand(ClearCompleted, () => CompletedCount > 0);
        TogglePinCommand = new RelayCommand(TogglePin);
        ToggleLanguageCommand = new RelayCommand(ToggleLanguage);
        ShowTodayCommand = new RelayCommand(ShowToday);
        ShowHistoryCommand = new RelayCommand(ShowHistory);

        if (wasLegacySchema && !string.IsNullOrWhiteSpace(_state.CurrentDate))
        {
            SyncHistoryFromActiveDay(preserveCompletedOrphans: false);
        }

        var didRollOver = RollOverToCurrentDay(saveAfterReset: false);
        RefreshHistoryEntries();

        if (isFirstRun || didNormalize || didRollOver)
        {
            Save();
        }
    }

    public ObservableCollection<ChecklistItem> Items { get; }

    public ObservableCollection<HistoryEntryViewModel> HistoryEntries { get; }

    public ICommand AddItemCommand { get; }

    public ICommand RemoveItemCommand { get; }

    public ICommand ResetTodayCommand { get; }

    public ICommand ClearCompletedCommand { get; }

    public ICommand TogglePinCommand { get; }

    public ICommand ToggleLanguageCommand { get; }

    public ICommand ShowTodayCommand { get; }

    public ICommand ShowHistoryCommand { get; }

    public WidgetWindowState SavedWindow => _state.Window;

    public UiCopy Copy => UiCopyCatalog.For(_state.Settings.LanguageCode);

    public string LanguageCode => _state.Settings.LanguageCode;

    public string LanguageBadge => LanguageCode == UiCopyCatalog.EnglishCode ? "EN" : "ID";

    public bool IsTodayView => !_isHistoryView;

    public bool IsHistoryView => _isHistoryView;

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
        var todayKey = GetDateKey(_now());
        if (string.Equals(_state.CurrentDate, todayKey, StringComparison.Ordinal))
        {
            OnPropertyChanged(nameof(Greeting));
            OnPropertyChanged(nameof(FriendlyDate));
            return false;
        }

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
        SyncHistoryFromActiveDay(preserveCompletedOrphans: false);
        NotifyProgressChanged();
        RefreshHistoryEntries();

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

    public void Save() => _stateStore.Save(CreateSnapshot());

    private static AppState CreateFirstRunState(DateTimeOffset now) => new()
    {
        SchemaVersion = CurrentSchemaVersion,
        CurrentDate = GetDateKey(now),
        Items = [],
        History = [],
        Settings = new AppSettings
        {
            AlwaysOnTop = true,
            LanguageCode = UiCopyCatalog.IndonesianCode
        }
    };

    private static string GetDateKey(DateTimeOffset value) =>
        value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

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

        var normalizedLanguage = UiCopyCatalog.NormalizeLanguageCode(_state.Settings.LanguageCode);
        if (!string.Equals(_state.Settings.LanguageCode, normalizedLanguage, StringComparison.Ordinal))
        {
            _state.Settings.LanguageCode = normalizedLanguage;
            needsSave = true;
        }

        _state.Window.Width = Math.Clamp(_state.Window.Width, 340, 760);
        _state.Window.Height = Math.Clamp(_state.Window.Height, 460, 1000);
        _state.SchemaVersion = CurrentSchemaVersion;
        return needsSave;
    }

    private List<ChecklistItemState> NormalizeChecklistStates(IEnumerable<ChecklistItemState> states)
    {
        var seenIds = new HashSet<Guid>();
        var result = new List<ChecklistItemState>();

        foreach (var state in states.Where(state => state is not null && !string.IsNullOrWhiteSpace(state.Text)))
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

    private static bool IsValidDateKey(string? value) =>
        DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);

    private void AddItem()
    {
        var cleanText = NewItemText.Trim();
        if (cleanText.Length == 0)
        {
            return;
        }

        if (cleanText.Length > 120)
        {
            cleanText = cleanText[..120];
        }

        var item = new ChecklistItem(Guid.NewGuid(), cleanText, false, _now());
        SubscribeToItem(item);
        Items.Add(item);
        NewItemText = string.Empty;

        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
    }

    private bool CanAddItem() => !string.IsNullOrWhiteSpace(NewItemText);

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
        _state.Settings.LanguageCode = LanguageCode == UiCopyCatalog.EnglishCode
            ? UiCopyCatalog.IndonesianCode
            : UiCopyCatalog.EnglishCode;

        NotifyLanguageChanged();
        RefreshHistoryEntries();
        Save();
    }

    private void ShowToday()
    {
        if (!_isHistoryView)
        {
            return;
        }

        _isHistoryView = false;
        OnPropertyChanged(nameof(IsTodayView));
        OnPropertyChanged(nameof(IsHistoryView));
    }

    private void ShowHistory()
    {
        RefreshHistoryEntries();
        if (_isHistoryView)
        {
            return;
        }

        _isHistoryView = true;
        OnPropertyChanged(nameof(IsTodayView));
        OnPropertyChanged(nameof(IsHistoryView));
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
            var activeById = activeItems.ToDictionary(item => item.Id);
            snapshotItems = [];

            foreach (var oldItem in existing.Items.OrderBy(item => item.SortOrder))
            {
                if (activeById.Remove(oldItem.Id, out var activeItem))
                {
                    snapshotItems.Add(CloneItemState(activeItem));
                }
                else if (oldItem.IsCompleted)
                {
                    snapshotItems.Add(CloneItemState(oldItem));
                }
            }

            foreach (var activeItem in activeItems.Where(item => activeById.ContainsKey(item.Id)))
            {
                snapshotItems.Add(CloneItemState(activeItem));
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
            LanguageCode = LanguageCode
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

    private void NotifyProgressChanged()
    {
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(RemainingCount));
        OnPropertyChanged(nameof(ProgressPercent));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(EncouragementText));
        OnPropertyChanged(nameof(IsAllDone));
        RaiseCommandStates();
    }

    private void NotifyLanguageChanged()
    {
        OnPropertyChanged(nameof(Copy));
        OnPropertyChanged(nameof(LanguageCode));
        OnPropertyChanged(nameof(LanguageBadge));
        OnPropertyChanged(nameof(PinTooltip));
        OnPropertyChanged(nameof(Greeting));
        OnPropertyChanged(nameof(FriendlyDate));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(EncouragementText));
    }

    private void RaiseCommandStates()
    {
        (AddItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetTodayCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ClearCompletedCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
