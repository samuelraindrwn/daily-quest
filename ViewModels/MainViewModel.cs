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
    private const int CurrentSchemaVersion = 8;
    private const int MaximumScheduleOffset = 8;
    private const int MaximumTimerDurationMinutes = 480;
    private const int MaximumQuestTextLength = 120;
    private const int MaximumQuestLabels = 12;
    private const int MaximumQuestLabelNameLength = 24;
    private const string DefaultLabelColorHex = "#5B8A72";
    private static readonly Guid ImportantLabelId = Guid.Parse("D9110000-0000-4000-8000-000000000001");
    private static readonly Guid PersonalLabelId = Guid.Parse("D9110000-0000-4000-8000-000000000002");
    private static readonly Guid RoutineLabelId = Guid.Parse("D9110000-0000-4000-8000-000000000003");
    private static readonly string AppVersion =
        typeof(MainViewModel).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

    private readonly IStateStore _stateStore;
    private readonly IStorageUsageService _storageUsageService;
    private readonly IThemeService _themeService;
    private readonly IQuestAlarmService? _alarmService;
    private readonly IRunAtStartupService _runAtStartupService;
    private readonly Func<DateTimeOffset> _now;
    private readonly AppState _state;
    private readonly Dictionary<Guid, int> _manualSortOrders = [];
    private string _newItemText = string.Empty;
    private bool _alwaysOnTop;
    private bool _isHistoryView;
    private bool _isUpcomingView;
    private bool _isSettingsView;
    private int _selectedScheduleOffset;
    private int? _selectedDurationMinutes;
    private Guid? _selectedLabelId;
    private QuestSortMode _sortMode;
    private Guid? _alarmingQuestId;
    private bool _suppressItemPersistence;
    private string _applicationStorageText = "0 B";
    private string _dataStorageText = "0 B";
    private string _historyStorageText = "0 B";

    public MainViewModel(
        IStateStore? stateStore = null,
        Func<DateTimeOffset>? now = null,
        IStorageUsageService? storageUsageService = null,
        IThemeService? themeService = null,
        IQuestAlarmService? alarmService = null,
        IRunAtStartupService? runAtStartupService = null)
    {
        _stateStore = stateStore ?? new JsonStateStore();
        _storageUsageService = storageUsageService ?? new StorageUsageService();
        _themeService = themeService ?? NullThemeService.Instance;
        _alarmService = alarmService;
        _runAtStartupService = runAtStartupService ?? NullRunAtStartupService.Instance;
        _now = now ?? (() => DateTimeOffset.Now);

        var loadedState = _stateStore.Load();
        var isFirstRun = loadedState is null;
        _state = loadedState ?? CreateFirstRunState(_now());

        var needsV1HistoryMigration = _state.SchemaVersion < 2;
        var didNormalize = NormalizeState();
        _themeService.Apply(_state.Settings.ThemeCode);
        TryApplyRunAtStartup(_state.Settings.RunAtStartup);
        _sortMode = QuestSortModeCodes.Parse(_state.Settings.QuestSortMode);
        Labels = new ObservableCollection<QuestLabelViewModel>(
            _state.Labels
                .OrderBy(label => label.SortOrder)
                .Select(ToLabelViewModel));
        foreach (var itemState in _state.Items.OrderBy(item => item.SortOrder))
        {
            _manualSortOrders[itemState.Id] = itemState.ManualSortOrder ?? itemState.SortOrder;
        }

        Items = new ObservableCollection<ChecklistItem>(
            _state.Items
                .OrderBy(item => item.SortOrder)
                .Select(item =>
                {
                    var label = FindLabel(item.LabelId);
                    return new ChecklistItem(
                        item.Id,
                        item.Text,
                        item.IsCompleted,
                        item.CreatedAt,
                        item.PlannedDurationMinutes,
                        item.RemainingSeconds,
                        item.TimerStartedAt,
                        label?.Id,
                        label?.Name,
                        label?.ColorHex,
                        item.IsOvertime,
                        item.OvertimeSeconds);
                }));
        HistoryEntries = [];
        UpcomingQuests = [];
        ScheduleOptions = [];
        DurationOptions = [0, 5, 10, 15, 25, 30, 45, 60];

        foreach (var item in Items)
        {
            SubscribeToItem(item);
        }

        _alwaysOnTop = _state.Settings.AlwaysOnTop;

        AddItemCommand = new RelayCommand(AddItem, CanAddItem);
        CopyItemCommand = new RelayCommand(CopyItem, CanCopyItem);
        CopyScheduleDayCommand = new RelayCommand(CopyScheduleDay, CanCopyScheduleDay);
        RemoveItemCommand = new RelayCommand(RemoveItem, parameter => parameter is ChecklistItem);
        CompleteItemCommand = new RelayCommand(CompleteItem);
        RemoveScheduledQuestCommand = new RelayCommand(
            RemoveScheduledQuest,
            CanRemoveScheduledQuest);
        ResetTodayCommand = new RelayCommand(ResetToday, () => CompletedCount > 0);
        ClearCompletedCommand = new RelayCommand(ClearCompleted, () => CompletedCount > 0);
        TogglePinCommand = new RelayCommand(TogglePin);
        ToggleLanguageCommand = new RelayCommand(ToggleLanguage);
        SetLanguageCommand = new RelayCommand(SetLanguage);
        SetThemeCommand = new RelayCommand(SetTheme);
        SetRunAtStartupCommand = new RelayCommand(
            SetRunAtStartup,
            parameter => TryGetBoolean(parameter, out _));
        SetScheduleOffsetCommand = new RelayCommand(
            SetScheduleOffset,
            parameter => TryGetScheduleOffset(parameter, out _));
        SetDurationCommand = new RelayCommand(SetDuration);
        SetSelectedLabelCommand = new RelayCommand(SetSelectedLabel);
        AssignItemLabelCommand = new RelayCommand(AssignItemLabel, CanAssignItemLabel);
        AddLabelCommand = new RelayCommand(AddLabel, CanAddLabel);
        UpdateLabelCommand = new RelayCommand(UpdateLabel, CanUpdateLabel);
        DeleteLabelCommand = new RelayCommand(DeleteLabel, CanDeleteLabel);
        MoveLabelUpCommand = new RelayCommand(MoveLabelUp, CanMoveLabelUp);
        MoveLabelDownCommand = new RelayCommand(MoveLabelDown, CanMoveLabelDown);
        SetSortModeCommand = new RelayCommand(SetSortMode, CanSetSortMode);
        ToggleTimerCommand = new RelayCommand(ToggleTimer, CanToggleTimer);
        ResetTimerCommand = new RelayCommand(ResetTimer, CanResetTimer);
        StartOvertimeCommand = new RelayCommand(StartOvertime, CanStartOvertime);
        SetOvertimeCommand = new RelayCommand(SetOvertime, CanSetOvertime);
        ToggleOvertimeCommand = new RelayCommand(ToggleOvertime);
        ShowTodayCommand = new RelayCommand(ShowToday);
        ShowHistoryCommand = new RelayCommand(ShowHistory);
        ShowUpcomingCommand = new RelayCommand(ShowUpcoming);
        ShowSettingsCommand = new RelayCommand(ShowSettings);
        ClearHistoryCommand = new RelayCommand(ClearHistory, () => _state.History.Count > 0);

        var didApplySort = ApplyQuestSort();

        if (needsV1HistoryMigration && !string.IsNullOrWhiteSpace(_state.CurrentDate))
        {
            SyncHistoryFromActiveDay(preserveCompletedOrphans: false);
        }

        var didRollOver = RollOverToCurrentDay(saveAfterReset: false);
        RefreshHistoryEntries();
        RefreshScheduleOptions();
        RefreshUpcomingQuests();

        if (isFirstRun || didNormalize || didRollOver || didApplySort)
        {
            Save();
        }
    }

    public ObservableCollection<ChecklistItem> Items { get; }

    public ObservableCollection<QuestLabelViewModel> Labels { get; }

    public ObservableCollection<HistoryEntryViewModel> HistoryEntries { get; }

    public ObservableCollection<UpcomingQuestViewModel> UpcomingQuests { get; }

    public ObservableCollection<ScheduleOptionViewModel> ScheduleOptions { get; }

    public IReadOnlyList<int> DurationOptions { get; }

    public ICommand AddItemCommand { get; }

    public ICommand CopyItemCommand { get; }

    public ICommand CopyScheduleDayCommand { get; }

    public ICommand RemoveItemCommand { get; }

    public ICommand CompleteItemCommand { get; }

    public ICommand RemoveScheduledQuestCommand { get; }

    public ICommand ResetTodayCommand { get; }

    public ICommand ClearCompletedCommand { get; }

    public ICommand TogglePinCommand { get; }

    public ICommand ToggleLanguageCommand { get; }

    public ICommand SetLanguageCommand { get; }

    public ICommand SetThemeCommand { get; }

    public ICommand SetRunAtStartupCommand { get; }

    public ICommand SetScheduleOffsetCommand { get; }

    public ICommand SetDurationCommand { get; }

    public ICommand SetSelectedLabelCommand { get; }

    public ICommand AssignItemLabelCommand { get; }

    public ICommand AddLabelCommand { get; }

    public ICommand UpdateLabelCommand { get; }

    public ICommand DeleteLabelCommand { get; }

    public ICommand MoveLabelUpCommand { get; }

    public ICommand MoveLabelDownCommand { get; }

    public ICommand SetSortModeCommand { get; }

    public ICommand ToggleTimerCommand { get; }

    public ICommand ResetTimerCommand { get; }

    public ICommand StartOvertimeCommand { get; }

    public ICommand SetOvertimeCommand { get; }

    public ICommand ToggleOvertimeCommand { get; }

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

    public bool RunAtStartup => _state.Settings.RunAtStartup;

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

    public int? SelectedDurationMinutes
    {
        get => _selectedDurationMinutes;
        set
        {
            if ((value.HasValue &&
                 (value.Value < 1 || value.Value > MaximumTimerDurationMinutes)) ||
                !SetField(ref _selectedDurationMinutes, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasSelectedDuration));
            OnPropertyChanged(nameof(SelectedDurationLabel));
        }
    }

    public bool HasSelectedDuration => SelectedDurationMinutes.HasValue;

    public string SelectedDurationLabel => SelectedDurationMinutes.HasValue
        ? string.Format(
            Copy.Culture,
            Copy.DurationMinutesFormat,
            SelectedDurationMinutes.Value)
        : Copy.NoTimer;

    public int MaximumDurationMinutes => MaximumTimerDurationMinutes;

    public int MaximumLabelCount => MaximumQuestLabels;

    public int MaximumLabelNameLength => MaximumQuestLabelNameLength;

    public bool CanAddMoreLabels => Labels.Count < MaximumQuestLabels;

    public string LabelCountText => string.Format(
        Copy.Culture,
        Copy.LabelLimitFormat,
        Labels.Count,
        MaximumQuestLabels);

    public Guid? SelectedLabelId
    {
        get => _selectedLabelId;
        set
        {
            var normalized = value.HasValue && FindLabel(value) is not null ? value : null;
            if (!SetField(ref _selectedLabelId, normalized))
            {
                return;
            }

            foreach (var label in Labels)
            {
                label.IsSelected = label.Id == normalized;
            }

            OnPropertyChanged(nameof(SelectedLabel));
            OnPropertyChanged(nameof(HasSelectedLabel));
            OnPropertyChanged(nameof(SelectedLabelName));
            OnPropertyChanged(nameof(SelectedLabelColorHex));
        }
    }

    public QuestLabelViewModel? SelectedLabel => Labels.FirstOrDefault(label => label.Id == SelectedLabelId);

    public bool HasSelectedLabel => SelectedLabelId.HasValue;

    public string? SelectedLabelName => SelectedLabel?.Name;

    public string? SelectedLabelColorHex => SelectedLabel?.ColorHex;

    public QuestSortMode SortMode => _sortMode;

    public string SortModeCode => QuestSortModeCodes.ToCode(SortMode);

    public IReadOnlyList<QuestSortMode> SortModes { get; } =
    [
        QuestSortMode.Manual,
        QuestSortMode.Label,
        QuestSortMode.DurationAscending,
        QuestSortMode.DurationDescending
    ];

    public bool IsManualSort => SortMode == QuestSortMode.Manual;

    public bool IsLabelSort => SortMode == QuestSortMode.Label;

    public bool IsDurationAscendingSort => SortMode == QuestSortMode.DurationAscending;

    public bool IsDurationDescendingSort => SortMode == QuestSortMode.DurationDescending;

    public bool OvertimeEnabled => _state.Settings.OvertimeEnabled;

    public ChecklistItem? NextPendingItem => Items.FirstOrDefault(item => !item.IsCompleted);

    public bool HasPendingItem => NextPendingItem is not null;

    public ChecklistItem? ActiveTimerItem => Items.FirstOrDefault(item => item.IsTimerRunning);

    public bool HasActiveTimer => ActiveTimerItem is not null;

    public ChecklistItem? CompactDisplayItem => ActiveTimerItem ?? NextPendingItem;

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
        TickTimers(now);
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
                StopTimerAlarm();

                // A completed quest belongs only to the day on which it was
                // completed. Its archived snapshot stays in history, while only
                // unfinished quests remain active for the new day.
                foreach (var completedItem in Items
                             .Where(item => item.IsCompleted)
                             .ToList())
                {
                    completedItem.PropertyChanged -= Item_PropertyChanged;
                    Items.Remove(completedItem);
                    RemoveFromManualOrder(completedItem.Id);
                }

                foreach (var item in Items)
                {
                    if (item.IsOvertime || !item.IsTimerRunning)
                    {
                        item.ResetTimer();
                    }
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

        ApplyQuestSort();
        SyncHistoryFromActiveDay(preserveCompletedOrphans: !dateChanged);
        NotifyTimerStateChanged();
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
        if (!IsManualSort || sourceIndex < 0 || Items.Count < 2)
        {
            return false;
        }

        destinationIndex = Math.Clamp(destinationIndex, 0, Items.Count - 1);
        var pendingCount = Items.Count(candidate => !candidate.IsCompleted);
        destinationIndex = item.IsCompleted
            ? Math.Clamp(destinationIndex, pendingCount, Items.Count - 1)
            : Math.Clamp(destinationIndex, 0, Math.Max(0, pendingCount - 1));
        if (sourceIndex == destinationIndex)
        {
            return false;
        }

        Items.Move(sourceIndex, destinationIndex);
        CaptureManualOrder();
        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        OnPropertyChanged(nameof(NextPendingItem));
        OnPropertyChanged(nameof(HasPendingItem));
        OnPropertyChanged(nameof(CompactDisplayItem));
        RefreshHistoryEntries();
        Save();
        return true;
    }

    public bool TryUpdateItemText(ChecklistItem? item, string? text)
        => TryUpdateItem(item, text, item?.PlannedDurationMinutes);

    public bool TryUpdateItem(
        ChecklistItem? item,
        string? text,
        int? plannedDurationMinutes)
    {
        if (item is null ||
            !Items.Contains(item) ||
            plannedDurationMinutes is not null and (< 1 or > MaximumTimerDurationMinutes))
        {
            return false;
        }

        var cleanText = NormalizeQuestText(text);
        if (cleanText.Length == 0)
        {
            return false;
        }

        var textChanged = item.UpdateText(cleanText);
        var timerChanged = item.UpdateTimerPlan(plannedDurationMinutes);
        if (!textChanged && !timerChanged)
        {
            return true;
        }

        if (timerChanged)
        {
            StopTimerAlarm(item.Id);
            ApplyQuestSort();
            NotifyTimerStateChanged();
        }

        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        RefreshHistoryEntries();
        Save();
        return true;
    }

    public bool CopyItemTo(ChecklistItem item, int offset)
    {
        if (!Items.Contains(item) || offset is < 0 or > MaximumScheduleOffset)
        {
            return false;
        }

        // Resolve a possible midnight rollover before deriving the target date.
        // A completed source can leave the active list during that rollover.
        var didRollOver = RollOverToCurrentDay(saveAfterReset: false);
        if (!Items.Contains(item))
        {
            if (didRollOver)
            {
                Save();
            }

            return false;
        }

        var now = _now();
        var label = FindLabel(item.LabelId);
        if (offset > 0)
        {
            _state.ScheduledQuests.Add(new ScheduledQuestState
            {
                Id = Guid.NewGuid(),
                Text = item.Text,
                ScheduledDate = GetDateKey(GetLocalDate(now).AddDays(offset)),
                SortOrder = _state.ScheduledQuests.Count,
                CreatedAt = now,
                PlannedDurationMinutes = item.PlannedDurationMinutes,
                LabelId = label?.Id
            });
            SortAndRenumberScheduledQuests();
            RefreshUpcomingQuests();
            Save();
            return true;
        }

        var copy = new ChecklistItem(
            Guid.NewGuid(),
            item.Text,
            false,
            now,
            item.PlannedDurationMinutes,
            labelId: label?.Id,
            labelName: label?.Name,
            labelColorHex: label?.ColorHex);
        SubscribeToItem(copy);
        Items.Add(copy);
        AddToManualOrder(copy);
        ApplyQuestSort();
        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
        return true;
    }

    public int CopyItemToAllFutureDays(ChecklistItem item)
    {
        if (!Items.Contains(item))
        {
            return 0;
        }

        var didRollOver = RollOverToCurrentDay(saveAfterReset: false);
        if (!Items.Contains(item))
        {
            if (didRollOver)
            {
                Save();
            }

            return 0;
        }

        var now = _now();
        var today = GetLocalDate(now);
        var label = FindLabel(item.LabelId);
        for (var offset = 1; offset <= MaximumScheduleOffset; offset++)
        {
            _state.ScheduledQuests.Add(new ScheduledQuestState
            {
                Id = Guid.NewGuid(),
                Text = item.Text,
                ScheduledDate = GetDateKey(today.AddDays(offset)),
                SortOrder = _state.ScheduledQuests.Count,
                CreatedAt = now,
                PlannedDurationMinutes = item.PlannedDurationMinutes,
                LabelId = label?.Id
            });
        }

        SortAndRenumberScheduledQuests();
        RefreshUpcomingQuests(today);
        Save();
        return MaximumScheduleOffset;
    }

    public int CopyScheduleDayTo(int sourceOffset, int targetOffset)
    {
        if (sourceOffset is < 0 or > MaximumScheduleOffset ||
            targetOffset is < 0 or > MaximumScheduleOffset)
        {
            return 0;
        }

        var didRollOver = RollOverToCurrentDay(saveAfterReset: false);
        var now = _now();
        var today = GetLocalDate(now);
        var definitions = GetScheduleDayCopyDefinitions(sourceOffset, today);
        if (definitions.Count == 0)
        {
            if (didRollOver)
            {
                Save();
            }

            return 0;
        }

        if (targetOffset > 0)
        {
            var targetDateKey = GetDateKey(today.AddDays(targetOffset));
            foreach (var definition in definitions)
            {
                var label = FindLabel(definition.LabelId);
                _state.ScheduledQuests.Add(new ScheduledQuestState
                {
                    Id = Guid.NewGuid(),
                    Text = definition.Text,
                    ScheduledDate = targetDateKey,
                    SortOrder = _state.ScheduledQuests.Count,
                    CreatedAt = now,
                    PlannedDurationMinutes = definition.PlannedDurationMinutes,
                    LabelId = label?.Id
                });
            }

            SortAndRenumberScheduledQuests();
            RefreshUpcomingQuests(today);
            Save();
            return definitions.Count;
        }

        foreach (var definition in definitions)
        {
            var label = FindLabel(definition.LabelId);
            var copy = new ChecklistItem(
                Guid.NewGuid(),
                definition.Text,
                false,
                now,
                definition.PlannedDurationMinutes,
                labelId: label?.Id,
                labelName: label?.Name,
                labelColorHex: label?.ColorHex);
            SubscribeToItem(copy);
            Items.Add(copy);
            AddToManualOrder(copy);
        }

        ApplyQuestSort();
        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
        return definitions.Count;
    }

    public int CopyScheduleToAllFutureDays(int sourceOffset)
    {
        if (sourceOffset is < 0 or > MaximumScheduleOffset)
        {
            return 0;
        }

        var didRollOver = RollOverToCurrentDay(saveAfterReset: false);
        var now = _now();
        var today = GetLocalDate(now);
        var definitions = GetScheduleDayCopyDefinitions(sourceOffset, today);
        if (definitions.Count == 0)
        {
            if (didRollOver)
            {
                Save();
            }

            return 0;
        }

        for (var targetOffset = 1; targetOffset <= MaximumScheduleOffset; targetOffset++)
        {
            var targetDateKey = GetDateKey(today.AddDays(targetOffset));
            foreach (var definition in definitions)
            {
                var label = FindLabel(definition.LabelId);
                _state.ScheduledQuests.Add(new ScheduledQuestState
                {
                    Id = Guid.NewGuid(),
                    Text = definition.Text,
                    ScheduledDate = targetDateKey,
                    SortOrder = _state.ScheduledQuests.Count,
                    CreatedAt = now,
                    PlannedDurationMinutes = definition.PlannedDurationMinutes,
                    LabelId = label?.Id
                });
            }
        }

        SortAndRenumberScheduledQuests();
        RefreshUpcomingQuests(today);
        Save();
        return definitions.Count * MaximumScheduleOffset;
    }

    public bool HasScheduleDayQuests(int sourceOffset)
    {
        if (sourceOffset is < 0 or > MaximumScheduleOffset)
        {
            return false;
        }

        if (sourceOffset == 0)
        {
            return Items.Count > 0;
        }

        var sourceDateKey = GetDateKey(GetLocalDate(_now()).AddDays(sourceOffset));
        return _state.ScheduledQuests.Any(item =>
            string.Equals(item.ScheduledDate, sourceDateKey, StringComparison.Ordinal));
    }

    public bool TickTimers() => TickTimers(_now());

    public void Save() => _stateStore.Save(CreateSnapshot());

    public void PauseTimersForShutdown()
    {
        var now = _now();
        var changed = false;

        foreach (var item in Items.Where(item => item.IsTimerRunning).ToList())
        {
            var result = item.AdvanceTimer(now);
            var paused = item.IsTimerRunning && item.PauseTimer();
            changed |= result != TimerAdvanceResult.None || paused;
        }

        StopTimerAlarm();
        if (changed)
        {
            NotifyTimerStateChanged();
            Save();
        }
    }

    private bool ApplyQuestSort()
    {
        if (Items.Count < 2)
        {
            return false;
        }

        var labelOrder = _state.Labels
            .Select((label, index) => new { label.Id, Index = index })
            .ToDictionary(entry => entry.Id, entry => entry.Index);
        var indexedItems = Items
            .Select((item, index) => new { Item = item, OriginalIndex = index });

        var targetOrder = indexedItems
            .OrderBy(entry => entry.Item.IsCompleted ? 1 : 0)
            .ThenBy(entry => SortMode switch
            {
                QuestSortMode.Label => entry.Item.LabelId.HasValue &&
                                       labelOrder.ContainsKey(entry.Item.LabelId.Value) ? 0 : 1,
                QuestSortMode.DurationAscending or QuestSortMode.DurationDescending =>
                    entry.Item.PlannedDurationMinutes.HasValue ? 0 : 1,
                _ => 0
            })
            .ThenBy(entry => SortMode switch
            {
                QuestSortMode.Label when entry.Item.LabelId.HasValue &&
                                         labelOrder.TryGetValue(entry.Item.LabelId.Value, out var index) => index,
                QuestSortMode.Label => int.MaxValue,
                QuestSortMode.DurationAscending => entry.Item.PlannedDurationMinutes ?? int.MaxValue,
                QuestSortMode.DurationDescending => -(entry.Item.PlannedDurationMinutes ?? 0),
                _ => 0
            })
            .ThenBy(entry => GetManualSortOrder(entry.Item, entry.OriginalIndex))
            .Select(entry => entry.Item)
            .ToList();
        var changed = !Items.Select(item => item.Id).SequenceEqual(targetOrder.Select(item => item.Id));
        if (!changed)
        {
            return false;
        }

        for (var targetIndex = 0; targetIndex < targetOrder.Count; targetIndex++)
        {
            var currentIndex = Items.IndexOf(targetOrder[targetIndex]);
            if (currentIndex != targetIndex)
            {
                Items.Move(currentIndex, targetIndex);
            }
        }

        return true;
    }

    private int GetManualSortOrder(ChecklistItem item, int fallback = int.MaxValue) =>
        _manualSortOrders.TryGetValue(item.Id, out var sortOrder) ? sortOrder : fallback;

    private void CaptureManualOrder()
    {
        _manualSortOrders.Clear();
        for (var index = 0; index < Items.Count; index++)
        {
            _manualSortOrders[Items[index].Id] = index;
        }
    }

    private void AddToManualOrder(ChecklistItem item)
    {
        var nextSortOrder = _manualSortOrders.Count == 0
            ? 0
            : _manualSortOrders.Values.Max() + 1;
        _manualSortOrders[item.Id] = nextSortOrder;
    }

    private void RemoveFromManualOrder(Guid id)
    {
        if (!_manualSortOrders.Remove(id))
        {
            return;
        }

        var orderedIds = _manualSortOrders
            .OrderBy(entry => entry.Value)
            .Select(entry => entry.Key)
            .ToList();
        for (var index = 0; index < orderedIds.Count; index++)
        {
            _manualSortOrders[orderedIds[index]] = index;
        }
    }

    private QuestLabelState? FindLabel(Guid? labelId) => labelId.HasValue
        ? _state.Labels.FirstOrDefault(label => label.Id == labelId.Value)
        : null;

    private static QuestLabelViewModel ToLabelViewModel(QuestLabelState label) => new()
    {
        Id = label.Id,
        Name = label.Name,
        ColorHex = label.ColorHex,
        SortOrder = label.SortOrder
    };

    private void RefreshLabels()
    {
        if (SelectedLabelId.HasValue && FindLabel(SelectedLabelId) is null)
        {
            _selectedLabelId = null;
        }

        Labels.Clear();
        foreach (var label in _state.Labels.OrderBy(label => label.SortOrder))
        {
            var viewModel = ToLabelViewModel(label);
            viewModel.IsSelected = viewModel.Id == SelectedLabelId;
            Labels.Add(viewModel);
        }

        OnPropertyChanged(nameof(Labels));
        OnPropertyChanged(nameof(CanAddMoreLabels));
        OnPropertyChanged(nameof(LabelCountText));
        OnPropertyChanged(nameof(SelectedLabelId));
        OnPropertyChanged(nameof(SelectedLabel));
        OnPropertyChanged(nameof(HasSelectedLabel));
        OnPropertyChanged(nameof(SelectedLabelName));
        OnPropertyChanged(nameof(SelectedLabelColorHex));
        RaiseLabelCommandStates();
    }

    private void RefreshActiveItemLabels()
    {
        foreach (var item in Items)
        {
            var label = FindLabel(item.LabelId);
            item.ApplyLabel(label?.Id, label?.Name, label?.ColorHex);
        }
    }

    private void RenumberLabels()
    {
        for (var index = 0; index < _state.Labels.Count; index++)
        {
            _state.Labels[index].SortOrder = index;
        }
    }

    private void NotifySortModeChanged()
    {
        OnPropertyChanged(nameof(SortMode));
        OnPropertyChanged(nameof(SortModeCode));
        OnPropertyChanged(nameof(IsManualSort));
        OnPropertyChanged(nameof(IsLabelSort));
        OnPropertyChanged(nameof(IsDurationAscendingSort));
        OnPropertyChanged(nameof(IsDurationDescendingSort));
    }

    private void RaiseLabelCommandStates()
    {
        (AddLabelCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (UpdateLabelCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteLabelCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (MoveLabelUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (MoveLabelDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (AssignItemLabelCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private bool TickTimers(DateTimeOffset now)
    {
        var changed = false;
        var requiresSave = false;
        List<ChecklistItem>? elapsedQuestItems = null;

        foreach (var item in Items.Where(item => item.IsTimerRunning).ToList())
        {
            var result = item.AdvanceTimer(now);
            changed |= result != TimerAdvanceResult.None;
            requiresSave |= result is TimerAdvanceResult.Elapsed or TimerAdvanceResult.ClockAdjusted;

            if (result == TimerAdvanceResult.Elapsed)
            {
                (elapsedQuestItems ??= []).Add(item);
            }
        }

        if (changed)
        {
            NotifyTimerStateChanged();
        }

        if (requiresSave)
        {
            Save();
        }

        NotifyElapsedTimers(elapsedQuestItems);
        return changed;
    }

    private static AppState CreateFirstRunState(DateTimeOffset now) => new()
    {
        SchemaVersion = CurrentSchemaVersion,
        CurrentDate = GetDateKey(now),
        Items = [],
        History = [],
        ScheduledQuests = [],
        Labels = CreateDefaultLabels(),
        Settings = new AppSettings
        {
            AlwaysOnTop = true,
            RunAtStartup = true,
            LanguageCode = UiCopyCatalog.EnglishCode,
            ThemeCode = ThemeCatalog.LightCode,
            QuestSortMode = QuestSortModeCodes.Manual,
            OvertimeEnabled = false
        }
    };

    private static List<QuestLabelState> CreateDefaultLabels() =>
    [
        new QuestLabelState
        {
            Id = ImportantLabelId,
            Name = "Important",
            ColorHex = "#D95757",
            SortOrder = 0
        },
        new QuestLabelState
        {
            Id = PersonalLabelId,
            Name = "Personal",
            ColorHex = "#5E7FA3",
            SortOrder = 1
        },
        new QuestLabelState
        {
            Id = RoutineLabelId,
            Name = "Routine",
            ColorHex = "#5B8A72",
            SortOrder = 2
        }
    ];

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

        var loadedSchemaVersion = _state.SchemaVersion;
        var needsSave = _state.SchemaVersion != CurrentSchemaVersion;

        _state.Items ??= [];
        _state.History ??= [];
        if (_state.ScheduledQuests is null)
        {
            _state.ScheduledQuests = [];
            needsSave = true;
        }

        if (_state.Labels is null)
        {
            _state.Labels = [];
            needsSave = true;
        }

        if (loadedSchemaVersion < 6 && _state.Labels.Count == 0)
        {
            _state.Labels = CreateDefaultLabels();
            needsSave = true;
        }

        _state.Window ??= new WidgetWindowState();
        _state.Settings ??= new AppSettings();

        if (string.IsNullOrWhiteSpace(_state.CurrentDate))
        {
            _state.CurrentDate = GetDateKey(_now());
            needsSave = true;
        }

        var labelsBeforeNormalization = _state.Labels;
        _state.Labels = NormalizeLabelStates(_state.Labels);
        if (!LabelStatesEqual(labelsBeforeNormalization, _state.Labels))
        {
            needsSave = true;
        }

        var validLabelIds = _state.Labels.Select(label => label.Id).ToHashSet();

        _state.Items = NormalizeChecklistStates(
            _state.Items,
            allowRunningTimer: true,
            allowOvertime: _state.Settings.OvertimeEnabled,
            validLabelIds,
            out var normalizedActiveItems);
        needsSave |= normalizedActiveItems;

        var normalizedHistoricalItems = false;
        _state.History = _state.History
            .Where(entry => entry is not null && IsValidDateKey(entry.Date))
            .GroupBy(entry => entry.Date, StringComparer.Ordinal)
            .Select(group => group.Last())
            .Select(entry =>
            {
                entry.Items ??= [];
                entry.Items = NormalizeChecklistStates(
                    entry.Items,
                    allowRunningTimer: false,
                    allowOvertime: false,
                    validLabelIds,
                    out var normalizedEntryItems);
                normalizedHistoricalItems |= normalizedEntryItems;
                return entry;
            })
            .Where(entry => entry.Items.Count > 0)
            .OrderByDescending(entry => entry.Date, StringComparer.Ordinal)
            .ToList();
        needsSave |= normalizedHistoricalItems;

        var scheduledBeforeNormalization = _state.ScheduledQuests;
        var reservedIds = _state.Items
            .Select(item => item.Id)
            .Concat(_state.History.SelectMany(entry => entry.Items.Select(item => item.Id)))
            .ToHashSet();
        var normalizedScheduledQuests = NormalizeScheduledQuestStates(
            scheduledBeforeNormalization,
            reservedIds,
            validLabelIds);
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

        var normalizedSortMode = QuestSortModeCodes.ToCode(
            QuestSortModeCodes.Parse(_state.Settings.QuestSortMode));
        if (!string.Equals(
                _state.Settings.QuestSortMode,
                normalizedSortMode,
                StringComparison.Ordinal))
        {
            _state.Settings.QuestSortMode = normalizedSortMode;
            needsSave = true;
        }

        if (loadedSchemaVersion < 5 &&
            _state.Window.Width == 430 &&
            _state.Window.Height == 610)
        {
            _state.Window.Width = 520;
            _state.Window.Height = 680;
            needsSave = true;
        }

        _state.Window.Width = Math.Clamp(_state.Window.Width, 390, 1200);
        _state.Window.Height = Math.Clamp(_state.Window.Height, 500, 1200);
        _state.SchemaVersion = CurrentSchemaVersion;
        return needsSave;
    }

    private static List<QuestLabelState> NormalizeLabelStates(IEnumerable<QuestLabelState> states)
    {
        var result = new List<QuestLabelState>();
        var seenIds = new HashSet<Guid>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var state in states
                     .Select((state, originalIndex) => new { State = state, OriginalIndex = originalIndex })
                     .Where(entry => entry.State is not null)
                     .OrderBy(entry => entry.State.SortOrder)
                     .ThenBy(entry => entry.OriginalIndex)
                     .Select(entry => entry.State))
        {
            if (result.Count >= MaximumQuestLabels)
            {
                break;
            }

            var name = NormalizeLabelNameForStorage(state.Name);
            if (name.Length == 0 || !seenNames.Add(name))
            {
                continue;
            }

            var id = state.Id;
            if (id == Guid.Empty || !seenIds.Add(id))
            {
                do
                {
                    id = Guid.NewGuid();
                }
                while (!seenIds.Add(id));
            }

            result.Add(new QuestLabelState
            {
                Id = id,
                Name = name,
                ColorHex = TryNormalizeColorHex(state.ColorHex, out var colorHex)
                    ? colorHex
                    : DefaultLabelColorHex,
                SortOrder = result.Count
            });
        }

        return result;
    }

    private static bool LabelStatesEqual(
        IReadOnlyList<QuestLabelState> left,
        IReadOnlyList<QuestLabelState> right)
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
                !string.Equals(first.Name, second.Name, StringComparison.Ordinal) ||
                !string.Equals(first.ColorHex, second.ColorHex, StringComparison.Ordinal) ||
                first.SortOrder != second.SortOrder)
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeLabelNameForStorage(string? name)
    {
        var cleanName = name?.Trim() ?? string.Empty;
        return cleanName.Length > MaximumQuestLabelNameLength
            ? cleanName[..MaximumQuestLabelNameLength]
            : cleanName;
    }

    private static bool TryNormalizeColorHex(string? value, out string colorHex)
    {
        var candidate = value?.Trim() ?? string.Empty;
        if (candidate.Length == 7 &&
            candidate[0] == '#' &&
            candidate.Skip(1).All(Uri.IsHexDigit))
        {
            colorHex = candidate.ToUpperInvariant();
            return true;
        }

        colorHex = string.Empty;
        return false;
    }

    private List<ChecklistItemState> NormalizeChecklistStates(
        IEnumerable<ChecklistItemState> states,
        bool allowRunningTimer,
        bool allowOvertime,
        ISet<Guid> validLabelIds,
        out bool didNormalize)
    {
        didNormalize = false;
        var seenIds = new HashSet<Guid>();
        var result = new List<ChecklistItemState>();
        var hasRunningTimer = false;

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
                didNormalize = true;
            }

            var cleanText = NormalizeQuestText(state.Text);
            var createdAt = state.CreatedAt == default ? _now() : state.CreatedAt;
            var plannedDurationMinutes = NormalizeDurationMinutes(state.PlannedDurationMinutes);
            var fullDurationSeconds = plannedDurationMinutes.GetValueOrDefault() * 60;
            int? remainingSeconds = plannedDurationMinutes.HasValue
                ? Math.Clamp(state.RemainingSeconds ?? fullDurationSeconds, 0, fullDurationSeconds)
                : null;
            var isOvertime = allowRunningTimer &&
                             allowOvertime &&
                             !state.IsCompleted &&
                             plannedDurationMinutes.HasValue &&
                             remainingSeconds == 0 &&
                             state.IsOvertime;
            var overtimeSeconds = isOvertime ? Math.Max(0, state.OvertimeSeconds) : 0;
            var canRun = allowRunningTimer &&
                          !hasRunningTimer &&
                          !state.IsCompleted &&
                          plannedDurationMinutes.HasValue &&
                          (remainingSeconds > 0 || isOvertime);
            var timerStartedAt = canRun ? state.TimerStartedAt : null;
            hasRunningTimer |= timerStartedAt.HasValue;
            var labelId = state.LabelId.HasValue && validLabelIds.Contains(state.LabelId.Value)
                ? state.LabelId
                : null;

            var normalizedState = new ChecklistItemState
            {
                Id = id,
                Text = cleanText,
                IsCompleted = state.IsCompleted,
                SortOrder = result.Count,
                ManualSortOrder = state.ManualSortOrder ?? state.SortOrder,
                CreatedAt = createdAt,
                PlannedDurationMinutes = plannedDurationMinutes,
                RemainingSeconds = remainingSeconds,
                TimerStartedAt = timerStartedAt,
                IsOvertime = isOvertime,
                OvertimeSeconds = overtimeSeconds,
                LabelId = labelId
            };
            result.Add(normalizedState);

            didNormalize |=
                state.Id != normalizedState.Id ||
                !string.Equals(state.Text, normalizedState.Text, StringComparison.Ordinal) ||
                state.SortOrder != normalizedState.SortOrder ||
                state.ManualSortOrder != normalizedState.ManualSortOrder ||
                state.CreatedAt != normalizedState.CreatedAt ||
                state.PlannedDurationMinutes != normalizedState.PlannedDurationMinutes ||
                state.RemainingSeconds != normalizedState.RemainingSeconds ||
                state.TimerStartedAt != normalizedState.TimerStartedAt ||
                state.IsOvertime != normalizedState.IsOvertime ||
                state.OvertimeSeconds != normalizedState.OvertimeSeconds ||
                state.LabelId != normalizedState.LabelId;
        }

        var manualOrder = result
            .Select((state, displayIndex) => new { State = state, DisplayIndex = displayIndex })
            .OrderBy(entry => entry.State.ManualSortOrder)
            .ThenBy(entry => entry.DisplayIndex)
            .ToList();
        for (var manualIndex = 0; manualIndex < manualOrder.Count; manualIndex++)
        {
            didNormalize |= manualOrder[manualIndex].State.ManualSortOrder != manualIndex;
            manualOrder[manualIndex].State.ManualSortOrder = manualIndex;
        }

        return result;
    }

    private static int? NormalizeDurationMinutes(int? durationMinutes) =>
        durationMinutes is >= 1 and <= MaximumTimerDurationMinutes
            ? durationMinutes
            : null;

    private List<ScheduledQuestState> NormalizeScheduledQuestStates(
        IEnumerable<ScheduledQuestState> states,
        ISet<Guid> reservedIds,
        ISet<Guid> validLabelIds)
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
                CreatedAt = state.CreatedAt == default ? normalizationTime : state.CreatedAt,
                PlannedDurationMinutes = NormalizeDurationMinutes(state.PlannedDurationMinutes),
                LabelId = state.LabelId.HasValue && validLabelIds.Contains(state.LabelId.Value)
                    ? state.LabelId
                    : null
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
                first.CreatedAt != second.CreatedAt ||
                first.PlannedDurationMinutes != second.PlannedDurationMinutes ||
                first.LabelId != second.LabelId)
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
        var plannedDurationMinutes = SelectedDurationMinutes;
        var selectedLabel = FindLabel(SelectedLabelId);
        if (SelectedScheduleOffset > 0)
        {
            var targetDate = GetLocalDate(now).AddDays(SelectedScheduleOffset);
            _state.ScheduledQuests.Add(new ScheduledQuestState
            {
                Id = Guid.NewGuid(),
                Text = cleanText,
                ScheduledDate = GetDateKey(targetDate),
                SortOrder = _state.ScheduledQuests.Count,
                CreatedAt = now,
                PlannedDurationMinutes = plannedDurationMinutes,
                LabelId = selectedLabel?.Id
            });
            SortAndRenumberScheduledQuests();

            NewItemText = string.Empty;
            SelectedScheduleOffset = 0;
            SelectedDurationMinutes = null;
            SelectedLabelId = null;
            RefreshUpcomingQuests();
            Save();
            return;
        }

        var item = new ChecklistItem(
            Guid.NewGuid(),
            cleanText,
            false,
            now,
            plannedDurationMinutes,
            labelId: selectedLabel?.Id,
            labelName: selectedLabel?.Name,
            labelColorHex: selectedLabel?.ColorHex);
        SubscribeToItem(item);
        Items.Add(item);
        AddToManualOrder(item);
        NewItemText = string.Empty;
        SelectedScheduleOffset = 0;
        SelectedDurationMinutes = null;
        SelectedLabelId = null;

        ApplyQuestSort();

        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
    }

    private bool CanAddItem() => !string.IsNullOrWhiteSpace(NewItemText);

    private bool CanCopyItem(object? parameter) =>
        parameter is QuestCopyRequest request &&
        request.Offset is >= 0 and <= MaximumScheduleOffset &&
        Items.Contains(request.Item);

    private void CopyItem(object? parameter)
    {
        if (parameter is QuestCopyRequest request)
        {
            CopyItemTo(request.Item, request.Offset);
        }
    }

    private bool CanCopyScheduleDay(object? parameter) =>
        parameter is ScheduleDayCopyRequest request &&
        request.SourceOffset is >= 0 and <= MaximumScheduleOffset &&
        request.TargetOffset is >= 0 and <= MaximumScheduleOffset &&
        HasScheduleDayQuests(request.SourceOffset);

    private void CopyScheduleDay(object? parameter)
    {
        if (parameter is ScheduleDayCopyRequest request)
        {
            CopyScheduleDayTo(request.SourceOffset, request.TargetOffset);
        }
    }

    private List<QuestCopyDefinition> GetScheduleDayCopyDefinitions(
        int sourceOffset,
        DateOnly today)
    {
        if (sourceOffset == 0)
        {
            return Items
                .Select(item => new QuestCopyDefinition(
                    item.Text,
                    item.PlannedDurationMinutes,
                    item.LabelId))
                .ToList();
        }

        var sourceDateKey = GetDateKey(today.AddDays(sourceOffset));
        return _state.ScheduledQuests
            .Where(item => string.Equals(
                item.ScheduledDate,
                sourceDateKey,
                StringComparison.Ordinal))
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.CreatedAt)
            .Select(item => new QuestCopyDefinition(
                item.Text,
                item.PlannedDurationMinutes,
                item.LabelId))
            .ToList();
    }

    private static string NormalizeQuestText(string? text)
    {
        var cleanText = text?.Trim() ?? string.Empty;
        return cleanText.Length > MaximumQuestTextLength
            ? cleanText[..MaximumQuestTextLength]
            : cleanText;
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

    private void SetDuration(object? parameter)
    {
        if (TryGetDurationMinutes(parameter, out var durationMinutes))
        {
            SelectedDurationMinutes = durationMinutes;
        }
    }

    private static bool TryGetDurationMinutes(object? parameter, out int? durationMinutes)
    {
        durationMinutes = null;
        switch (parameter)
        {
            case null:
                return true;
            case int intValue when intValue == 0:
            case long longValue when longValue == 0:
                return true;
            case int intValue when intValue is >= 1 and <= MaximumTimerDurationMinutes:
                durationMinutes = intValue;
                return true;
            case long longValue when longValue is >= 1 and <= MaximumTimerDurationMinutes:
                durationMinutes = (int)longValue;
                return true;
            case string text:
                var cleanText = text.Trim();
                if (cleanText.Length == 0 ||
                    cleanText.Equals("none", StringComparison.OrdinalIgnoreCase) ||
                    cleanText.Equals("off", StringComparison.OrdinalIgnoreCase) ||
                    cleanText == "0")
                {
                    return true;
                }

                if (int.TryParse(
                        cleanText,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var parsed) &&
                    parsed is >= 1 and <= MaximumTimerDurationMinutes)
                {
                    durationMinutes = parsed;
                    return true;
                }

                return false;
            default:
                return false;
        }
    }

    private void SetSelectedLabel(object? parameter)
    {
        if (TryGetLabelId(parameter, allowNone: true, out var labelId))
        {
            SelectedLabelId = labelId;
        }
    }

    private bool CanAssignItemLabel(object? parameter) =>
        parameter is QuestItemLabelRequest request &&
        Items.Contains(request.Item) &&
        (!request.LabelId.HasValue || FindLabel(request.LabelId) is not null);

    private void AssignItemLabel(object? parameter)
    {
        if (parameter is QuestItemLabelRequest request)
        {
            AssignItemLabel(request.Item, request.LabelId);
        }
    }

    public bool AssignItemLabel(ChecklistItem item, Guid? labelId)
    {
        if (!Items.Contains(item))
        {
            return false;
        }

        var label = FindLabel(labelId);
        if (labelId.HasValue && label is null)
        {
            return false;
        }

        if (!item.ApplyLabel(label?.Id, label?.Name, label?.ColorHex))
        {
            return false;
        }

        ApplyQuestSort();
        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
        return true;
    }

    private bool CanAddLabel(object? parameter) =>
        parameter is QuestLabelDraft draft &&
        CanCreateLabel(draft.Name, draft.ColorHex);

    private void AddLabel(object? parameter)
    {
        if (parameter is QuestLabelDraft draft)
        {
            TryAddLabel(draft.Name, draft.ColorHex);
        }
    }

    public bool TryAddLabel(string? name, string? colorHex)
    {
        if (!CanCreateLabel(name, colorHex) ||
            !TryNormalizeColorHex(colorHex, out var normalizedColor))
        {
            return false;
        }

        var normalizedName = name!.Trim();
        var label = new QuestLabelState
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            ColorHex = normalizedColor,
            SortOrder = _state.Labels.Count
        };
        _state.Labels.Add(label);
        RefreshLabels();
        Save();
        return true;
    }

    private bool CanCreateLabel(string? name, string? colorHex)
    {
        var normalizedName = name?.Trim() ?? string.Empty;
        return _state.Labels.Count < MaximumQuestLabels &&
               normalizedName.Length is > 0 and <= MaximumQuestLabelNameLength &&
               !_state.Labels.Any(label => string.Equals(
                   label.Name,
                   normalizedName,
                   StringComparison.OrdinalIgnoreCase)) &&
               TryNormalizeColorHex(colorHex, out _);
    }

    private bool CanUpdateLabel(object? parameter) =>
        parameter is QuestLabelUpdateRequest request &&
        CanUpdateLabel(request.Id, request.Name, request.ColorHex);

    private void UpdateLabel(object? parameter)
    {
        if (parameter is QuestLabelUpdateRequest request)
        {
            TryUpdateLabel(request.Id, request.Name, request.ColorHex);
        }
    }

    public bool TryUpdateLabel(Guid id, string? name, string? colorHex)
    {
        if (!CanUpdateLabel(id, name, colorHex) ||
            !TryNormalizeColorHex(colorHex, out var normalizedColor))
        {
            return false;
        }

        var label = _state.Labels.Single(candidate => candidate.Id == id);
        label.Name = name!.Trim();
        label.ColorHex = normalizedColor;
        RefreshLabels();
        RefreshActiveItemLabels();
        RefreshUpcomingQuests();
        RefreshHistoryEntries();
        Save();
        return true;
    }

    private bool CanUpdateLabel(Guid id, string? name, string? colorHex)
    {
        var normalizedName = name?.Trim() ?? string.Empty;
        return id != Guid.Empty &&
               _state.Labels.Any(label => label.Id == id) &&
               normalizedName.Length is > 0 and <= MaximumQuestLabelNameLength &&
               !_state.Labels.Any(label =>
                   label.Id != id &&
                   string.Equals(label.Name, normalizedName, StringComparison.OrdinalIgnoreCase)) &&
               TryNormalizeColorHex(colorHex, out _);
    }

    private bool CanDeleteLabel(object? parameter) =>
        TryGetLabelId(parameter, allowNone: false, out var labelId) &&
        labelId.HasValue &&
        _state.Labels.Any(label => label.Id == labelId.Value);

    private void DeleteLabel(object? parameter)
    {
        if (TryGetLabelId(parameter, allowNone: false, out var labelId) && labelId.HasValue)
        {
            DeleteLabel(labelId.Value);
        }
    }

    public bool DeleteLabel(Guid id)
    {
        var label = _state.Labels.FirstOrDefault(candidate => candidate.Id == id);
        if (label is null)
        {
            return false;
        }

        _state.Labels.Remove(label);
        RenumberLabels();
        foreach (var item in Items.Where(item => item.LabelId == id))
        {
            item.ApplyLabel(null, null, null);
        }

        foreach (var scheduledQuest in _state.ScheduledQuests.Where(item => item.LabelId == id))
        {
            scheduledQuest.LabelId = null;
        }

        foreach (var historicalItem in _state.History
                     .SelectMany(entry => entry.Items)
                     .Where(item => item.LabelId == id))
        {
            historicalItem.LabelId = null;
        }

        if (SelectedLabelId == id)
        {
            SelectedLabelId = null;
        }

        RefreshLabels();
        ApplyQuestSort();
        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        RefreshUpcomingQuests();
        RefreshHistoryEntries();
        Save();
        return true;
    }

    private bool CanMoveLabelUp(object? parameter) =>
        TryGetLabelIndex(parameter, out var index) && index > 0;

    private void MoveLabelUp(object? parameter)
    {
        if (TryGetLabelIndex(parameter, out var index))
        {
            MoveLabel(index, index - 1);
        }
    }

    private bool CanMoveLabelDown(object? parameter) =>
        TryGetLabelIndex(parameter, out var index) && index < _state.Labels.Count - 1;

    private void MoveLabelDown(object? parameter)
    {
        if (TryGetLabelIndex(parameter, out var index))
        {
            MoveLabel(index, index + 1);
        }
    }

    public bool MoveLabel(Guid id, int destinationIndex)
    {
        var sourceIndex = _state.Labels.FindIndex(label => label.Id == id);
        return MoveLabel(sourceIndex, destinationIndex);
    }

    private bool MoveLabel(int sourceIndex, int destinationIndex)
    {
        if (sourceIndex < 0 || sourceIndex >= _state.Labels.Count || _state.Labels.Count < 2)
        {
            return false;
        }

        destinationIndex = Math.Clamp(destinationIndex, 0, _state.Labels.Count - 1);
        if (sourceIndex == destinationIndex)
        {
            return false;
        }

        var label = _state.Labels[sourceIndex];
        _state.Labels.RemoveAt(sourceIndex);
        _state.Labels.Insert(destinationIndex, label);
        RenumberLabels();

        var visibleSourceIndex = Labels
            .Select((candidate, index) => new { candidate.Id, Index = index })
            .FirstOrDefault(candidate => candidate.Id == label.Id)?.Index ?? -1;
        if (visibleSourceIndex >= 0 &&
            destinationIndex < Labels.Count &&
            visibleSourceIndex != destinationIndex)
        {
            Labels.Move(visibleSourceIndex, destinationIndex);
        }

        for (var index = 0; index < Labels.Count; index++)
        {
            Labels[index].UpdateSortOrder(index);
        }

        RaiseLabelCommandStates();
        if (SortMode == QuestSortMode.Label)
        {
            ApplyQuestSort();
            SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
            RefreshHistoryEntries();
        }

        Save();
        return true;
    }

    private bool CanSetSortMode(object? parameter) =>
        TryGetSortMode(parameter, out _);

    private void SetSortMode(object? parameter)
    {
        if (!TryGetSortMode(parameter, out var sortMode) || sortMode == SortMode)
        {
            return;
        }

        _sortMode = sortMode;
        _state.Settings.QuestSortMode = QuestSortModeCodes.ToCode(sortMode);
        ApplyQuestSort();
        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifySortModeChanged();
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
    }

    private static bool TryGetSortMode(object? parameter, out QuestSortMode sortMode)
    {
        switch (parameter)
        {
            case QuestSortMode mode when Enum.IsDefined(mode):
                sortMode = mode;
                return true;
            case string text:
                var normalized = text.Trim().ToLowerInvariant();
                if (normalized is QuestSortModeCodes.Manual or
                    QuestSortModeCodes.Label or
                    QuestSortModeCodes.DurationAscending or
                    QuestSortModeCodes.DurationDescending)
                {
                    sortMode = QuestSortModeCodes.Parse(normalized);
                    return true;
                }

                break;
        }

        sortMode = QuestSortMode.Manual;
        return false;
    }

    private bool TryGetLabelIndex(object? parameter, out int index)
    {
        index = -1;
        if (!TryGetLabelId(parameter, allowNone: false, out var labelId) || !labelId.HasValue)
        {
            return false;
        }

        index = _state.Labels.FindIndex(label => label.Id == labelId.Value);
        return index >= 0;
    }

    private bool TryGetLabelId(object? parameter, bool allowNone, out Guid? labelId)
    {
        labelId = parameter switch
        {
            QuestLabelViewModel label => label.Id,
            QuestLabelState label => label.Id,
            Guid guid => guid,
            string text when Guid.TryParse(text, out var parsed) => parsed,
            _ => null
        };

        if (!labelId.HasValue)
        {
            return allowNone &&
                   (parameter is null ||
                    parameter is string text &&
                    (string.IsNullOrWhiteSpace(text) ||
                     text.Equals("none", StringComparison.OrdinalIgnoreCase)));
        }

        return FindLabel(labelId) is not null;
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
        StopTimerAlarm(item.Id);
        Items.Remove(item);
        RemoveFromManualOrder(item.Id);
        NotifyTimerStateChanged();
        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
    }

    private void CompleteItem(object? parameter)
    {
        if (parameter is not ChecklistItem item ||
            item.IsCompleted ||
            !Items.Contains(item))
        {
            return;
        }

        item.IsCompleted = true;
    }

    private bool CanToggleTimer(object? parameter) =>
        parameter is ChecklistItem item &&
        Items.Contains(item) &&
        item.HasTimer &&
        !item.IsCompleted &&
        (item.RemainingSeconds > 0 || item.IsOvertime);

    private void ToggleTimer(object? parameter)
    {
        if (parameter is not ChecklistItem item || !CanToggleTimer(item))
        {
            return;
        }

        var now = _now();
        List<ChecklistItem>? elapsedQuestItems = null;
        var stateChanged = false;

        if (item.IsTimerRunning)
        {
            var result = item.AdvanceTimer(now);
            if (result == TimerAdvanceResult.Elapsed)
            {
                (elapsedQuestItems ??= []).Add(item);
                stateChanged = true;
            }
            else
            {
                stateChanged = item.PauseTimer() || result != TimerAdvanceResult.None;
            }
        }
        else
        {
            foreach (var runningItem in Items
                         .Where(candidate => candidate.IsTimerRunning && candidate != item)
                         .ToList())
            {
                var result = runningItem.AdvanceTimer(now);
                if (result == TimerAdvanceResult.Elapsed)
                {
                    (elapsedQuestItems ??= []).Add(runningItem);
                }
                else
                {
                    runningItem.PauseTimer();
                }

                stateChanged = true;
            }

            stateChanged = item.StartTimer(now) || stateChanged;
        }

        if (stateChanged)
        {
            NotifyTimerStateChanged();
            Save();
        }

        NotifyElapsedTimers(elapsedQuestItems);
    }

    private bool CanResetTimer(object? parameter) =>
        parameter is ChecklistItem item &&
        Items.Contains(item) &&
        item.HasTimer &&
        (item.IsTimerRunning || item.IsOvertime ||
         item.RemainingSeconds != item.PlannedDurationMinutes!.Value * 60);

    private void ResetTimer(object? parameter)
    {
        if (parameter is not ChecklistItem item || !CanResetTimer(item) || !item.ResetTimer())
        {
            return;
        }

        StopTimerAlarm(item.Id);
        NotifyTimerStateChanged();
        Save();
    }

    private bool CanStartOvertime(object? parameter) =>
        OvertimeEnabled &&
        parameter is ChecklistItem item &&
        Items.Contains(item) &&
        item.CanStartOvertime;

    private void StartOvertime(object? parameter)
    {
        if (parameter is not ChecklistItem item ||
            !CanStartOvertime(item) ||
            !item.StartOvertime(_now()))
        {
            return;
        }

        foreach (var runningItem in Items
                     .Where(candidate => candidate != item && candidate.IsTimerRunning)
                     .ToList())
        {
            runningItem.AdvanceTimer(_now());
            runningItem.PauseTimer();
        }

        StopTimerAlarm(item.Id);
        NotifyTimerStateChanged();
        Save();
    }

    private bool CanSetOvertime(object? parameter) => TryGetBoolean(parameter, out _);

    private void SetOvertime(object? parameter)
    {
        if (!TryGetBoolean(parameter, out var enabled) || enabled == OvertimeEnabled)
        {
            return;
        }

        _state.Settings.OvertimeEnabled = enabled;
        if (!enabled)
        {
            StopTimerAlarm();
            foreach (var item in Items.Where(item => item.IsOvertime).ToList())
            {
                item.ClearOvertime();
            }
        }

        OnPropertyChanged(nameof(OvertimeEnabled));
        NotifyTimerStateChanged();
        (StartOvertimeCommand as RelayCommand)?.RaiseCanExecuteChanged();
        Save();
    }

    private void ToggleOvertime() => SetOvertime(!OvertimeEnabled);

    private static bool TryGetBoolean(object? parameter, out bool value)
    {
        switch (parameter)
        {
            case bool boolean:
                value = boolean;
                return true;
            case string text when bool.TryParse(text, out var parsed):
                value = parsed;
                return true;
            case string text when text.Trim().Equals("on", StringComparison.OrdinalIgnoreCase):
                value = true;
                return true;
            case string text when text.Trim().Equals("off", StringComparison.OrdinalIgnoreCase):
                value = false;
                return true;
            default:
                value = false;
                return false;
        }
    }

    private void ResetToday()
    {
        StopTimerAlarm();
        _suppressItemPersistence = true;
        try
        {
            foreach (var item in Items)
            {
                item.IsCompleted = false;
                item.ResetTimer();
            }
        }
        finally
        {
            _suppressItemPersistence = false;
        }

        ApplyQuestSort();
        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyTimerStateChanged();
        NotifyProgressChanged();
        RefreshHistoryEntries();
        Save();
    }

    private void ClearCompleted()
    {
        var completedItems = Items.Where(item => item.IsCompleted).ToList();
        foreach (var item in completedItems)
        {
            StopTimerAlarm(item.Id);
            item.PropertyChanged -= Item_PropertyChanged;
            Items.Remove(item);
            RemoveFromManualOrder(item.Id);
        }

        SyncHistoryFromActiveDay(preserveCompletedOrphans: true);
        NotifyTimerStateChanged();
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

    private void SetRunAtStartup(object? parameter)
    {
        if (!TryGetBoolean(parameter, out var enabled) ||
            enabled == RunAtStartup ||
            !TryApplyRunAtStartup(enabled))
        {
            return;
        }

        _state.Settings.RunAtStartup = enabled;
        OnPropertyChanged(nameof(RunAtStartup));
        Save();
    }

    private bool TryApplyRunAtStartup(bool enabled)
    {
        try
        {
            return _runAtStartupService.TrySetEnabled(enabled);
        }
        catch (Exception)
        {
            // Startup registration is a convenience and must never prevent the app from opening.
            return false;
        }
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
                scheduledQuest.CreatedAt,
                scheduledQuest.PlannedDurationMinutes,
                labelId: scheduledQuest.LabelId,
                labelName: FindLabel(scheduledQuest.LabelId)?.Name,
                labelColorHex: FindLabel(scheduledQuest.LabelId)?.ColorHex);
            SubscribeToItem(item);
            Items.Add(item);
            AddToManualOrder(item);
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
            var questLabel = FindLabel(scheduledQuest.LabelId);
            UpcomingQuests.Add(new UpcomingQuestViewModel
            {
                Id = scheduledQuest.Id,
                Text = scheduledQuest.Text,
                ScheduledDate = scheduledQuest.ScheduledDate,
                DateText = dateText,
                ScheduleLabel = scheduleLabel,
                PlannedDurationMinutes = scheduledQuest.PlannedDurationMinutes,
                DurationText = scheduledQuest.PlannedDurationMinutes.HasValue
                    ? string.Format(
                        Copy.Culture,
                        Copy.DurationMinutesFormat,
                        scheduledQuest.PlannedDurationMinutes.Value)
                    : null,
                LabelId = questLabel?.Id,
                LabelName = questLabel?.Name,
                LabelColorHex = questLabel?.ColorHex,
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
        (CopyScheduleDayCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

        if (sender is ChecklistItem { IsCompleted: true } completedItem)
        {
            if (completedItem.IsTimerRunning)
            {
                completedItem.AdvanceTimer(_now());
                completedItem.PauseTimer();
            }

            completedItem.ClearOvertime();
            StopTimerAlarm(completedItem.Id);

            var currentIndex = Items.IndexOf(completedItem);
            if (currentIndex >= 0 && currentIndex < Items.Count - 1)
            {
                Items.Move(currentIndex, Items.Count - 1);
            }

            if (IsManualSort)
            {
                CaptureManualOrder();
            }
        }

        ApplyQuestSort();
        NotifyTimerStateChanged();
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
                    .Select(item =>
                    {
                        var label = FindLabel(item.LabelId);
                        return new HistoryItemViewModel
                        {
                            Text = item.Text,
                            IsCompleted = item.IsCompleted,
                            LabelId = label?.Id,
                            LabelName = label?.Name,
                            LabelColorHex = label?.ColorHex
                        };
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
        Items = Items.Select((item, index) => ToState(
            item,
            index,
            GetManualSortOrder(item, index))).ToList(),
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
        Labels = _state.Labels
            .Select(CloneLabelState)
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
            RunAtStartup = RunAtStartup,
            LanguageCode = LanguageCode,
            ThemeCode = ThemeCode,
            QuestSortMode = SortModeCode,
            OvertimeEnabled = OvertimeEnabled
        }
    };

    private static ChecklistItemState ToState(
        ChecklistItem item,
        int index,
        int? manualSortOrder = null) => new()
        {
            Id = item.Id,
            Text = item.Text,
            IsCompleted = item.IsCompleted,
            SortOrder = index,
            ManualSortOrder = manualSortOrder ?? index,
            CreatedAt = item.CreatedAt,
            PlannedDurationMinutes = item.PlannedDurationMinutes,
            RemainingSeconds = item.HasTimer ? item.RemainingSeconds : null,
            TimerStartedAt = item.TimerStartedAt,
            IsOvertime = item.IsOvertime,
            OvertimeSeconds = item.OvertimeSeconds,
            LabelId = item.LabelId
        };

    private static ChecklistItemState CloneItemState(ChecklistItemState item) => new()
    {
        Id = item.Id,
        Text = item.Text,
        IsCompleted = item.IsCompleted,
        SortOrder = item.SortOrder,
        ManualSortOrder = item.ManualSortOrder,
        CreatedAt = item.CreatedAt,
        PlannedDurationMinutes = item.PlannedDurationMinutes,
        RemainingSeconds = item.RemainingSeconds,
        TimerStartedAt = item.TimerStartedAt,
        IsOvertime = item.IsOvertime,
        OvertimeSeconds = item.OvertimeSeconds,
        LabelId = item.LabelId
    };

    private static ScheduledQuestState CloneScheduledQuestState(ScheduledQuestState item) => new()
    {
        Id = item.Id,
        Text = item.Text,
        ScheduledDate = item.ScheduledDate,
        SortOrder = item.SortOrder,
        CreatedAt = item.CreatedAt,
        PlannedDurationMinutes = item.PlannedDurationMinutes,
        LabelId = item.LabelId
    };

    private static QuestLabelState CloneLabelState(QuestLabelState label) => new()
    {
        Id = label.Id,
        Name = label.Name,
        ColorHex = label.ColorHex,
        SortOrder = label.SortOrder
    };

    private void NotifyTimerStateChanged()
    {
        OnPropertyChanged(nameof(ActiveTimerItem));
        OnPropertyChanged(nameof(HasActiveTimer));
        OnPropertyChanged(nameof(CompactDisplayItem));
        (ToggleTimerCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetTimerCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (StartOvertimeCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void NotifyElapsedTimers(IEnumerable<ChecklistItem>? questItems)
    {
        if (_alarmService is null || questItems is null)
        {
            return;
        }

        foreach (var item in questItems)
        {
            try
            {
                _alarmingQuestId = item.Id;
                _alarmService.NotifyTimerCompleted(item.Text);
            }
            catch (Exception)
            {
                // A notification failure must never take down the user's checklist.
            }
        }
    }

    private void StopTimerAlarm(Guid? questId = null)
    {
        if (questId.HasValue && _alarmingQuestId != questId)
        {
            return;
        }

        _alarmingQuestId = null;
        if (_alarmService is null)
        {
            return;
        }

        try
        {
            _alarmService.StopTimerAlarm();
        }
        catch (Exception)
        {
            // Alarm cleanup should never interrupt checklist persistence.
        }
    }

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
        OnPropertyChanged(nameof(CompactDisplayItem));
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
        OnPropertyChanged(nameof(SelectedDurationLabel));
        OnPropertyChanged(nameof(LabelCountText));
        RefreshScheduleOptions();
        RefreshUpcomingQuests();
    }

    private void RaiseCommandStates()
    {
        (AddItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CopyItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CopyScheduleDayCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetTodayCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ClearCompletedCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ClearHistoryCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RemoveScheduledQuestCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ToggleTimerCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetTimerCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (StartOvertimeCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

    private sealed record QuestCopyDefinition(
        string Text,
        int? PlannedDurationMinutes,
        Guid? LabelId);

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
