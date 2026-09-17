using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DailyQuest.Models;
using DailyQuest.Services;
using DailyQuest.ViewModels;

namespace DailyQuest.LogicTests;

internal static class Program
{
    private static readonly (string Name, Action Test)[] Tests =
    [
        ("first run starts empty and persists defaults", FirstRunStartsEmptyAndPersistsDefaults),
        ("saving window size clears stored position", SavingWindowSizeClearsStoredPosition),
        ("language toggle updates copy and persists", LanguageToggleUpdatesCopyAndPersists),
        ("explicit language selection validates aliases and no-ops", ExplicitLanguageSelectionValidatesAliasesAndNoOps),
        ("explicit theme selection applies validates and persists", ExplicitThemeSelectionAppliesValidatesAndPersists),
        ("today, upcoming, history, and settings navigation is mutually exclusive", ViewNavigationIsMutuallyExclusive),
        ("unknown language falls back to English", UnknownLanguageFallsBackToEnglish),
        ("language normalization preserves history-only completed items", LanguageNormalizationPreservesHistoryOnlyCompletedItems),
        ("invalid theme falls back to light without data loss", InvalidThemeFallsBackToLightWithoutDataLoss),
        ("current-day history tracks add, toggle, remove, and clear", CurrentDayHistoryTracksChecklistMutations),
        ("composer schedules only today through H plus eight", ComposerSchedulesOnlyTodayThroughEightDays),
        ("copy to today creates fresh quests without mutating their sources", CopyToTodayCreatesFreshQuestsWithoutMutatingSources),
        ("copy to future dates queues independent quests without changing today", CopyToFutureDatesQueuesIndependentQuestsWithoutChangingToday),
        ("copy rejects invalid destinations and foreign quests", CopyRejectsInvalidDestinationsAndForeignQuests),
        ("copying today's schedule preserves every quest definition without mutating today", CopyingTodaySchedulePreservesEveryDefinitionWithoutMutatingToday),
        ("copying a future schedule to today creates fresh active quests", CopyingFutureScheduleToTodayCreatesFreshActiveQuests),
        ("copying between future dates uses only the selected source day", CopyingBetweenFutureDatesUsesOnlySelectedSourceDay),
        ("same-day schedule copies use a finite source snapshot", SameDayScheduleCopiesUseFiniteSourceSnapshot),
        ("empty and invalid schedule copies do not save", EmptyAndInvalidScheduleCopiesDoNotSave),
        ("scheduled quests stay outside today's checklist until due", ScheduledQuestsStayOutsideTodayUntilDue),
        ("upcoming quests can be canceled without changing today", UpcomingQuestsCanBeCanceledWithoutChangingToday),
        ("duration selection supports custom values and no timer", DurationSelectionSupportsCustomValuesAndNoTimer),
        ("scheduled timer duration survives activation", ScheduledTimerDurationSurvivesActivation),
        ("timer starts pauses resumes resets and persists", TimerStartsPausesResumesResetsAndPersists),
        ("optional embedded ringtone is exact and alarm is capped at one minute", EmbeddedRingtoneIsValidExactAndCappedAtOneMinute),
        ("starting a timer pauses the other active timer", StartingTimerPausesOtherActiveTimer),
        ("running timer restores from timestamp and alarms once", RunningTimerRestoresFromTimestampAndAlarmsOnce),
        ("completion pauses timer and daily rollover resets it", CompletionPausesTimerAndDailyRolloverResetsIt),
        ("midnight rollover resolves timers before resetting the day", MidnightRolloverResolvesTimersBeforeResettingDay),
        ("overtime action stops the alarm and count-up persists", OvertimeActionStopsAlarmAndCountUpPersists),
        ("disabled overtime rejects count-up and alarm can stop", DisabledOvertimeRejectsCountUpAndAlarmCanStop),
        ("overtime clears on completion reset and setting disable", OvertimeClearsOnCompletionResetAndSettingDisable),
        ("overtime dependent properties notify after reset and completion", OvertimeDependentPropertiesNotifyAfterResetAndCompletion),
        ("v6 migration defaults overtime off without adding deleted labels", V6MigrationDefaultsOvertimeOffWithoutAddingLabels),
        ("v4 migration expands only the legacy default window", V4MigrationExpandsOnlyLegacyDefaultWindow),
        ("next pending item follows quest order and completion", NextPendingItemFollowsQuestOrderAndCompletion),
        ("completed quest moves to the bottom without checking the next quest", CompletedQuestMovesToBottomWithoutCheckingNextQuest),
        ("pin toggle persists and updates its presentation", PinTogglePersistsAndUpdatesPresentation),
        ("quest reorder persists to active state and current history", QuestReorderPersistsToActiveStateAndCurrentHistory),
        ("quest reorder retains completed history-only items", QuestReorderRetainsCompletedHistoryOnlyItems),
        ("quest reorder rejects foreign and unchanged moves", QuestReorderRejectsForeignAndUnchangedMoves),
        ("persisted sort order is normalized stably", PersistedSortOrderIsNormalizedStably),
        ("label defaults migrate once while an intentional empty set stays empty", LabelDefaultsMigrateOnceAndEmptySetStaysEmpty),
        ("label CRUD validates and clears quest references safely", LabelCrudValidatesAndClearsReferences),
        ("label sorting assignment and label order preserve manual order", LabelSortingAssignmentAndOrderPreserveManualOrder),
        ("duration sorting keeps untimed and completed quests last without disturbing timers", DurationSortingKeepsTimerSound),
        ("scheduled labels survive activation and history projection", ScheduledLabelsSurviveActivationAndHistory),
        ("malformed persisted labels normalize safely", MalformedPersistedLabelsNormalizeSafely),
        ("clear history preserves active quests", ClearHistoryPreservesActiveQuests),
        ("settings refreshes injected storage usage", SettingsRefreshesInjectedStorageUsage),
        ("storage usage failures degrade gracefully", StorageUsageFailuresDegradeGracefully),
        ("active quests recur unchecked after daily rollover", ActiveQuestsRecurUncheckedAfterDailyRollover),
        ("daily rollover archives once without duplicates", DailyRolloverArchivesOnceWithoutDuplicates),
        ("daily rollover activates due quests after archiving", DailyRolloverActivatesDueQuestsAfterArchiving),
        ("overdue quests activate once on the next launch", OverdueQuestsActivateOnceOnNextLaunch),
        ("legacy v3 state migrates to the light theme", LegacyV3StateMigratesToLightTheme),
        ("legacy v2 state migrates with an empty future queue", LegacyV2StateMigratesWithEmptyFutureQueue),
        ("legacy v1 state migrates without history loss", LegacyV1StateMigratesWithoutHistoryLoss),
        ("future schema is rejected without saving", FutureSchemaIsRejectedWithoutSaving),
        ("JSON state store round-trips history language and theme", JsonStateStoreRoundTripsHistoryLanguageAndTheme),
        ("JSON state store recovers from corrupt state", JsonStateStoreRecoversFromCorruptState),
        ("JSON state store backs up and recovers from null root", JsonStateStoreRecoversFromNullRootState),
        ("JSON state store migrates valid legacy bytes and preserves source", JsonStateStoreMigratesValidLegacyBytesAndPreservesSource),
        ("JSON state store prefers an existing target over legacy state", JsonStateStorePrefersExistingTargetOverLegacyState),
        ("JSON state store custom path does not probe legacy state", JsonStateStoreCustomPathDoesNotProbeLegacyState),
        ("JSON state store snapshots corrupt legacy migration", JsonStateStoreSnapshotsCorruptLegacyMigration),
        ("JSON state store snapshots null legacy migration", JsonStateStoreSnapshotsNullLegacyMigration)
    ];

    private static int Main()
    {
        var failures = new List<string>();

        foreach (var (name, test) in Tests)
        {
            try
            {
                test();
                Console.WriteLine($"PASS  {name}");
            }
            catch (Exception exception)
            {
                failures.Add(name);
                Console.Error.WriteLine($"FAIL  {name}");
                Console.Error.WriteLine($"      {exception.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{Tests.Length - failures.Count}/{Tests.Length} tests passed.");

        if (failures.Count == 0)
        {
            return 0;
        }

        Console.Error.WriteLine($"Failed: {string.Join(", ", failures)}");
        return 1;
    }

    private static void FirstRunStartsEmptyAndPersistsDefaults()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 30, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore();
        var theme = new RecordingThemeService();

        var viewModel = new MainViewModel(store, () => now, null, theme);

        AssertEx.Equal(0, viewModel.TotalCount);
        AssertEx.Equal(0, viewModel.CompletedCount);
        AssertEx.Equal(0d, viewModel.ProgressPercent);
        AssertEx.Equal("No activities yet", viewModel.ProgressText);
        AssertEx.Equal("en-US", viewModel.LanguageCode);
        AssertEx.Equal("EN", viewModel.LanguageBadge);
        AssertEx.Equal("Good morning!", viewModel.Greeting);
        AssertEx.Equal(0, viewModel.Items.Count);
        AssertEx.Equal(0, viewModel.HistoryEntries.Count);
        AssertEx.SequenceEqual(
            ["Important", "Personal", "Routine"],
            viewModel.Labels.Select(label => label.Name));
        AssertEx.True(
            viewModel.Labels.All(label => label.Id != Guid.Empty),
            "Built-in labels should have stable non-empty IDs.");
        AssertEx.Equal(1, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(7, saved.SchemaVersion);
        AssertEx.Equal("2026-09-16", saved.CurrentDate);
        AssertEx.Equal(0, saved.Items.Count);
        AssertEx.Equal(0, saved.ScheduledQuests.Count);
        AssertEx.Equal(0, saved.History.Count);
        AssertEx.Equal(3, saved.Labels.Count);
        AssertEx.Equal("manual", saved.Settings.QuestSortMode);
        AssertEx.Equal(520d, saved.Window.Width);
        AssertEx.Equal(680d, saved.Window.Height);
        AssertEx.True(saved.Settings.AlwaysOnTop, "Always-on-top should default to enabled.");
        AssertEx.Equal("en-US", saved.Settings.LanguageCode);
        AssertEx.Equal("light", saved.Settings.ThemeCode);
        AssertEx.Equal("light", viewModel.ThemeCode);
        AssertEx.True(viewModel.IsLightTheme, "First run should use the light theme.");
        AssertEx.False(viewModel.IsDarkTheme);
        AssertEx.SequenceEqual(["light"], theme.AppliedThemes);

        var reloadStore = new InMemoryStateStore(saved);
        var reloadTheme = new RecordingThemeService();
        var reloaded = new MainViewModel(reloadStore, () => now, null, reloadTheme);

        AssertEx.Equal(0, reloaded.TotalCount);
        AssertEx.Equal(0, reloaded.Items.Count);
        AssertEx.Equal(0, reloaded.HistoryEntries.Count);
        AssertEx.SequenceEqual(
            saved.Labels.Select(label => label.Id),
            reloaded.Labels.Select(label => label.Id));
        AssertEx.Equal("light", reloaded.ThemeCode);
        AssertEx.SequenceEqual(["light"], reloadTheme.AppliedThemes);
        AssertEx.Equal(0, reloadStore.SaveCount);
    }

    private static void SavingWindowSizeClearsStoredPosition()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 30, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Window = new WidgetWindowState
            {
                Left = 120,
                Top = 80,
                Width = 390,
                Height = 610
            }
        });
        var viewModel = new MainViewModel(store, () => now);

        viewModel.SaveWindowSize(430, 650);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.False(saved.Window.Left.HasValue, "Saved horizontal position should be cleared.");
        AssertEx.False(saved.Window.Top.HasValue, "Saved vertical position should be cleared.");
        AssertEx.Equal(430d, saved.Window.Width);
        AssertEx.Equal(650d, saved.Window.Height);
        AssertEx.Equal(1, store.SaveCount);
    }

    private static void LanguageToggleUpdatesCopyAndPersists()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 30, 0, TimeSpan.FromHours(7));
        var itemId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(itemId, "Minum air", false, 0, now.AddMinutes(-2))
            ],
            Settings = new AppSettings
            {
                AlwaysOnTop = true,
                LanguageCode = "id-ID"
            }
        });
        var viewModel = new MainViewModel(store, () => now);
        var changedProperties = new HashSet<string?>();
        viewModel.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName);

        AssertEx.Equal("id-ID", viewModel.LanguageCode);
        AssertEx.Equal("ID", viewModel.LanguageBadge);
        AssertEx.Equal("Selamat pagi!", viewModel.Greeting);
        AssertEx.Equal("Rabu, 16 September", viewModel.FriendlyDate);
        AssertEx.Equal("0 dari 1 selesai", viewModel.ProgressText);
        AssertEx.Equal("Pelan-pelan, mulai dari yang paling gampang.", viewModel.EncouragementText);
        AssertEx.Equal("Lepas dari paling depan", viewModel.PinTooltip);

        viewModel.ToggleLanguageCommand.Execute(null);

        AssertEx.Equal("en-US", viewModel.LanguageCode);
        AssertEx.Equal("EN", viewModel.LanguageBadge);
        AssertEx.Equal("Good morning!", viewModel.Greeting);
        AssertEx.Equal("Wednesday, 16 September", viewModel.FriendlyDate);
        AssertEx.Equal("0 of 1 done", viewModel.ProgressText);
        AssertEx.Equal("Take it easy\u2014start with the simplest one.", viewModel.EncouragementText);
        AssertEx.Equal("Turn off always on top", viewModel.PinTooltip);
        AssertEx.Equal(itemId, viewModel.Items.Single().Id);
        AssertEx.Equal("Minum air", viewModel.Items.Single().Text);
        AssertEx.Equal(1, store.SaveCount);

        foreach (var propertyName in new[]
                 {
                     nameof(MainViewModel.Copy),
                     nameof(MainViewModel.LanguageCode),
                     nameof(MainViewModel.LanguageBadge),
                     nameof(MainViewModel.PinTooltip),
                     nameof(MainViewModel.Greeting),
                     nameof(MainViewModel.FriendlyDate),
                     nameof(MainViewModel.ProgressText),
                     nameof(MainViewModel.EncouragementText)
                 })
        {
            AssertEx.True(
                changedProperties.Contains(propertyName),
                $"Changing language should notify {propertyName}.");
        }

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal("en-US", saved.Settings.LanguageCode);
        AssertEx.Equal("Minum air", saved.Items.Single().Text);

        var reloadStore = new InMemoryStateStore(saved);
        var reloaded = new MainViewModel(reloadStore, () => now);

        AssertEx.Equal("en-US", reloaded.LanguageCode);
        AssertEx.Equal("Good morning!", reloaded.Greeting);
        AssertEx.Equal("0 of 1 done", reloaded.ProgressText);
        AssertEx.Equal(0, reloadStore.SaveCount);

        reloaded.ToggleLanguageCommand.Execute(null);
        AssertEx.Equal("id-ID", reloaded.LanguageCode);
        AssertEx.Equal("id-ID", AssertEx.NotNull(reloadStore.Snapshot).Settings.LanguageCode);
    }

    private static void ExplicitLanguageSelectionValidatesAliasesAndNoOps()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 30, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Settings = new AppSettings
            {
                LanguageCode = "id-ID"
            }
        });
        var viewModel = new MainViewModel(store, () => now);
        var changedProperties = new HashSet<string?>();
        viewModel.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName);

        viewModel.SetLanguageCommand.Execute(" EN ");

        AssertEx.Equal("en-US", viewModel.LanguageCode);
        AssertEx.True(viewModel.IsEnglish, "The English selection should become active.");
        AssertEx.False(viewModel.IsIndonesian, "The Indonesian selection should become inactive.");
        AssertEx.True(
            changedProperties.Contains(nameof(MainViewModel.FooterText)),
            "Localized footer copy should be refreshed with the language.");
        AssertEx.Equal(1, store.SaveCount);

        viewModel.SetLanguageCommand.Execute("en-US");
        viewModel.SetLanguageCommand.Execute("ja-JP");
        viewModel.SetLanguageCommand.Execute(null);

        AssertEx.Equal("en-US", viewModel.LanguageCode);
        AssertEx.Equal(1, store.SaveCount);

        viewModel.SetLanguageCommand.Execute("id");

        AssertEx.Equal("id-ID", viewModel.LanguageCode);
        AssertEx.True(viewModel.IsIndonesian, "The Indonesian alias should be accepted.");
        AssertEx.False(viewModel.IsEnglish, "The English selection should become inactive.");
        AssertEx.Equal(2, store.SaveCount);
        AssertEx.Equal("id-ID", AssertEx.NotNull(store.Snapshot).Settings.LanguageCode);
    }

    private static void ExplicitThemeSelectionAppliesValidatesAndPersists()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 30, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Settings = new AppSettings
            {
                LanguageCode = "id-ID",
                ThemeCode = "light"
            }
        });
        var theme = new RecordingThemeService();
        var viewModel = new MainViewModel(store, () => now, null, theme);
        var changedProperties = new HashSet<string?>();
        viewModel.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName);

        AssertEx.SequenceEqual(["light"], theme.AppliedThemes);
        viewModel.SetThemeCommand.Execute(" DARK ");

        AssertEx.Equal("dark", viewModel.ThemeCode);
        AssertEx.True(viewModel.IsDarkTheme, "The dark selection should become active.");
        AssertEx.False(viewModel.IsLightTheme, "The light selection should become inactive.");
        AssertEx.SequenceEqual(["light", "dark"], theme.AppliedThemes);
        AssertEx.True(
            changedProperties.Contains(nameof(MainViewModel.ThemeCode)),
            "Changing the theme should notify ThemeCode.");
        AssertEx.True(
            changedProperties.Contains(nameof(MainViewModel.IsLightTheme)),
            "Changing the theme should notify IsLightTheme.");
        AssertEx.True(
            changedProperties.Contains(nameof(MainViewModel.IsDarkTheme)),
            "Changing the theme should notify IsDarkTheme.");
        AssertEx.Equal(1, store.SaveCount);

        viewModel.SetThemeCommand.Execute("dark");
        viewModel.SetThemeCommand.Execute("system");
        viewModel.SetThemeCommand.Execute("sepia");
        viewModel.SetThemeCommand.Execute(null);

        AssertEx.Equal("dark", viewModel.ThemeCode);
        AssertEx.SequenceEqual(["light", "dark"], theme.AppliedThemes);
        AssertEx.Equal(1, store.SaveCount);

        viewModel.SetLanguageCommand.Execute("en-US");

        AssertEx.Equal("en-US", viewModel.LanguageCode);
        AssertEx.Equal("dark", viewModel.ThemeCode);
        AssertEx.SequenceEqual(["light", "dark"], theme.AppliedThemes);
        AssertEx.Equal(2, store.SaveCount);

        var darkSnapshot = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal("dark", darkSnapshot.Settings.ThemeCode);
        AssertEx.Equal("en-US", darkSnapshot.Settings.LanguageCode);
        var reloadStore = new InMemoryStateStore(darkSnapshot);
        var reloadTheme = new RecordingThemeService();
        var reloaded = new MainViewModel(reloadStore, () => now, null, reloadTheme);

        AssertEx.Equal("dark", reloaded.ThemeCode);
        AssertEx.True(reloaded.IsDarkTheme);
        AssertEx.SequenceEqual(["dark"], reloadTheme.AppliedThemes);
        AssertEx.Equal(0, reloadStore.SaveCount);

        viewModel.SetThemeCommand.Execute("LIGHT");

        AssertEx.Equal("light", viewModel.ThemeCode);
        AssertEx.True(viewModel.IsLightTheme);
        AssertEx.False(viewModel.IsDarkTheme);
        AssertEx.SequenceEqual(["light", "dark", "light"], theme.AppliedThemes);
        AssertEx.Equal(3, store.SaveCount);
        AssertEx.Equal("light", AssertEx.NotNull(store.Snapshot).Settings.ThemeCode);
    }

    private static void ViewNavigationIsMutuallyExclusive()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 30, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16"
        });
        var storage = new FakeStorageUsageService(
            _ => new StorageUsageSnapshot(0, 0, 0));
        var viewModel = new MainViewModel(store, () => now, storage);

        AssertEx.True(viewModel.IsTodayView, "Today should be the initial view.");
        AssertEx.False(viewModel.IsHistoryView);
        AssertEx.False(viewModel.IsUpcomingView);
        AssertEx.False(viewModel.IsSettingsView);

        viewModel.ShowSettingsCommand.Execute(null);

        AssertEx.False(viewModel.IsTodayView);
        AssertEx.False(viewModel.IsHistoryView);
        AssertEx.False(viewModel.IsUpcomingView);
        AssertEx.True(viewModel.IsSettingsView, "Settings should be the only active view.");
        AssertEx.Equal(1, storage.MeasureCount);

        viewModel.ShowUpcomingCommand.Execute(null);

        AssertEx.False(viewModel.IsTodayView);
        AssertEx.False(viewModel.IsHistoryView);
        AssertEx.True(viewModel.IsUpcomingView, "Upcoming should be the only active view.");
        AssertEx.False(viewModel.IsSettingsView);

        viewModel.ShowHistoryCommand.Execute(null);

        AssertEx.False(viewModel.IsTodayView);
        AssertEx.True(viewModel.IsHistoryView, "History should be the only active view.");
        AssertEx.False(viewModel.IsUpcomingView);
        AssertEx.False(viewModel.IsSettingsView);

        viewModel.ShowTodayCommand.Execute(null);

        AssertEx.True(viewModel.IsTodayView, "Today should be restored as the only active view.");
        AssertEx.False(viewModel.IsHistoryView);
        AssertEx.False(viewModel.IsUpcomingView);
        AssertEx.False(viewModel.IsSettingsView);
        AssertEx.Equal(0, store.SaveCount);
    }

    private static void UnknownLanguageFallsBackToEnglish()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 30, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Settings = new AppSettings
            {
                LanguageCode = "fr-FR"
            }
        });

        var viewModel = new MainViewModel(store, () => now);

        AssertEx.Equal("en-US", viewModel.LanguageCode);
        AssertEx.Equal("EN", viewModel.LanguageBadge);
        AssertEx.Equal("Good morning!", viewModel.Greeting);
        AssertEx.Equal("en-US", AssertEx.NotNull(store.Snapshot).Settings.LanguageCode);
        AssertEx.Equal(1, store.SaveCount);
    }

    private static void LanguageNormalizationPreservesHistoryOnlyCompletedItems()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 30, 0, TimeSpan.FromHours(7));
        var orphanId = Guid.NewGuid();
        var orphanCreatedAt = now.AddMinutes(-20);
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items = [],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-16",
                    Items =
                    [
                        CreateItemState(
                            orphanId,
                            "Selesai lalu dihapus",
                            true,
                            0,
                            orphanCreatedAt)
                    ]
                }
            ],
            Settings = new AppSettings
            {
                LanguageCode = "not-a-language"
            }
        });

        var viewModel = new MainViewModel(store, () => now);

        AssertEx.Equal("en-US", viewModel.LanguageCode);
        AssertEx.Equal(0, viewModel.Items.Count);
        AssertEx.Equal(1, viewModel.HistoryEntries.Count);
        AssertEx.Equal(1, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        var retainedHistory = HistoryFor(saved, "2026-09-16");
        var retainedOrphan = retainedHistory.Items.Single();
        AssertEx.Equal(orphanId, retainedOrphan.Id);
        AssertEx.Equal("Selesai lalu dihapus", retainedOrphan.Text);
        AssertEx.True(retainedOrphan.IsCompleted, "Language normalization must not erase a completed history-only item.");
        AssertEx.Equal(orphanCreatedAt, retainedOrphan.CreatedAt);
        AssertEx.Equal("en-US", saved.Settings.LanguageCode);
    }

    private static void InvalidThemeFallsBackToLightWithoutDataLoss()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 30, 0, TimeSpan.FromHours(7));
        var activeId = Guid.NewGuid();
        var historicalId = Guid.NewGuid();
        var scheduledId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(activeId, "Quest aktif", false, 0, now.AddMinutes(-2))
            ],
            ScheduledQuests =
            [
                CreateScheduledQuestState(
                    scheduledId,
                    "Quest mendatang",
                    "2026-09-18",
                    0,
                    now.AddMinutes(-1))
            ],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-15",
                    Items =
                    [
                        CreateItemState(
                            historicalId,
                            "Quest historis",
                            true,
                            0,
                            now.AddDays(-1))
                    ]
                }
            ],
            Settings = new AppSettings
            {
                AlwaysOnTop = false,
                LanguageCode = "en-US",
                ThemeCode = "sepia"
            }
        });
        var theme = new RecordingThemeService();

        var viewModel = new MainViewModel(store, () => now, null, theme);

        AssertEx.Equal("light", viewModel.ThemeCode);
        AssertEx.True(viewModel.IsLightTheme);
        AssertEx.False(viewModel.IsDarkTheme);
        AssertEx.SequenceEqual(["light"], theme.AppliedThemes);
        AssertEx.SequenceEqual([activeId], viewModel.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([scheduledId], viewModel.UpcomingQuests.Select(item => item.Id));
        AssertEx.Equal(1, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal("light", saved.Settings.ThemeCode);
        AssertEx.Equal("en-US", saved.Settings.LanguageCode);
        AssertEx.False(saved.Settings.AlwaysOnTop, "Theme normalization must preserve the pin preference.");
        AssertEx.SequenceEqual([activeId], saved.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([scheduledId], saved.ScheduledQuests.Select(item => item.Id));
        AssertEx.SequenceEqual(
            [historicalId],
            HistoryFor(saved, "2026-09-15").Items.Select(item => item.Id));
    }

    private static void CurrentDayHistoryTracksChecklistMutations()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16"
        });
        var viewModel = new MainViewModel(store, () => now);

        viewModel.NewItemText = "Catatan sementara";
        viewModel.AddItemCommand.Execute(null);
        var temporary = viewModel.Items.Single();
        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal("Catatan sementara", HistoryFor(saved, "2026-09-16").Items.Single().Text);

        viewModel.RemoveItemCommand.Execute(temporary);
        saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(0, saved.Items.Count);
        AssertEx.Equal(0, saved.History.Count);

        viewModel.NewItemText = "Selesai lalu dihapus";
        viewModel.AddItemCommand.Execute(null);
        var removedAfterCompletion = viewModel.Items.Single();
        removedAfterCompletion.IsCompleted = true;
        viewModel.RemoveItemCommand.Execute(removedAfterCompletion);

        saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(0, saved.Items.Count);
        var retainedCompleted = HistoryFor(saved, "2026-09-16").Items.Single();
        AssertEx.Equal(removedAfterCompletion.Id, retainedCompleted.Id);
        AssertEx.True(retainedCompleted.IsCompleted, "A completed removed item should remain in today's history.");

        viewModel.NewItemText = "Beres via clear";
        viewModel.AddItemCommand.Execute(null);
        var cleared = viewModel.Items.Single(item => item.Text == "Beres via clear");
        viewModel.NewItemText = "Tetap aktif";
        viewModel.AddItemCommand.Execute(null);
        var active = viewModel.Items.Single(item => item.Text == "Tetap aktif");
        cleared.IsCompleted = true;

        AssertEx.True(viewModel.ClearCompletedCommand.CanExecute(null), "Clear should be enabled with completed items.");
        viewModel.ClearCompletedCommand.Execute(null);

        AssertEx.SequenceEqual([active.Id], viewModel.Items.Select(item => item.Id));
        saved = AssertEx.NotNull(store.Snapshot);
        var afterClear = HistoryFor(saved, "2026-09-16");
        AssertEx.SequenceEqual(
            [removedAfterCompletion.Id, active.Id, cleared.Id],
            afterClear.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([true, false, true], afterClear.Items.Select(item => item.IsCompleted));

        viewModel.RemoveItemCommand.Execute(active);
        saved = AssertEx.NotNull(store.Snapshot);
        var finalHistory = HistoryFor(saved, "2026-09-16");
        AssertEx.SequenceEqual(
            [removedAfterCompletion.Id, cleared.Id],
            finalHistory.Items.Select(item => item.Id));
        AssertEx.True(finalHistory.Items.All(item => item.IsCompleted), "Only completed historical items should remain.");
        AssertEx.Equal(1, viewModel.HistoryEntries.Count);
        AssertEx.Equal("2 of 2 done", viewModel.HistoryEntries.Single().SummaryText);
        AssertEx.Equal(10, store.SaveCount);
    }

    private static void ComposerSchedulesOnlyTodayThroughEightDays()
    {
        var now = new DateTimeOffset(2026, 9, 16, 23, 45, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16"
        });
        var viewModel = new MainViewModel(store, () => now);

        AssertEx.Equal(9, viewModel.ScheduleOptions.Count);
        AssertEx.Equal(0, viewModel.SelectedScheduleOffset);
        AssertEx.True(
            !string.IsNullOrWhiteSpace(viewModel.SelectedScheduleLabel),
            "The composer should expose a localized label for its selected date.");

        viewModel.SetScheduleOffsetCommand.Execute(-1);
        AssertEx.Equal(0, viewModel.SelectedScheduleOffset);
        viewModel.SetScheduleOffsetCommand.Execute(9);
        AssertEx.Equal(0, viewModel.SelectedScheduleOffset);
        AssertEx.Equal(0, store.SaveCount);

        viewModel.SetScheduleOffsetCommand.Execute(1);
        viewModel.NewItemText = "Quest besok";
        viewModel.AddItemCommand.Execute(null);

        AssertEx.Equal(0, viewModel.SelectedScheduleOffset);
        AssertEx.Equal(0, viewModel.Items.Count);
        AssertEx.Equal(1, viewModel.UpcomingQuests.Count);

        viewModel.SetScheduleOffsetCommand.Execute(8);
        viewModel.NewItemText = "Quest H+8";
        viewModel.AddItemCommand.Execute(null);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(2, store.SaveCount);
        AssertEx.Equal(0, saved.Items.Count);
        AssertEx.Equal(0, saved.History.Count);
        AssertEx.SequenceEqual(
            ["2026-09-17", "2026-09-24"],
            saved.ScheduledQuests.Select(item => item.ScheduledDate));
        AssertEx.SequenceEqual(
            ["Quest besok", "Quest H+8"],
            saved.ScheduledQuests.Select(item => item.Text));
        AssertEx.SequenceEqual([0, 1], saved.ScheduledQuests.Select(item => item.SortOrder));
        AssertEx.Equal(2, viewModel.UpcomingQuests.Count);
        AssertEx.Equal(0, viewModel.SelectedScheduleOffset);
    }

    private static void CopyToTodayCreatesFreshQuestsWithoutMutatingSources()
    {
        var now = new DateTimeOffset(2026, 9, 17, 14, 30, 0, TimeSpan.FromHours(7));
        var firstId = Guid.NewGuid();
        var overtimeId = Guid.NewGuid();
        var completedId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-17",
            Items =
            [
                CreateItemState(firstId, "First pending", false, 0, now.AddMinutes(-30)),
                CreateItemState(
                    overtimeId,
                    "Timed focus",
                    false,
                    1,
                    now.AddMinutes(-20),
                    plannedDurationMinutes: 25,
                    remainingSeconds: 0,
                    labelId: labelId,
                    manualSortOrder: 1,
                    isOvertime: true,
                    overtimeSeconds: 93),
                CreateItemState(
                    completedId,
                    "Completed source",
                    true,
                    2,
                    now.AddMinutes(-10),
                    plannedDurationMinutes: 15,
                    remainingSeconds: 240,
                    labelId: labelId,
                    manualSortOrder: 2)
            ],
            Labels =
            [
                new QuestLabelState
                {
                    Id = labelId,
                    Name = "Important",
                    ColorHex = "#D95757",
                    SortOrder = 0
                }
            ],
            Settings = new AppSettings
            {
                OvertimeEnabled = true,
                QuestSortMode = QuestSortModeCodes.Manual
            }
        });
        var viewModel = new MainViewModel(store, () => now);
        var overtimeSource = viewModel.Items.Single(item => item.Id == overtimeId);
        var completedSource = viewModel.Items.Single(item => item.Id == completedId);

        var request = new QuestCopyRequest(overtimeSource, 0);
        AssertEx.True(viewModel.CopyItemCommand.CanExecute(request));
        viewModel.CopyItemCommand.Execute(request);

        var overtimeCopy = viewModel.Items.Single(item =>
            item.Text == overtimeSource.Text && item.Id != overtimeSource.Id);
        AssertEx.True(overtimeCopy.Id != Guid.Empty && overtimeCopy.Id != overtimeSource.Id);
        AssertEx.Equal(overtimeSource.Text, overtimeCopy.Text);
        AssertEx.False(overtimeCopy.IsCompleted);
        AssertEx.Equal(now, overtimeCopy.CreatedAt);
        AssertEx.Equal(25, overtimeCopy.PlannedDurationMinutes);
        AssertEx.Equal(25 * 60, overtimeCopy.RemainingSeconds);
        AssertEx.Equal<DateTimeOffset?>(null, overtimeCopy.TimerStartedAt);
        AssertEx.False(overtimeCopy.IsTimerRunning);
        AssertEx.False(overtimeCopy.IsOvertime);
        AssertEx.Equal(0, overtimeCopy.OvertimeSeconds);
        AssertEx.Equal(labelId, overtimeCopy.LabelId);
        AssertEx.Equal("Important", overtimeCopy.LabelName);
        AssertEx.Equal("#D95757", overtimeCopy.LabelColorHex);

        AssertEx.False(overtimeSource.IsCompleted);
        AssertEx.Equal(0, overtimeSource.RemainingSeconds);
        AssertEx.Equal<DateTimeOffset?>(null, overtimeSource.TimerStartedAt);
        AssertEx.True(overtimeSource.IsOvertime);
        AssertEx.Equal(93, overtimeSource.OvertimeSeconds);
        AssertEx.Equal(labelId, overtimeSource.LabelId);

        AssertEx.True(viewModel.CopyItemTo(completedSource, 0));
        var completedCopy = viewModel.Items.Single(item =>
            item.Text == completedSource.Text && item.Id != completedSource.Id);
        AssertEx.True(completedCopy.Id != Guid.Empty && completedCopy.Id != completedSource.Id);
        AssertEx.False(completedCopy.IsCompleted, "A copy of a completed quest must start unchecked.");
        AssertEx.Equal(15, completedCopy.PlannedDurationMinutes);
        AssertEx.Equal(15 * 60, completedCopy.RemainingSeconds);
        AssertEx.Equal<DateTimeOffset?>(null, completedCopy.TimerStartedAt);
        AssertEx.False(completedCopy.IsOvertime);
        AssertEx.Equal(labelId, completedCopy.LabelId);
        AssertEx.True(completedSource.IsCompleted, "Copying must not reopen the source quest.");
        AssertEx.Equal(240, completedSource.RemainingSeconds);

        AssertEx.SequenceEqual(
            [firstId, overtimeId, overtimeCopy.Id, completedCopy.Id, completedId],
            viewModel.Items.Select(item => item.Id));
        AssertEx.Equal(2, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.SequenceEqual([0, 1, 3, 4, 2], saved.Items.Select(item => item.ManualSortOrder.GetValueOrDefault()));
        AssertEx.SequenceEqual([0, 1, 2, 3, 4], saved.Items.Select(item => item.SortOrder));
        AssertEx.SequenceEqual(
            viewModel.Items.Select(item => item.Id),
            HistoryFor(saved, "2026-09-17").Items.Select(item => item.Id));
    }

    private static void CopyToFutureDatesQueuesIndependentQuestsWithoutChangingToday()
    {
        var now = new DateTimeOffset(2026, 9, 17, 15, 0, 0, TimeSpan.FromHours(7));
        var sourceId = Guid.NewGuid();
        var historicalId = Guid.NewGuid();
        var existingScheduledId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var sourceState = CreateItemState(
            sourceId,
            "Reusable quest",
            false,
            0,
            now.AddHours(-1),
            plannedDurationMinutes: 40,
            remainingSeconds: 1_620,
            labelId: labelId);
        var historicalState = CreateItemState(
            historicalId,
            "Already removed",
            true,
            1,
            now.AddHours(-2));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-17",
            Items = [sourceState],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-17",
                    Items = [sourceState, historicalState]
                }
            ],
            ScheduledQuests =
            [
                CreateScheduledQuestState(
                    existingScheduledId,
                    "Existing future quest",
                    "2026-09-21",
                    0,
                    now.AddMinutes(-5))
            ],
            Labels =
            [
                new QuestLabelState
                {
                    Id = labelId,
                    Name = "Focus",
                    ColorHex = "#5E7FA3",
                    SortOrder = 0
                }
            ]
        });
        var viewModel = new MainViewModel(store, () => now);
        var source = viewModel.Items.Single();
        var before = AssertEx.NotNull(store.Snapshot);
        var historyBefore = JsonSerializer.Serialize(before.History);
        var itemsBefore = JsonSerializer.Serialize(before.Items);

        AssertEx.True(viewModel.CopyItemTo(source, 1));
        var dayEightRequest = new QuestCopyRequest(source, 8);
        AssertEx.True(viewModel.CopyItemCommand.CanExecute(dayEightRequest));
        viewModel.CopyItemCommand.Execute(dayEightRequest);

        AssertEx.Equal(1, viewModel.Items.Count);
        AssertEx.Equal(sourceId, viewModel.Items.Single().Id);
        AssertEx.Equal(1_620, source.RemainingSeconds);
        AssertEx.False(source.IsTimerRunning);
        AssertEx.Equal(3, viewModel.UpcomingQuests.Count);
        AssertEx.Equal(2, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(historyBefore, JsonSerializer.Serialize(saved.History));
        AssertEx.Equal(itemsBefore, JsonSerializer.Serialize(saved.Items));
        AssertEx.SequenceEqual(
            ["2026-09-18", "2026-09-21", "2026-09-25"],
            saved.ScheduledQuests.Select(item => item.ScheduledDate));
        AssertEx.SequenceEqual([0, 1, 2], saved.ScheduledQuests.Select(item => item.SortOrder));

        var copies = saved.ScheduledQuests
            .Where(item => item.Text == source.Text)
            .OrderBy(item => item.ScheduledDate, StringComparer.Ordinal)
            .ToList();
        AssertEx.Equal(2, copies.Count);
        AssertEx.SequenceEqual(["2026-09-18", "2026-09-25"], copies.Select(item => item.ScheduledDate));
        AssertEx.True(copies.All(item => item.Id != Guid.Empty && item.Id != source.Id));
        AssertEx.Equal(2, copies.Select(item => item.Id).Distinct().Count());
        AssertEx.True(copies.All(item => item.CreatedAt == now));
        AssertEx.True(copies.All(item => item.PlannedDurationMinutes == 40));
        AssertEx.True(copies.All(item => item.LabelId == labelId));
        AssertEx.Equal(existingScheduledId, saved.ScheduledQuests[1].Id);
    }

    private static void CopyRejectsInvalidDestinationsAndForeignQuests()
    {
        var now = new DateTimeOffset(2026, 9, 17, 16, 0, 0, TimeSpan.FromHours(7));
        var sourceId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-17",
            Items =
            [
                CreateItemState(sourceId, "Owned quest", false, 0, now.AddMinutes(-1))
            ]
        });
        var viewModel = new MainViewModel(store, () => now);
        var source = viewModel.Items.Single();
        var foreign = new ChecklistItem(Guid.NewGuid(), "Foreign quest", false, now);
        var before = JsonSerializer.Serialize(AssertEx.NotNull(store.Snapshot));

        AssertEx.False(viewModel.CopyItemCommand.CanExecute(null));
        AssertEx.False(viewModel.CopyItemCommand.CanExecute(new QuestCopyRequest(source, -1)));
        AssertEx.False(viewModel.CopyItemCommand.CanExecute(new QuestCopyRequest(source, 9)));
        AssertEx.False(viewModel.CopyItemCommand.CanExecute(new QuestCopyRequest(foreign, 0)));
        AssertEx.True(viewModel.CopyItemCommand.CanExecute(new QuestCopyRequest(source, 0)));

        AssertEx.False(viewModel.CopyItemTo(source, -1));
        AssertEx.False(viewModel.CopyItemTo(source, 9));
        AssertEx.False(viewModel.CopyItemTo(foreign, 0));
        viewModel.CopyItemCommand.Execute(new QuestCopyRequest(source, -1));
        viewModel.CopyItemCommand.Execute(new QuestCopyRequest(source, 9));
        viewModel.CopyItemCommand.Execute(new QuestCopyRequest(foreign, 0));
        viewModel.CopyItemCommand.Execute("not a copy request");

        AssertEx.Equal(0, store.SaveCount);
        AssertEx.Equal(before, JsonSerializer.Serialize(AssertEx.NotNull(store.Snapshot)));
        AssertEx.SequenceEqual([sourceId], viewModel.Items.Select(item => item.Id));
        AssertEx.Equal(0, viewModel.UpcomingQuests.Count);
    }

    private static void CopyingTodaySchedulePreservesEveryDefinitionWithoutMutatingToday()
    {
        var now = new DateTimeOffset(2026, 9, 17, 16, 30, 0, TimeSpan.FromHours(7));
        var pendingId = Guid.NewGuid();
        var runningId = Guid.NewGuid();
        var overtimeId = Guid.NewGuid();
        var completedId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var activeStates = new List<ChecklistItemState>
        {
            CreateItemState(pendingId, "Pending", false, 0, now.AddHours(-4)),
            CreateItemState(
                runningId,
                "Running",
                false,
                1,
                now.AddHours(-3),
                plannedDurationMinutes: 20,
                remainingSeconds: 900,
                timerStartedAt: now,
                labelId: labelId),
            CreateItemState(
                overtimeId,
                "Overtime",
                false,
                2,
                now.AddHours(-2),
                plannedDurationMinutes: 10,
                remainingSeconds: 0,
                labelId: labelId,
                isOvertime: true,
                overtimeSeconds: 42),
            CreateItemState(
                completedId,
                "Completed",
                true,
                3,
                now.AddHours(-1),
                plannedDurationMinutes: 15,
                remainingSeconds: 120,
                labelId: labelId)
        };
        var historyStates = activeStates
            .Select(item => CreateItemState(
                item.Id,
                item.Text,
                item.IsCompleted,
                item.SortOrder,
                item.CreatedAt,
                item.PlannedDurationMinutes,
                item.RemainingSeconds,
                labelId: item.LabelId,
                manualSortOrder: item.ManualSortOrder))
            .ToList();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-17",
            Items = activeStates,
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-17",
                    Items = historyStates
                }
            ],
            Labels =
            [
                new QuestLabelState
                {
                    Id = labelId,
                    Name = "Focus",
                    ColorHex = "#5E7FA3",
                    SortOrder = 0
                }
            ],
            Settings = new AppSettings { OvertimeEnabled = true }
        });
        var viewModel = new MainViewModel(store, () => now);
        var savesBeforeCopy = store.SaveCount;
        var before = AssertEx.NotNull(store.Snapshot);
        var itemsBefore = JsonSerializer.Serialize(before.Items);
        var historyBefore = JsonSerializer.Serialize(before.History);
        var request = new ScheduleDayCopyRequest(0, 3);

        AssertEx.True(viewModel.CopyScheduleDayCommand.CanExecute(request));
        viewModel.CopyScheduleDayCommand.Execute(request);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(savesBeforeCopy + 1, store.SaveCount);
        AssertEx.Equal(itemsBefore, JsonSerializer.Serialize(saved.Items));
        AssertEx.Equal(historyBefore, JsonSerializer.Serialize(saved.History));
        AssertEx.True(viewModel.Items.Single(item => item.Id == runningId).IsTimerRunning);
        AssertEx.True(viewModel.Items.Single(item => item.Id == overtimeId).IsOvertime);
        AssertEx.True(viewModel.Items.Single(item => item.Id == completedId).IsCompleted);

        var copies = saved.ScheduledQuests
            .Where(item => item.ScheduledDate == "2026-09-20")
            .ToList();
        AssertEx.Equal(4, copies.Count);
        AssertEx.SequenceEqual(
            ["Pending", "Running", "Overtime", "Completed"],
            copies.Select(item => item.Text));
        AssertEx.SequenceEqual<int?>([null, 20, 10, 15], copies.Select(item => item.PlannedDurationMinutes));
        AssertEx.SequenceEqual<Guid?>([null, labelId, labelId, labelId], copies.Select(item => item.LabelId));
        AssertEx.True(copies.All(item => item.Id != Guid.Empty));
        AssertEx.Equal(4, copies.Select(item => item.Id).Distinct().Count());
        AssertEx.True(copies.All(item => item.CreatedAt == now));
        AssertEx.True(copies.All(item => !viewModel.Items.Any(source => source.Id == item.Id)));
    }

    private static void CopyingFutureScheduleToTodayCreatesFreshActiveQuests()
    {
        var now = new DateTimeOffset(2026, 9, 17, 17, 0, 0, TimeSpan.FromHours(7));
        var existingId = Guid.NewGuid();
        var timedSourceId = Guid.NewGuid();
        var untimedSourceId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var existingState = CreateItemState(
            existingId,
            "Existing today",
            true,
            0,
            now.AddHours(-1));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-17",
            Items = [existingState],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-17",
                    Items = [existingState]
                }
            ],
            ScheduledQuests =
            [
                CreateScheduledQuestState(
                    timedSourceId,
                    "Timed future",
                    "2026-09-19",
                    0,
                    now.AddMinutes(-2),
                    plannedDurationMinutes: 25,
                    labelId: labelId),
                CreateScheduledQuestState(
                    untimedSourceId,
                    "Untimed future",
                    "2026-09-19",
                    1,
                    now.AddMinutes(-1))
            ],
            Labels =
            [
                new QuestLabelState
                {
                    Id = labelId,
                    Name = "Important",
                    ColorHex = "#D95757",
                    SortOrder = 0
                }
            ]
        });
        var viewModel = new MainViewModel(store, () => now);
        var savesBeforeCopy = store.SaveCount;
        var scheduleBefore = JsonSerializer.Serialize(
            AssertEx.NotNull(store.Snapshot).ScheduledQuests);

        AssertEx.Equal(2, viewModel.CopyScheduleDayTo(2, 0));

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(savesBeforeCopy + 1, store.SaveCount);
        AssertEx.Equal(scheduleBefore, JsonSerializer.Serialize(saved.ScheduledQuests));
        AssertEx.Equal(3, viewModel.Items.Count);
        AssertEx.True(viewModel.Items.Single(item => item.Id == existingId).IsCompleted);

        var timedCopy = viewModel.Items.Single(item => item.Text == "Timed future");
        AssertEx.True(timedCopy.Id != Guid.Empty && timedCopy.Id != timedSourceId);
        AssertEx.False(timedCopy.IsCompleted);
        AssertEx.Equal(25, timedCopy.PlannedDurationMinutes);
        AssertEx.Equal(25 * 60, timedCopy.RemainingSeconds);
        AssertEx.Equal<DateTimeOffset?>(null, timedCopy.TimerStartedAt);
        AssertEx.False(timedCopy.IsOvertime);
        AssertEx.Equal(labelId, timedCopy.LabelId);
        AssertEx.Equal("Important", timedCopy.LabelName);

        var untimedCopy = viewModel.Items.Single(item => item.Text == "Untimed future");
        AssertEx.True(untimedCopy.Id != Guid.Empty && untimedCopy.Id != untimedSourceId);
        AssertEx.False(untimedCopy.IsCompleted);
        AssertEx.Equal<int?>(null, untimedCopy.PlannedDurationMinutes);
        AssertEx.False(untimedCopy.IsTimerRunning);
        AssertEx.SequenceEqual(
            viewModel.Items.Select(item => item.Id),
            HistoryFor(saved, "2026-09-17").Items.Select(item => item.Id));
    }

    private static void CopyingBetweenFutureDatesUsesOnlySelectedSourceDay()
    {
        var now = new DateTimeOffset(2026, 9, 17, 17, 30, 0, TimeSpan.FromHours(7));
        var firstSourceId = Guid.NewGuid();
        var secondSourceId = Guid.NewGuid();
        var decoyId = Guid.NewGuid();
        var existingTargetId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-17",
            ScheduledQuests =
            [
                CreateScheduledQuestState(firstSourceId, "Source A", "2026-09-18", 0, now.AddMinutes(-4), 5),
                CreateScheduledQuestState(secondSourceId, "Source B", "2026-09-18", 1, now.AddMinutes(-3), 10),
                CreateScheduledQuestState(decoyId, "Different day", "2026-09-19", 2, now.AddMinutes(-2), 15),
                CreateScheduledQuestState(existingTargetId, "Existing target", "2026-09-21", 3, now.AddMinutes(-1), 20)
            ]
        });
        var viewModel = new MainViewModel(store, () => now);
        var savesBeforeCopy = store.SaveCount;

        AssertEx.Equal(2, viewModel.CopyScheduleDayTo(1, 4));

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(savesBeforeCopy + 1, store.SaveCount);
        AssertEx.SequenceEqual(
            [firstSourceId, secondSourceId],
            saved.ScheduledQuests
                .Where(item => item.ScheduledDate == "2026-09-18")
                .Select(item => item.Id));
        AssertEx.SequenceEqual(
            [decoyId],
            saved.ScheduledQuests
                .Where(item => item.ScheduledDate == "2026-09-19")
                .Select(item => item.Id));

        var target = saved.ScheduledQuests
            .Where(item => item.ScheduledDate == "2026-09-21")
            .ToList();
        AssertEx.SequenceEqual(
            ["Existing target", "Source A", "Source B"],
            target.Select(item => item.Text));
        AssertEx.Equal(existingTargetId, target[0].Id);
        AssertEx.True(target[1].Id != firstSourceId && target[1].Id != Guid.Empty);
        AssertEx.True(target[2].Id != secondSourceId && target[2].Id != Guid.Empty);
        AssertEx.SequenceEqual<int?>([20, 5, 10], target.Select(item => item.PlannedDurationMinutes));
        AssertEx.Equal(0, saved.Items.Count);
        AssertEx.Equal(0, saved.History.Count);
    }

    private static void SameDayScheduleCopiesUseFiniteSourceSnapshot()
    {
        var now = new DateTimeOffset(2026, 9, 17, 18, 0, 0, TimeSpan.FromHours(7));
        var todayA = CreateItemState(Guid.NewGuid(), "Today A", false, 0, now.AddMinutes(-4));
        var todayB = CreateItemState(Guid.NewGuid(), "Today B", false, 1, now.AddMinutes(-3), 15);
        var futureA = CreateScheduledQuestState(
            Guid.NewGuid(), "Future A", "2026-09-18", 0, now.AddMinutes(-2), 20);
        var futureB = CreateScheduledQuestState(
            Guid.NewGuid(), "Future B", "2026-09-18", 1, now.AddMinutes(-1));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-17",
            Items = [todayA, todayB],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-17",
                    Items = [todayA, todayB]
                }
            ],
            ScheduledQuests = [futureA, futureB]
        });
        var viewModel = new MainViewModel(store, () => now);
        var savesBeforeCopy = store.SaveCount;

        AssertEx.Equal(2, viewModel.CopyScheduleDayTo(0, 0));
        AssertEx.Equal(2, viewModel.CopyScheduleDayTo(1, 1));

        AssertEx.Equal(savesBeforeCopy + 2, store.SaveCount);
        AssertEx.SequenceEqual(
            ["Today A", "Today B", "Today A", "Today B"],
            viewModel.Items.Select(item => item.Text));
        AssertEx.Equal(4, viewModel.Items.Select(item => item.Id).Distinct().Count());

        var saved = AssertEx.NotNull(store.Snapshot);
        var future = saved.ScheduledQuests
            .Where(item => item.ScheduledDate == "2026-09-18")
            .ToList();
        AssertEx.SequenceEqual(
            ["Future A", "Future B", "Future A", "Future B"],
            future.Select(item => item.Text));
        AssertEx.Equal(4, future.Select(item => item.Id).Distinct().Count());
        AssertEx.Equal(4, HistoryFor(saved, "2026-09-17").Items.Count);
    }

    private static void EmptyAndInvalidScheduleCopiesDoNotSave()
    {
        var now = new DateTimeOffset(2026, 9, 17, 18, 30, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-17"
        });
        var viewModel = new MainViewModel(store, () => now);
        var savesBeforeCopy = store.SaveCount;
        var before = JsonSerializer.Serialize(AssertEx.NotNull(store.Snapshot));

        AssertEx.False(viewModel.HasScheduleDayQuests(-1));
        AssertEx.False(viewModel.HasScheduleDayQuests(0));
        AssertEx.False(viewModel.HasScheduleDayQuests(1));
        AssertEx.False(viewModel.HasScheduleDayQuests(9));
        AssertEx.False(viewModel.CopyScheduleDayCommand.CanExecute(null));
        AssertEx.False(viewModel.CopyScheduleDayCommand.CanExecute(new ScheduleDayCopyRequest(-1, 0)));
        AssertEx.False(viewModel.CopyScheduleDayCommand.CanExecute(new ScheduleDayCopyRequest(0, 9)));
        AssertEx.False(viewModel.CopyScheduleDayCommand.CanExecute(new ScheduleDayCopyRequest(0, 1)));
        AssertEx.False(viewModel.CopyScheduleDayCommand.CanExecute(new ScheduleDayCopyRequest(1, 0)));

        AssertEx.Equal(0, viewModel.CopyScheduleDayTo(-1, 0));
        AssertEx.Equal(0, viewModel.CopyScheduleDayTo(0, 9));
        AssertEx.Equal(0, viewModel.CopyScheduleDayTo(0, 1));
        AssertEx.Equal(0, viewModel.CopyScheduleDayTo(1, 0));
        viewModel.CopyScheduleDayCommand.Execute(new ScheduleDayCopyRequest(-1, 0));
        viewModel.CopyScheduleDayCommand.Execute(new ScheduleDayCopyRequest(0, 9));
        viewModel.CopyScheduleDayCommand.Execute(new ScheduleDayCopyRequest(0, 1));
        viewModel.CopyScheduleDayCommand.Execute(new ScheduleDayCopyRequest(1, 0));
        viewModel.CopyScheduleDayCommand.Execute("not a schedule copy request");

        AssertEx.Equal(savesBeforeCopy, store.SaveCount);
        AssertEx.Equal(before, JsonSerializer.Serialize(AssertEx.NotNull(store.Snapshot)));
        AssertEx.Equal(0, viewModel.Items.Count);
        AssertEx.Equal(0, viewModel.UpcomingQuests.Count);
    }

    private static void ScheduledQuestsStayOutsideTodayUntilDue()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var activeId = Guid.NewGuid();
        var activeState = CreateItemState(activeId, "Quest hari ini", false, 0, now.AddMinutes(-1));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items = [activeState],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-16",
                    Items = [activeState]
                }
            ]
        });
        var viewModel = new MainViewModel(store, () => now);

        viewModel.SetScheduleOffsetCommand.Execute(1);
        viewModel.NewItemText = "Quest untuk besok";
        viewModel.AddItemCommand.Execute(null);

        AssertEx.SequenceEqual([activeId], viewModel.Items.Select(item => item.Id));
        AssertEx.Equal(1, viewModel.TotalCount);
        AssertEx.Equal(activeId, AssertEx.NotNull(viewModel.NextPendingItem).Id);
        AssertEx.Equal(1, viewModel.UpcomingQuests.Count);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.SequenceEqual([activeId], saved.Items.Select(item => item.Id));
        AssertEx.SequenceEqual(
            [activeId],
            HistoryFor(saved, "2026-09-16").Items.Select(item => item.Id));
        AssertEx.Equal("2026-09-17", saved.ScheduledQuests.Single().ScheduledDate);
        AssertEx.Equal(1, store.SaveCount);
    }

    private static void UpcomingQuestsCanBeCanceledWithoutChangingToday()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var activeId = Guid.NewGuid();
        var scheduledId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(activeId, "Quest aktif", false, 0, now.AddMinutes(-2))
            ],
            ScheduledQuests =
            [
                CreateScheduledQuestState(
                    scheduledId,
                    "Quest yang dibatalkan",
                    "2026-09-18",
                    0,
                    now.AddMinutes(-1))
            ]
        });
        var viewModel = new MainViewModel(store, () => now);

        viewModel.ShowUpcomingCommand.Execute(null);
        AssertEx.True(viewModel.IsUpcomingView, "The upcoming page should open before cancellation.");
        AssertEx.False(viewModel.IsTodayView);
        AssertEx.False(viewModel.IsHistoryView);
        AssertEx.False(viewModel.IsSettingsView);

        var upcoming = viewModel.UpcomingQuests.Single();
        AssertEx.True(
            viewModel.RemoveScheduledQuestCommand.CanExecute(upcoming),
            "A queued quest should be cancellable.");
        viewModel.RemoveScheduledQuestCommand.Execute(upcoming);

        AssertEx.Equal(0, viewModel.UpcomingQuests.Count);
        AssertEx.SequenceEqual([activeId], viewModel.Items.Select(item => item.Id));
        AssertEx.Equal(1, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(0, saved.ScheduledQuests.Count);
        AssertEx.SequenceEqual([activeId], saved.Items.Select(item => item.Id));
        AssertEx.Equal(0, saved.History.Count);
    }

    private static void DurationSelectionSupportsCustomValuesAndNoTimer()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Settings = new AppSettings
            {
                LanguageCode = "en-US",
                ThemeCode = "light"
            }
        });
        var viewModel = new MainViewModel(store, () => now);

        AssertEx.SequenceEqual([0, 5, 10, 15, 25, 30, 45, 60], viewModel.DurationOptions);
        AssertEx.False(viewModel.SelectedDurationMinutes.HasValue);
        AssertEx.Equal("No timer", viewModel.SelectedDurationLabel);

        viewModel.SetDurationCommand.Execute("37");

        AssertEx.Equal(37, viewModel.SelectedDurationMinutes);
        AssertEx.True(viewModel.HasSelectedDuration);
        AssertEx.Equal("37 min", viewModel.SelectedDurationLabel);

        viewModel.SetDurationCommand.Execute("481");
        viewModel.SetDurationCommand.Execute(-1);
        AssertEx.Equal(37, viewModel.SelectedDurationMinutes);

        viewModel.NewItemText = "Write focused draft";
        viewModel.AddItemCommand.Execute(null);

        var item = viewModel.Items.Single();
        AssertEx.True(item.HasTimer);
        AssertEx.Equal(37, item.PlannedDurationMinutes);
        AssertEx.Equal(37 * 60, item.RemainingSeconds);
        AssertEx.Equal("37:00", item.RemainingTimeText);
        AssertEx.False(viewModel.SelectedDurationMinutes.HasValue);
        AssertEx.Equal("No timer", viewModel.SelectedDurationLabel);

        var savedItem = AssertEx.NotNull(store.Snapshot).Items.Single();
        AssertEx.Equal(37, savedItem.PlannedDurationMinutes);
        AssertEx.Equal(37 * 60, savedItem.RemainingSeconds);
        AssertEx.False(savedItem.TimerStartedAt.HasValue);

        viewModel.SetDurationCommand.Execute(15);
        viewModel.SetDurationCommand.Execute(0);
        AssertEx.False(viewModel.SelectedDurationMinutes.HasValue);
    }

    private static void ScheduledTimerDurationSurvivesActivation()
    {
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7)));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Settings = new AppSettings
            {
                LanguageCode = "en-US",
                ThemeCode = "light"
            }
        });
        var viewModel = new MainViewModel(store, () => clock.Now);

        viewModel.SetDurationCommand.Execute("45");
        viewModel.SetScheduleOffsetCommand.Execute(2);
        viewModel.NewItemText = "Prepare launch notes";
        viewModel.AddItemCommand.Execute(null);

        var upcoming = viewModel.UpcomingQuests.Single();
        AssertEx.Equal(45, upcoming.PlannedDurationMinutes);
        AssertEx.True(upcoming.HasTimer);
        AssertEx.Equal("45 min", upcoming.DurationText);
        AssertEx.Equal(45, AssertEx.NotNull(store.Snapshot).ScheduledQuests.Single().PlannedDurationMinutes);

        clock.Now = clock.Now.AddDays(2);
        AssertEx.True(viewModel.RollOverToCurrentDay());

        var activated = viewModel.Items.Single();
        AssertEx.Equal("Prepare launch notes", activated.Text);
        AssertEx.Equal(45, activated.PlannedDurationMinutes);
        AssertEx.Equal(45 * 60, activated.RemainingSeconds);
        AssertEx.False(activated.IsTimerRunning);
        AssertEx.Equal(0, viewModel.UpcomingQuests.Count);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(45, saved.Items.Single().PlannedDurationMinutes);
        AssertEx.Equal(45 * 60, saved.Items.Single().RemainingSeconds);
        AssertEx.Equal(0, saved.ScheduledQuests.Count);
    }

    private static void TimerStartsPausesResumesResetsAndPersists()
    {
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7)));
        var itemId = Guid.NewGuid();
        var alarm = new RecordingQuestAlarmService();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(
                    itemId,
                    "Deep work",
                    false,
                    0,
                    clock.Now,
                    plannedDurationMinutes: 2,
                    remainingSeconds: 120)
            ]
        });
        var viewModel = new MainViewModel(store, () => clock.Now, null, null, alarm);
        var item = viewModel.Items.Single();

        AssertEx.True(viewModel.ToggleTimerCommand.CanExecute(item));
        viewModel.ToggleTimerCommand.Execute(item);

        AssertEx.True(item.IsTimerRunning);
        AssertEx.Equal(clock.Now, item.TimerStartedAt);
        AssertEx.Equal(itemId, AssertEx.NotNull(viewModel.ActiveTimerItem).Id);
        AssertEx.True(viewModel.HasActiveTimer);
        AssertEx.Equal(1, store.SaveCount);

        clock.Now = clock.Now.AddSeconds(30.4);
        AssertEx.True(viewModel.TickTimers());
        AssertEx.Equal(90, item.RemainingSeconds);
        AssertEx.Equal("01:30", item.RemainingTimeText);
        AssertEx.Equal(25d, item.TimerProgressPercent);
        AssertEx.Equal(1, store.SaveCount);

        clock.Now = clock.Now.AddSeconds(1);
        viewModel.ToggleTimerCommand.Execute(item);

        AssertEx.False(item.IsTimerRunning);
        AssertEx.Equal(89, item.RemainingSeconds);
        AssertEx.False(viewModel.HasActiveTimer);
        var paused = AssertEx.NotNull(store.Snapshot).Items.Single();
        AssertEx.Equal(89, paused.RemainingSeconds);
        AssertEx.False(paused.TimerStartedAt.HasValue);

        clock.Now = clock.Now.AddMinutes(5);
        viewModel.ToggleTimerCommand.Execute(item);
        clock.Now = clock.Now.AddSeconds(10);
        viewModel.TickTimers();
        AssertEx.Equal(79, item.RemainingSeconds);

        viewModel.ResetTimerCommand.Execute(item);

        AssertEx.False(item.IsTimerRunning);
        AssertEx.Equal(120, item.RemainingSeconds);
        AssertEx.Equal("02:00", item.RemainingTimeText);
        AssertEx.Equal(0d, item.TimerProgressPercent);
        AssertEx.False(viewModel.ResetTimerCommand.CanExecute(item));
        AssertEx.Equal(0, alarm.Notifications.Count);

        var reset = AssertEx.NotNull(store.Snapshot).Items.Single();
        AssertEx.Equal(120, reset.RemainingSeconds);
        AssertEx.False(reset.TimerStartedAt.HasValue);
    }

    private static void EmbeddedRingtoneIsValidExactAndCappedAtOneMinute()
    {
        AssertEx.Equal(TimeSpan.FromMinutes(1), WindowsQuestAlarmService.MaximumAlarmDuration);
        var ringtoneRequired = string.Equals(
            Environment.GetEnvironmentVariable("DAILYQUEST_REQUIRE_RINGTONE"),
            "1",
            StringComparison.Ordinal);
        AssertEx.True(
            !ringtoneRequired || WindowsQuestAlarmService.HasEmbeddedRingtone,
            "Official release validation requires the licensed ringtone to be embedded.");
        if (!WindowsQuestAlarmService.HasEmbeddedRingtone)
        {
            return;
        }

        var wave = WindowsQuestAlarmService.LoadAlarmWave();

        AssertEx.True(wave.Length > 44, "The generated alarm must contain PCM samples.");
        AssertEx.Equal(3_173_020, wave.Length);
        AssertEx.Equal(
            "75F14A2044AF42630DE43FEA45ED720988FEC1345EB7EF688A413EAF24DB5A7B",
            Convert.ToHexString(SHA256.HashData(wave)));
        AssertEx.Equal("RIFF", Encoding.ASCII.GetString(wave, 0, 4));
        AssertEx.Equal("WAVE", Encoding.ASCII.GetString(wave, 8, 4));
        AssertEx.Equal("fmt ", Encoding.ASCII.GetString(wave, 12, 4));
        AssertEx.Equal((short)1, BitConverter.ToInt16(wave, 20));
        AssertEx.Equal((short)2, BitConverter.ToInt16(wave, 22));
        var sampleRate = BitConverter.ToInt32(wave, 24);
        AssertEx.Equal(44_100, sampleRate);
        AssertEx.Equal((short)16, BitConverter.ToInt16(wave, 34));
        AssertEx.Equal("data", Encoding.ASCII.GetString(wave, 36, 4));

        var dataLength = BitConverter.ToInt32(wave, 40);
        AssertEx.Equal(wave.Length - 44, dataLength);
        var loopDurationSeconds = dataLength / (double)(sampleRate * 2 * sizeof(short));
        AssertEx.True(
            loopDurationSeconds >= 17.9d && loopDurationSeconds <= 18.1d,
            "The embedded ringtone should retain its original duration.");
        AssertEx.True(
            wave.AsSpan(44).ContainsAnyExcept((byte)0),
            "The alarm wave must contain audible, non-silent samples.");
        var peakSample = 0;
        for (var offset = 44; offset < wave.Length; offset += sizeof(short))
        {
            peakSample = Math.Max(peakSample, Math.Abs((int)BitConverter.ToInt16(wave, offset)));
        }

        AssertEx.True(
            peakSample >= 15_000,
            "The alarm should use a strong enough signal to be attention-grabbing at the user's Windows volume.");
        AssertEx.Equal(
            "DailyQuest.Assets.Ringtone.FacilityAlarm.wav",
            WindowsQuestAlarmService.RingtoneResourceName);
    }

    private static void StartingTimerPausesOtherActiveTimer()
    {
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 16, 9, 0, 0, TimeSpan.FromHours(7)));
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(firstId, "First timer", false, 0, clock.Now, 5, 300),
                CreateItemState(secondId, "Second timer", false, 1, clock.Now, 3, 180)
            ]
        });
        var viewModel = new MainViewModel(store, () => clock.Now);
        var first = viewModel.Items.Single(item => item.Id == firstId);
        var second = viewModel.Items.Single(item => item.Id == secondId);

        viewModel.ToggleTimerCommand.Execute(first);
        clock.Now = clock.Now.AddSeconds(10);
        viewModel.ToggleTimerCommand.Execute(second);

        AssertEx.False(first.IsTimerRunning);
        AssertEx.Equal(290, first.RemainingSeconds);
        AssertEx.True(second.IsTimerRunning);
        AssertEx.Equal(secondId, AssertEx.NotNull(viewModel.ActiveTimerItem).Id);
        AssertEx.Equal(
            secondId,
            AssertEx.NotNull(viewModel.CompactDisplayItem).Id);
        AssertEx.Equal(1, viewModel.Items.Count(item => item.IsTimerRunning));

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(1, saved.Items.Count(item => item.TimerStartedAt.HasValue));
        AssertEx.Equal(290, saved.Items.Single(item => item.Id == firstId).RemainingSeconds);
        AssertEx.Equal(clock.Now, saved.Items.Single(item => item.Id == secondId).TimerStartedAt);

        viewModel.ToggleTimerCommand.Execute(second);
        AssertEx.Equal(
            firstId,
            AssertEx.NotNull(viewModel.CompactDisplayItem).Id);
    }

    private static void RunningTimerRestoresFromTimestampAndAlarmsOnce()
    {
        var startedAt = new DateTimeOffset(2026, 9, 16, 10, 0, 0, TimeSpan.FromHours(7));
        var clock = new MutableClock(startedAt.AddSeconds(45));
        var itemId = Guid.NewGuid();
        var alarm = new RecordingQuestAlarmService();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(
                    itemId,
                    "Tea break",
                    false,
                    0,
                    startedAt,
                    plannedDurationMinutes: 1,
                    remainingSeconds: 60,
                    timerStartedAt: startedAt)
            ]
        });

        var viewModel = new MainViewModel(store, () => clock.Now, null, null, alarm);
        var item = viewModel.Items.Single();

        AssertEx.True(item.IsTimerRunning);
        AssertEx.Equal(15, item.RemainingSeconds);
        AssertEx.Equal("00:15", item.RemainingTimeText);
        AssertEx.Equal(0, alarm.Notifications.Count);
        AssertEx.Equal(0, store.SaveCount);

        clock.Now = startedAt.AddSeconds(61);
        AssertEx.True(viewModel.TickTimers());

        AssertEx.False(item.IsTimerRunning);
        AssertEx.True(item.IsTimerExpired);
        AssertEx.Equal(0, item.RemainingSeconds);
        AssertEx.SequenceEqual(["Tea break"], alarm.Notifications);
        AssertEx.Equal(1, store.SaveCount);

        AssertEx.False(viewModel.TickTimers());
        AssertEx.SequenceEqual(["Tea break"], alarm.Notifications);
        AssertEx.Equal(1, store.SaveCount);

        var reloadAlarm = new RecordingQuestAlarmService();
        var reloadStore = new InMemoryStateStore(AssertEx.NotNull(store.Snapshot));
        var reloaded = new MainViewModel(reloadStore, () => clock.Now, null, null, reloadAlarm);

        AssertEx.Equal(0, reloaded.Items.Single().RemainingSeconds);
        AssertEx.False(reloaded.Items.Single().IsTimerRunning);
        AssertEx.Equal(0, reloadAlarm.Notifications.Count);
        AssertEx.Equal(0, reloadStore.SaveCount);
    }

    private static void CompletionPausesTimerAndDailyRolloverResetsIt()
    {
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 16, 11, 0, 0, TimeSpan.FromHours(7)));
        var itemId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(itemId, "Timed quest", false, 0, clock.Now, 2, 120)
            ]
        });
        var viewModel = new MainViewModel(store, () => clock.Now);
        var item = viewModel.Items.Single();

        viewModel.ToggleTimerCommand.Execute(item);
        clock.Now = clock.Now.AddSeconds(10);
        item.IsCompleted = true;

        AssertEx.True(item.IsCompleted);
        AssertEx.False(item.IsTimerRunning);
        AssertEx.Equal(110, item.RemainingSeconds);
        AssertEx.False(viewModel.ToggleTimerCommand.CanExecute(item));

        clock.Now = clock.Now.AddDays(1);
        AssertEx.True(viewModel.RollOverToCurrentDay());

        AssertEx.False(item.IsCompleted);
        AssertEx.False(item.IsTimerRunning);
        AssertEx.Equal(120, item.RemainingSeconds);
        AssertEx.True(viewModel.ToggleTimerCommand.CanExecute(item));

        var saved = AssertEx.NotNull(store.Snapshot).Items.Single();
        AssertEx.False(saved.IsCompleted);
        AssertEx.Equal(120, saved.RemainingSeconds);
        AssertEx.False(saved.TimerStartedAt.HasValue);
    }

    private static void MidnightRolloverResolvesTimersBeforeResettingDay()
    {
        var startedAt = new DateTimeOffset(2026, 9, 16, 23, 58, 0, TimeSpan.FromHours(7));
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 17, 0, 1, 0, TimeSpan.FromHours(7)));
        var elapsedId = Guid.NewGuid();
        var continuingId = Guid.NewGuid();
        var alarm = new RecordingQuestAlarmService();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(elapsedId, "Short timer", false, 0, startedAt, 1, 60, startedAt),
                CreateItemState(continuingId, "Long timer", false, 1, startedAt, 10, 600)
            ]
        });

        var viewModel = new MainViewModel(store, () => clock.Now, null, null, alarm);

        var elapsed = viewModel.Items.Single(item => item.Id == elapsedId);
        AssertEx.SequenceEqual(["Short timer"], alarm.Notifications);
        AssertEx.False(elapsed.IsTimerRunning);
        AssertEx.Equal(60, elapsed.RemainingSeconds);
        AssertEx.Equal("2026-09-17", AssertEx.NotNull(store.Snapshot).CurrentDate);

        // Starting the longer timer after rollover verifies that daily reset left it usable.
        var continuing = viewModel.Items.Single(item => item.Id == continuingId);
        AssertEx.False(continuing.IsTimerRunning);
        AssertEx.Equal(600, continuing.RemainingSeconds);
        viewModel.ToggleTimerCommand.Execute(continuing);
        AssertEx.True(continuing.IsTimerRunning);

        var crossMidnightAlarm = new RecordingQuestAlarmService();
        var crossMidnightId = Guid.NewGuid();
        var crossMidnightStore = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(
                    crossMidnightId,
                    "Long running timer",
                    false,
                    0,
                    startedAt,
                    plannedDurationMinutes: 10,
                    remainingSeconds: 600,
                    timerStartedAt: startedAt)
            ]
        });

        var crossMidnight = new MainViewModel(
            crossMidnightStore,
            () => clock.Now,
            null,
            null,
            crossMidnightAlarm);
        var runningAcrossMidnight = crossMidnight.Items.Single();

        AssertEx.True(runningAcrossMidnight.IsTimerRunning);
        AssertEx.Equal(420, runningAcrossMidnight.RemainingSeconds);
        AssertEx.Equal(clock.Now, runningAcrossMidnight.TimerStartedAt);
        AssertEx.Equal(0, crossMidnightAlarm.Notifications.Count);
        AssertEx.Equal("2026-09-17", AssertEx.NotNull(crossMidnightStore.Snapshot).CurrentDate);
    }

    private static void OvertimeActionStopsAlarmAndCountUpPersists()
    {
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7)));
        var itemId = Guid.NewGuid();
        var alarm = new RecordingQuestAlarmService();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(
                    itemId,
                    "Timed focus",
                    false,
                    0,
                    clock.Now,
                    plannedDurationMinutes: 1,
                    remainingSeconds: 1,
                    timerStartedAt: clock.Now)
            ],
            Settings = new AppSettings { OvertimeEnabled = true }
        });
        var viewModel = new MainViewModel(store, () => clock.Now, null, null, alarm);
        var item = viewModel.Items.Single();

        clock.Now = clock.Now.AddSeconds(2);
        AssertEx.True(viewModel.TickTimers());
        AssertEx.True(item.IsTimerExpired);
        AssertEx.False(item.IsOvertime);
        AssertEx.SequenceEqual(["Timed focus"], alarm.Notifications);
        AssertEx.True(viewModel.StartOvertimeCommand.CanExecute(item));

        viewModel.StartOvertimeCommand.Execute(item);
        AssertEx.True(item.IsOvertime);
        AssertEx.True(item.IsTimerRunning);
        AssertEx.Equal(0, item.OvertimeSeconds);
        AssertEx.Equal("+01:00", item.RemainingTimeText);
        AssertEx.Equal(1, alarm.StopCount);

        clock.Now = clock.Now.AddSeconds(65);
        AssertEx.True(viewModel.TickTimers());
        AssertEx.Equal(65, item.OvertimeSeconds);
        AssertEx.Equal("+02:05", item.RemainingTimeText);

        viewModel.ToggleTimerCommand.Execute(item);
        AssertEx.False(item.IsTimerRunning);
        clock.Now = clock.Now.AddSeconds(10);
        AssertEx.False(viewModel.TickTimers());
        AssertEx.Equal(65, item.OvertimeSeconds);
        AssertEx.Equal("+02:05", item.RemainingTimeText);

        viewModel.ToggleTimerCommand.Execute(item);
        clock.Now = clock.Now.AddSeconds(5);
        AssertEx.True(viewModel.TickTimers());
        AssertEx.Equal(70, item.OvertimeSeconds);
        AssertEx.Equal("+02:10", item.RemainingTimeText);
        viewModel.Save();

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.True(saved.Settings.OvertimeEnabled);
        AssertEx.True(saved.Items.Single().IsOvertime);
        AssertEx.Equal(70, saved.Items.Single().OvertimeSeconds);
        var reloaded = new MainViewModel(new InMemoryStateStore(saved), () => clock.Now);
        var restored = reloaded.Items.Single();
        AssertEx.True(restored.IsOvertime);
        AssertEx.True(restored.IsTimerRunning);
        AssertEx.Equal(70, restored.OvertimeSeconds);
        AssertEx.Equal("+02:10", restored.RemainingTimeText);
    }

    private static void DisabledOvertimeRejectsCountUpAndAlarmCanStop()
    {
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7)));
        var alarm = new RecordingQuestAlarmService();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(
                    Guid.NewGuid(),
                    "Finite alarm",
                    false,
                    0,
                    clock.Now,
                    plannedDurationMinutes: 1,
                    remainingSeconds: 1,
                    timerStartedAt: clock.Now)
            ]
        });
        var viewModel = new MainViewModel(store, () => clock.Now, null, null, alarm);
        var item = viewModel.Items.Single();

        clock.Now = clock.Now.AddSeconds(2);
        viewModel.TickTimers();

        AssertEx.SequenceEqual(["Finite alarm"], alarm.Notifications);
        AssertEx.False(viewModel.OvertimeEnabled);
        AssertEx.False(viewModel.StartOvertimeCommand.CanExecute(item));
        viewModel.StartOvertimeCommand.Execute(item);
        AssertEx.False(item.IsOvertime);
        viewModel.ResetTimerCommand.Execute(item);
        AssertEx.Equal(1, alarm.StopCount);
        AssertEx.Equal(60, item.RemainingSeconds);

        var inconsistentStore = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(
                    Guid.NewGuid(),
                    "Invalid overtime",
                    false,
                    0,
                    clock.Now,
                    plannedDurationMinutes: 1,
                    remainingSeconds: 0,
                    timerStartedAt: clock.Now,
                    isOvertime: true,
                    overtimeSeconds: 50)
            ],
            Settings = new AppSettings { OvertimeEnabled = false }
        });
        var normalized = new MainViewModel(inconsistentStore, () => clock.Now);
        var normalizedItem = normalized.Items.Single();
        AssertEx.False(normalizedItem.IsOvertime);
        AssertEx.Equal(0, normalizedItem.OvertimeSeconds);
        AssertEx.False(normalizedItem.IsTimerRunning);
        AssertEx.True(normalizedItem.IsTimerExpired);
        AssertEx.Equal(1, inconsistentStore.SaveCount);
    }

    private static void OvertimeClearsOnCompletionResetAndSettingDisable()
    {
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7)));
        var itemId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(
                    itemId,
                    "Overtime",
                    false,
                    0,
                    clock.Now,
                    plannedDurationMinutes: 1,
                    remainingSeconds: 0,
                    timerStartedAt: clock.Now,
                    isOvertime: true,
                    overtimeSeconds: 12)
            ],
            Settings = new AppSettings { OvertimeEnabled = true }
        });
        var viewModel = new MainViewModel(store, () => clock.Now);
        var item = viewModel.Items.Single();

        item.IsCompleted = true;
        AssertEx.False(item.IsOvertime);
        AssertEx.Equal(0, item.OvertimeSeconds);
        AssertEx.False(item.IsTimerRunning);

        viewModel.ResetTodayCommand.Execute(null);
        AssertEx.Equal(60, item.RemainingSeconds);
        AssertEx.False(item.IsOvertime);

        item.IsCompleted = false;
        viewModel.ToggleTimerCommand.Execute(item);
        clock.Now = clock.Now.AddSeconds(61);
        viewModel.TickTimers();
        viewModel.StartOvertimeCommand.Execute(item);
        AssertEx.True(item.IsOvertime);
        viewModel.SetOvertimeCommand.Execute(false);
        AssertEx.False(viewModel.OvertimeEnabled);
        AssertEx.False(item.IsOvertime);
        AssertEx.False(item.IsTimerRunning);
        AssertEx.Equal(0, item.RemainingSeconds);
        AssertEx.False(viewModel.StartOvertimeCommand.CanExecute(item));
    }

    private static void OvertimeDependentPropertiesNotifyAfterResetAndCompletion()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var overtimeStore = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(
                    Guid.NewGuid(),
                    "Overtime reset notifications",
                    false,
                    0,
                    now,
                    plannedDurationMinutes: 1,
                    remainingSeconds: 0,
                    isOvertime: true,
                    overtimeSeconds: 25)
            ],
            Settings = new AppSettings { OvertimeEnabled = true }
        });
        var overtimeViewModel = new MainViewModel(overtimeStore, () => now);
        var overtimeItem = overtimeViewModel.Items.Single();
        var resetNotifications = new HashSet<string?>();
        overtimeItem.PropertyChanged += (_, eventArgs) =>
            resetNotifications.Add(eventArgs.PropertyName);

        overtimeViewModel.ResetTimerCommand.Execute(overtimeItem);

        AssertEx.False(overtimeItem.IsOvertime);
        AssertEx.Equal(0, overtimeItem.OvertimeSeconds);
        AssertEx.True(
            resetNotifications.Contains(nameof(ChecklistItem.IsOvertime)),
            "Resetting overtime should notify the red overtime presentation trigger.");
        AssertEx.True(
            resetNotifications.Contains(nameof(ChecklistItem.OvertimeSeconds)),
            "Resetting a nonzero overtime counter should notify its dependent display.");

        var expiredStore = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(
                    Guid.NewGuid(),
                    "Expired completion notifications",
                    false,
                    0,
                    now,
                    plannedDurationMinutes: 1,
                    remainingSeconds: 0)
            ],
            Settings = new AppSettings { OvertimeEnabled = true }
        });
        var expiredViewModel = new MainViewModel(expiredStore, () => now);
        var expiredItem = expiredViewModel.Items.Single();
        var completionNotifications = new HashSet<string?>();
        expiredItem.PropertyChanged += (_, eventArgs) =>
            completionNotifications.Add(eventArgs.PropertyName);

        AssertEx.True(expiredItem.IsTimerExpired);
        AssertEx.True(expiredItem.CanStartOvertime);
        expiredItem.IsCompleted = true;

        AssertEx.False(expiredItem.IsTimerExpired);
        AssertEx.False(expiredItem.CanStartOvertime);
        AssertEx.True(
            completionNotifications.Contains(nameof(ChecklistItem.IsTimerExpired)),
            "Completing an expired quest should notify expiry-dependent visibility.");
        AssertEx.True(
            completionNotifications.Contains(nameof(ChecklistItem.CanStartOvertime)),
            "Completing an expired quest should hide its overtime action immediately.");
    }

    private static void V6MigrationDefaultsOvertimeOffWithoutAddingLabels()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 6,
            CurrentDate = "2026-09-16",
            Labels = [],
            Settings = new AppSettings { QuestSortMode = QuestSortModeCodes.Manual }
        });

        var viewModel = new MainViewModel(store, () => now);

        AssertEx.False(viewModel.OvertimeEnabled);
        AssertEx.Equal(0, viewModel.Labels.Count);
        AssertEx.Equal(1, store.SaveCount);
        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(7, saved.SchemaVersion);
        AssertEx.False(saved.Settings.OvertimeEnabled);
        AssertEx.Equal(0, saved.Labels.Count);
    }

    private static void V4MigrationExpandsOnlyLegacyDefaultWindow()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var legacyDefaultStore = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 4,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(Guid.NewGuid(), "Legacy untimed quest", false, 0, now)
            ],
            Window = new WidgetWindowState
            {
                Width = 430,
                Height = 610
            }
        });

        _ = new MainViewModel(legacyDefaultStore, () => now);

        var migratedDefault = AssertEx.NotNull(legacyDefaultStore.Snapshot);
        AssertEx.Equal(7, migratedDefault.SchemaVersion);
        AssertEx.Equal(520d, migratedDefault.Window.Width);
        AssertEx.Equal(680d, migratedDefault.Window.Height);
        AssertEx.False(migratedDefault.Items.Single().PlannedDurationMinutes.HasValue);
        AssertEx.False(migratedDefault.Items.Single().RemainingSeconds.HasValue);
        AssertEx.False(migratedDefault.Items.Single().TimerStartedAt.HasValue);

        var customStore = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 4,
            CurrentDate = "2026-09-16",
            Window = new WidgetWindowState
            {
                Width = 500,
                Height = 700
            }
        });

        _ = new MainViewModel(customStore, () => now);

        var migratedCustom = AssertEx.NotNull(customStore.Snapshot);
        AssertEx.Equal(500d, migratedCustom.Window.Width);
        AssertEx.Equal(700d, migratedCustom.Window.Height);

        var currentSchemaStore = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Window = new WidgetWindowState
            {
                Width = 430,
                Height = 610
            }
        });

        _ = new MainViewModel(currentSchemaStore, () => now);

        AssertEx.Equal(430d, AssertEx.NotNull(currentSchemaStore.Snapshot).Window.Width);
        AssertEx.Equal(610d, AssertEx.NotNull(currentSchemaStore.Snapshot).Window.Height);
        AssertEx.Equal(0, currentSchemaStore.SaveCount);
    }

    private static void NextPendingItemFollowsQuestOrderAndCompletion()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var thirdId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(firstId, "Sudah selesai", true, 0, now.AddMinutes(-3)),
                CreateItemState(secondId, "Quest berikutnya", false, 1, now.AddMinutes(-2)),
                CreateItemState(thirdId, "Quest terakhir", false, 2, now.AddMinutes(-1))
            ]
        });
        var viewModel = new MainViewModel(store, () => now);
        var changedProperties = new HashSet<string?>();
        viewModel.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName);

        AssertEx.Equal(secondId, AssertEx.NotNull(viewModel.NextPendingItem).Id);
        AssertEx.True(viewModel.HasPendingItem);

        var third = viewModel.Items.Single(item => item.Id == thirdId);
        AssertEx.True(viewModel.MoveItem(third, 0), "Moving the last pending quest to the front should succeed.");
        AssertEx.Equal(thirdId, AssertEx.NotNull(viewModel.NextPendingItem).Id);
        AssertEx.True(
            changedProperties.Contains(nameof(MainViewModel.NextPendingItem)),
            "Reordering should notify the compact quest binding.");

        third.IsCompleted = true;
        AssertEx.Equal(secondId, AssertEx.NotNull(viewModel.NextPendingItem).Id);

        var second = viewModel.Items.Single(item => item.Id == secondId);
        AssertEx.False(second.IsCompleted, "The newly exposed compact quest must remain unchecked.");
        second.IsCompleted = true;
        AssertEx.Null(viewModel.NextPendingItem);
        AssertEx.False(viewModel.HasPendingItem, "No pending quest should remain after every quest is complete.");

        var first = viewModel.Items.Single(item => item.Id == firstId);
        first.IsCompleted = false;
        AssertEx.Equal(firstId, AssertEx.NotNull(viewModel.NextPendingItem).Id);
        AssertEx.True(viewModel.HasPendingItem);
    }

    private static void CompletedQuestMovesToBottomWithoutCheckingNextQuest()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(firstId, "Quest pertama", false, 0, now.AddMinutes(-2)),
                CreateItemState(secondId, "Quest berikutnya", false, 1, now.AddMinutes(-1))
            ]
        });
        var viewModel = new MainViewModel(store, () => now);
        var first = AssertEx.NotNull(viewModel.NextPendingItem);

        viewModel.CompleteItemCommand.Execute(first);

        var second = AssertEx.NotNull(viewModel.NextPendingItem);
        AssertEx.Equal(firstId, first.Id);
        AssertEx.True(first.IsCompleted, "The clicked compact quest should be completed.");
        AssertEx.Equal(secondId, second.Id);
        AssertEx.False(second.IsCompleted, "Advancing compact mode must not complete the next quest.");
        AssertEx.SequenceEqual(
            [secondId, firstId],
            viewModel.Items.Select(item => item.Id));
        AssertEx.Equal(1, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.SequenceEqual(
            [secondId, firstId],
            saved.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([false, true], saved.Items.Select(item => item.IsCompleted));
        AssertEx.SequenceEqual(
            [secondId, firstId],
            HistoryFor(saved, "2026-09-16").Items.Select(item => item.Id));
        AssertEx.SequenceEqual(
            [false, true],
            HistoryFor(saved, "2026-09-16").Items.Select(item => item.IsCompleted));

        viewModel.CompleteItemCommand.Execute(first);
        viewModel.CompleteItemCommand.Execute(
            new ChecklistItem(Guid.NewGuid(), "Quest asing", false, now));
        viewModel.CompleteItemCommand.Execute(null);

        AssertEx.False(second.IsCompleted, "Stale commands must not affect the current compact quest.");
        AssertEx.Equal(1, store.SaveCount);
    }

    private static void ActiveQuestsRecurUncheckedAfterDailyRollover()
    {
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7)));
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(firstId, "Quest pertama", false, 0, clock.Now.AddMinutes(-2)),
                CreateItemState(secondId, "Quest kedua", false, 1, clock.Now.AddMinutes(-1))
            ]
        });
        var viewModel = new MainViewModel(store, () => clock.Now);
        var first = viewModel.Items.Single(item => item.Id == firstId);

        viewModel.CompleteItemCommand.Execute(first);
        AssertEx.SequenceEqual(
            [secondId, firstId],
            viewModel.Items.Select(item => item.Id));
        AssertEx.SequenceEqual(
            [false, true],
            viewModel.Items.Select(item => item.IsCompleted));

        clock.Now = clock.Now.AddDays(1);

        AssertEx.True(
            viewModel.RollOverToCurrentDay(),
            "Advancing the local date should roll the active checklist forward.");
        AssertEx.SequenceEqual(
            [secondId, firstId],
            viewModel.Items.Select(item => item.Id));
        AssertEx.True(
            viewModel.Items.All(item => !item.IsCompleted),
            "Recurring quests should be unchecked for the new day.");
        AssertEx.Equal(secondId, AssertEx.NotNull(viewModel.NextPendingItem).Id);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal("2026-09-17", saved.CurrentDate);
        AssertEx.SequenceEqual([secondId, firstId], saved.Items.Select(item => item.Id));
        AssertEx.True(
            saved.Items.All(item => !item.IsCompleted),
            "The reset completion state should be persisted.");

        var previousDay = HistoryFor(saved, "2026-09-16");
        AssertEx.SequenceEqual([secondId, firstId], previousDay.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([false, true], previousDay.Items.Select(item => item.IsCompleted));

        var currentDay = HistoryFor(saved, "2026-09-17");
        AssertEx.SequenceEqual([secondId, firstId], currentDay.Items.Select(item => item.Id));
        AssertEx.True(
            currentDay.Items.All(item => !item.IsCompleted),
            "The new day's history should begin with every recurring quest unchecked.");

        var reloadStore = new InMemoryStateStore(saved);
        var reloaded = new MainViewModel(reloadStore, () => clock.Now);

        AssertEx.SequenceEqual([secondId, firstId], reloaded.Items.Select(item => item.Id));
        AssertEx.True(reloaded.Items.All(item => !item.IsCompleted));
        AssertEx.Equal(0, reloadStore.SaveCount);
    }

    private static void PinTogglePersistsAndUpdatesPresentation()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Settings = new AppSettings
            {
                AlwaysOnTop = true,
                LanguageCode = "en-US"
            }
        });
        var viewModel = new MainViewModel(store, () => now);
        var changedProperties = new HashSet<string?>();
        viewModel.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName);

        viewModel.TogglePinCommand.Execute(null);

        AssertEx.False(viewModel.AlwaysOnTop);
        AssertEx.Equal("Always on top", viewModel.PinTooltip);
        AssertEx.False(AssertEx.NotNull(store.Snapshot).Settings.AlwaysOnTop);
        AssertEx.Equal(1, store.SaveCount);
        AssertEx.True(changedProperties.Contains(nameof(MainViewModel.AlwaysOnTop)));
        AssertEx.True(changedProperties.Contains(nameof(MainViewModel.PinTooltip)));
    }

    private static void QuestReorderPersistsToActiveStateAndCurrentHistory()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var thirdId = Guid.NewGuid();
        var first = CreateItemState(firstId, "Pertama", false, 0, now.AddMinutes(-3));
        var second = CreateItemState(secondId, "Kedua", false, 1, now.AddMinutes(-2));
        var third = CreateItemState(thirdId, "Ketiga", false, 2, now.AddMinutes(-1));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Items = [first, second, third],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-16",
                    Items = [first, second, third]
                }
            ]
        });
        var viewModel = new MainViewModel(store, () => now);

        AssertEx.True(viewModel.MoveItem(viewModel.Items[2], 0));

        AssertEx.SequenceEqual(
            [thirdId, firstId, secondId],
            viewModel.Items.Select(item => item.Id));
        AssertEx.Equal(1, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.SequenceEqual(
            [thirdId, firstId, secondId],
            saved.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([0, 1, 2], saved.Items.Select(item => item.SortOrder));

        var currentHistory = HistoryFor(saved, "2026-09-16");
        AssertEx.SequenceEqual(
            [thirdId, firstId, secondId],
            currentHistory.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([0, 1, 2], currentHistory.Items.Select(item => item.SortOrder));
        AssertEx.SequenceEqual(
            ["Ketiga", "Pertama", "Kedua"],
            viewModel.HistoryEntries.Single().Items.Select(item => item.Text));

        var reloadStore = new InMemoryStateStore(saved);
        var reloaded = new MainViewModel(reloadStore, () => now);

        AssertEx.SequenceEqual(
            [thirdId, firstId, secondId],
            reloaded.Items.Select(item => item.Id));
        AssertEx.SequenceEqual(
            ["Ketiga", "Pertama", "Kedua"],
            reloaded.HistoryEntries.Single().Items.Select(item => item.Text));
        AssertEx.Equal(0, reloadStore.SaveCount);
    }

    private static void QuestReorderRetainsCompletedHistoryOnlyItems()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var completedOrphanId = Guid.NewGuid();
        var pendingOrphanId = Guid.NewGuid();
        var first = CreateItemState(firstId, "Aktif pertama", false, 0, now.AddMinutes(-4));
        var completedOrphan = CreateItemState(
            completedOrphanId,
            "Selesai lalu dihapus",
            true,
            1,
            now.AddMinutes(-3));
        var pendingOrphan = CreateItemState(
            pendingOrphanId,
            "Belum selesai lalu dihapus",
            false,
            2,
            now.AddMinutes(-2));
        var second = CreateItemState(secondId, "Aktif kedua", false, 3, now.AddMinutes(-1));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Items = [first, second],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-16",
                    Items = [first, completedOrphan, pendingOrphan, second]
                }
            ]
        });
        var viewModel = new MainViewModel(store, () => now);

        AssertEx.True(viewModel.MoveItem(viewModel.Items[1], 0));

        var saved = AssertEx.NotNull(store.Snapshot);
        var historyItems = HistoryFor(saved, "2026-09-16").Items;
        AssertEx.SequenceEqual(
            [secondId, completedOrphanId, firstId],
            historyItems.Select(item => item.Id));
        AssertEx.SequenceEqual([0, 1, 2], historyItems.Select(item => item.SortOrder));
        AssertEx.True(
            historyItems.Single(item => item.Id == completedOrphanId).IsCompleted,
            "A completed quest removed from the active list must remain in history.");
        AssertEx.False(
            historyItems.Any(item => item.Id == pendingOrphanId),
            "An unfinished quest removed from the active list should not remain in history.");
        AssertEx.SequenceEqual(
            [secondId, firstId],
            historyItems
                .Where(item => item.Id == firstId || item.Id == secondId)
                .Select(item => item.Id));
    }

    private static void QuestReorderRejectsForeignAndUnchangedMoves()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(firstId, "Pertama", false, 0, now.AddMinutes(-2)),
                CreateItemState(secondId, "Kedua", false, 1, now.AddMinutes(-1))
            ]
        });
        var viewModel = new MainViewModel(store, () => now);
        var foreignItem = new ChecklistItem(
            Guid.NewGuid(),
            "Bukan milik koleksi",
            false,
            now);

        AssertEx.False(viewModel.MoveItem(foreignItem, 0), "A foreign item should be rejected.");
        AssertEx.False(viewModel.MoveItem(viewModel.Items[0], 0), "Moving to the same index should be a no-op.");
        AssertEx.False(
            viewModel.MoveItem(viewModel.Items[1], int.MaxValue),
            "A clamped destination equal to the source should be a no-op.");
        AssertEx.SequenceEqual(
            [firstId, secondId],
            viewModel.Items.Select(item => item.Id));
        AssertEx.Equal(0, store.SaveCount);
    }

    private static void PersistedSortOrderIsNormalizedStably()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var thirdId = Guid.NewGuid();
        var historyFirstId = Guid.NewGuid();
        var historySecondId = Guid.NewGuid();
        var historyThirdId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(firstId, "Urutan sepuluh A", false, 10, now.AddMinutes(-3)),
                CreateItemState(secondId, "Urutan minus satu", false, -1, now.AddMinutes(-2)),
                CreateItemState(thirdId, "Urutan sepuluh B", false, 10, now.AddMinutes(-1))
            ],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-15",
                    Items =
                    [
                        CreateItemState(historyFirstId, "Riwayat lima A", true, 5, now.AddDays(-1)),
                        CreateItemState(historySecondId, "Riwayat nol", false, 0, now.AddDays(-1)),
                        CreateItemState(historyThirdId, "Riwayat lima B", true, 5, now.AddDays(-1))
                    ]
                }
            ]
        });
        var viewModel = new MainViewModel(store, () => now);

        AssertEx.SequenceEqual(
            [secondId, firstId, thirdId],
            viewModel.Items.Select(item => item.Id));
        AssertEx.SequenceEqual(
            ["Riwayat nol", "Riwayat lima A", "Riwayat lima B"],
            viewModel.HistoryEntries.Single().Items.Select(item => item.Text));

        viewModel.Save();

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.SequenceEqual(
            [secondId, firstId, thirdId],
            saved.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([0, 1, 2], saved.Items.Select(item => item.SortOrder));
        var savedHistory = HistoryFor(saved, "2026-09-15").Items;
        AssertEx.SequenceEqual(
            [historySecondId, historyFirstId, historyThirdId],
            savedHistory.Select(item => item.Id));
        AssertEx.SequenceEqual([0, 1, 2], savedHistory.Select(item => item.SortOrder));
    }

    private static void LabelDefaultsMigrateOnceAndEmptySetStaysEmpty()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var legacyStore = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 5,
            CurrentDate = "2026-09-16",
            Labels = [],
            Settings = new AppSettings { QuestSortMode = "not-a-mode" }
        });

        var migrated = new MainViewModel(legacyStore, () => now);

        AssertEx.SequenceEqual(
            ["Important", "Personal", "Routine"],
            migrated.Labels.Select(label => label.Name));
        AssertEx.Equal("manual", migrated.SortModeCode);
        AssertEx.Equal(1, legacyStore.SaveCount);
        AssertEx.Equal(7, AssertEx.NotNull(legacyStore.Snapshot).SchemaVersion);

        var emptyStore = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Labels = []
        });

        var intentionallyEmpty = new MainViewModel(emptyStore, () => now);

        AssertEx.Equal(0, intentionallyEmpty.Labels.Count);
        AssertEx.Equal(0, emptyStore.SaveCount);
    }

    private static void LabelCrudValidatesAndClearsReferences()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var labelId = Guid.NewGuid();
        var activeId = Guid.NewGuid();
        var historicalId = Guid.NewGuid();
        var scheduledId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Labels =
            [
                new QuestLabelState
                {
                    Id = labelId,
                    Name = "Work",
                    ColorHex = "#336699",
                    SortOrder = 0
                }
            ],
            Items =
            [
                CreateItemState(activeId, "Current", false, 0, now, labelId: labelId)
            ],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-15",
                    Items =
                    [
                        CreateItemState(
                            historicalId,
                            "Past",
                            true,
                            0,
                            now.AddDays(-1),
                            labelId: labelId)
                    ]
                }
            ],
            ScheduledQuests =
            [
                CreateScheduledQuestState(
                    scheduledId,
                    "Tomorrow",
                    "2026-09-17",
                    0,
                    now,
                    labelId: labelId)
            ]
        });
        var viewModel = new MainViewModel(store, () => now);

        AssertEx.False(viewModel.TryAddLabel("", "#123456"));
        AssertEx.False(viewModel.TryAddLabel("Bad color", "red"));
        AssertEx.False(viewModel.TryAddLabel(new string('x', 25), "#123456"));
        AssertEx.False(viewModel.TryAddLabel("work", "#123456"));
        AssertEx.True(viewModel.TryUpdateLabel(labelId, "Deep Work", "#abcdef"));
        AssertEx.Equal("Deep Work", viewModel.Items.Single().LabelName);
        AssertEx.Equal("#ABCDEF", viewModel.Items.Single().LabelColorHex);
        AssertEx.Equal("Deep Work", viewModel.UpcomingQuests.Single().LabelName);
        AssertEx.True(
            viewModel.HistoryEntries.SelectMany(entry => entry.Items).All(item => item.LabelName == "Deep Work"),
            "Historical label presentation should follow the edited label definition.");

        for (var index = 1; index < 12; index++)
        {
            AssertEx.True(viewModel.TryAddLabel($"Label {index}", $"#{index:X6}"));
        }

        AssertEx.Equal(12, viewModel.Labels.Count);
        AssertEx.False(viewModel.CanAddMoreLabels);
        AssertEx.False(viewModel.TryAddLabel("Overflow", "#112233"));
        AssertEx.True(viewModel.DeleteLabel(labelId));

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.False(viewModel.Items.Single().HasLabel);
        AssertEx.False(viewModel.UpcomingQuests.Single().HasLabel);
        AssertEx.True(saved.Items.All(item => !item.LabelId.HasValue));
        AssertEx.True(saved.ScheduledQuests.All(item => !item.LabelId.HasValue));
        AssertEx.True(
            saved.History.SelectMany(entry => entry.Items).All(item => !item.LabelId.HasValue),
            "Deleting a label must clear history references without deleting history quests.");
        AssertEx.Equal(2, saved.History.SelectMany(entry => entry.Items).Count());
        AssertEx.True(viewModel.CanAddMoreLabels);
    }

    private static void LabelSortingAssignmentAndOrderPreserveManualOrder()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var highId = Guid.NewGuid();
        var lowId = Guid.NewGuid();
        var aId = Guid.NewGuid();
        var bId = Guid.NewGuid();
        var cId = Guid.NewGuid();
        var doneId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Labels =
            [
                new QuestLabelState { Id = highId, Name = "High", ColorHex = "#CC4455", SortOrder = 0 },
                new QuestLabelState { Id = lowId, Name = "Low", ColorHex = "#557799", SortOrder = 1 }
            ],
            Items =
            [
                CreateItemState(aId, "A", false, 0, now.AddMinutes(-4)),
                CreateItemState(bId, "B", false, 1, now.AddMinutes(-3), labelId: lowId),
                CreateItemState(cId, "C", false, 2, now.AddMinutes(-2), labelId: highId),
                CreateItemState(doneId, "Done", true, 3, now.AddMinutes(-1), labelId: highId)
            ]
        });
        var viewModel = new MainViewModel(store, () => now);

        viewModel.SetSortModeCommand.Execute(QuestSortModeCodes.Label);
        AssertEx.SequenceEqual([cId, bId, aId, doneId], viewModel.Items.Select(item => item.Id));
        AssertEx.False(viewModel.MoveItem(viewModel.Items[2], 0), "Drag reorder should be disabled outside manual mode.");

        AssertEx.True(viewModel.AssignItemLabel(viewModel.Items.Single(item => item.Id == aId), highId));
        AssertEx.SequenceEqual([aId, cId, bId, doneId], viewModel.Items.Select(item => item.Id));
        var highLabelViewModel = viewModel.Labels.Single(label => label.Id == highId);
        var lowLabelViewModel = viewModel.Labels.Single(label => label.Id == lowId);
        AssertEx.True(viewModel.MoveLabel(lowId, 0));
        AssertEx.True(
            ReferenceEquals(lowLabelViewModel, viewModel.Labels[0]) &&
            ReferenceEquals(highLabelViewModel, viewModel.Labels[1]),
            "Label reorder should move the existing editor view models instead of recreating them.");
        AssertEx.SequenceEqual([lowId, highId], viewModel.Labels.Select(label => label.Id));
        AssertEx.SequenceEqual([0, 1], viewModel.Labels.Select(label => label.SortOrder));
        AssertEx.False(viewModel.MoveLabel(lowId, 0), "Dropping a label in its current position should be a no-op.");
        AssertEx.False(viewModel.MoveLabel(Guid.NewGuid(), 1), "A foreign label cannot be reordered.");
        AssertEx.SequenceEqual([bId, aId, cId, doneId], viewModel.Items.Select(item => item.Id));

        viewModel.SetSortModeCommand.Execute(QuestSortMode.Manual);
        AssertEx.SequenceEqual([aId, bId, cId, doneId], viewModel.Items.Select(item => item.Id));

        viewModel.SetSortModeCommand.Execute(QuestSortMode.Label);
        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(QuestSortModeCodes.Label, saved.Settings.QuestSortMode);
        AssertEx.Equal(
            0,
            saved.Items.Single(item => item.Id == aId).ManualSortOrder.GetValueOrDefault());
        AssertEx.Equal(
            1,
            saved.Items.Single(item => item.Id == bId).ManualSortOrder.GetValueOrDefault());
        AssertEx.Equal(
            2,
            saved.Items.Single(item => item.Id == cId).ManualSortOrder.GetValueOrDefault());
        AssertEx.Equal(
            3,
            saved.Items.Single(item => item.Id == doneId).ManualSortOrder.GetValueOrDefault());

        var reloaded = new MainViewModel(new InMemoryStateStore(saved), () => now);
        AssertEx.Equal(QuestSortMode.Label, reloaded.SortMode);
        AssertEx.SequenceEqual([lowId, highId], reloaded.Labels.Select(label => label.Id));
        AssertEx.SequenceEqual([0, 1], reloaded.Labels.Select(label => label.SortOrder));
        AssertEx.SequenceEqual([bId, aId, cId, doneId], reloaded.Items.Select(item => item.Id));
        reloaded.SetSortModeCommand.Execute(QuestSortMode.Manual);
        AssertEx.SequenceEqual([aId, bId, cId, doneId], reloaded.Items.Select(item => item.Id));
    }

    private static void DurationSortingKeepsTimerSound()
    {
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7)));
        var untimedId = Guid.NewGuid();
        var longId = Guid.NewGuid();
        var shortId = Guid.NewGuid();
        var doneId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(untimedId, "Untimed", false, 0, clock.Now),
                CreateItemState(longId, "Long", false, 1, clock.Now, plannedDurationMinutes: 30),
                CreateItemState(shortId, "Short", false, 2, clock.Now, plannedDurationMinutes: 5),
                CreateItemState(doneId, "Done", true, 3, clock.Now, plannedDurationMinutes: 1)
            ]
        });
        var viewModel = new MainViewModel(store, () => clock.Now);
        var longQuest = viewModel.Items.Single(item => item.Id == longId);

        viewModel.ToggleTimerCommand.Execute(longQuest);
        viewModel.SetSortModeCommand.Execute(QuestSortMode.DurationAscending);
        AssertEx.SequenceEqual([shortId, longId, untimedId, doneId], viewModel.Items.Select(item => item.Id));
        AssertEx.True(longQuest.IsTimerRunning, "Sorting must not pause or replace the running timer item.");

        clock.Now = clock.Now.AddSeconds(30);
        AssertEx.True(viewModel.TickTimers());
        AssertEx.Equal(1770, longQuest.RemainingSeconds);
        viewModel.SetSortModeCommand.Execute(QuestSortMode.DurationDescending);
        AssertEx.SequenceEqual([longId, shortId, untimedId, doneId], viewModel.Items.Select(item => item.Id));
        AssertEx.True(longQuest.IsTimerRunning);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(QuestSortModeCodes.DurationDescending, saved.Settings.QuestSortMode);
        var reloaded = new MainViewModel(new InMemoryStateStore(saved), () => clock.Now);
        AssertEx.SequenceEqual([longId, shortId, untimedId, doneId], reloaded.Items.Select(item => item.Id));
        AssertEx.True(reloaded.Items.Single(item => item.Id == longId).IsTimerRunning);
    }

    private static void ScheduledLabelsSurviveActivationAndHistory()
    {
        var now = new DateTimeOffset(2026, 9, 17, 7, 0, 0, TimeSpan.FromHours(7));
        var labelId = Guid.NewGuid();
        var dueId = Guid.NewGuid();
        var futureId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-17",
            Labels =
            [
                new QuestLabelState { Id = labelId, Name = "Focus", ColorHex = "#8866CC", SortOrder = 0 }
            ],
            ScheduledQuests =
            [
                CreateScheduledQuestState(dueId, "Due", "2026-09-17", 0, now, 25, labelId),
                CreateScheduledQuestState(futureId, "Future", "2026-09-18", 1, now, 10, labelId)
            ]
        });

        var viewModel = new MainViewModel(store, () => now);

        var active = viewModel.Items.Single();
        AssertEx.Equal(dueId, active.Id);
        AssertEx.Equal(labelId, active.LabelId);
        AssertEx.Equal("Focus", active.LabelName);
        AssertEx.Equal(25, active.PlannedDurationMinutes);
        AssertEx.Equal(labelId, viewModel.UpcomingQuests.Single().LabelId);
        AssertEx.Equal("Focus", viewModel.UpcomingQuests.Single().LabelName);
        AssertEx.Equal(
            labelId,
            viewModel.HistoryEntries.Single().Items.Single().LabelId);
    }

    private static void MalformedPersistedLabelsNormalizeSafely()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var sharedId = Guid.NewGuid();
        var focusId = Guid.NewGuid();
        var missingId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Labels =
            [
                new QuestLabelState { Id = sharedId, Name = "  Work  ", ColorHex = "#abcdef", SortOrder = 0 },
                new QuestLabelState { Id = sharedId, Name = "Other", ColorHex = "#112233", SortOrder = 1 },
                new QuestLabelState { Id = Guid.NewGuid(), Name = "work", ColorHex = "#445566", SortOrder = 2 },
                new QuestLabelState { Id = focusId, Name = "Focus", ColorHex = "invalid", SortOrder = 3 },
                new QuestLabelState { Id = Guid.NewGuid(), Name = "   ", ColorHex = "#778899", SortOrder = 4 }
            ],
            Items =
            [
                CreateItemState(Guid.NewGuid(), "Known", false, 0, now, labelId: focusId),
                CreateItemState(Guid.NewGuid(), "Missing", false, 1, now, labelId: missingId)
            ]
        });

        var viewModel = new MainViewModel(store, () => now);

        AssertEx.SequenceEqual(["Work", "Other", "Focus"], viewModel.Labels.Select(label => label.Name));
        AssertEx.True(viewModel.Labels.Select(label => label.Id).Distinct().Count() == 3);
        AssertEx.Equal("#ABCDEF", viewModel.Labels.Single(label => label.Name == "Work").ColorHex);
        AssertEx.Equal("#5B8A72", viewModel.Labels.Single(label => label.Name == "Focus").ColorHex);
        AssertEx.Equal("Focus", viewModel.Items.Single(item => item.Text == "Known").LabelName);
        AssertEx.False(viewModel.Items.Single(item => item.Text == "Missing").HasLabel);
        AssertEx.Equal(1, store.SaveCount);
    }

    private static void ClearHistoryPreservesActiveQuests()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var completedId = Guid.NewGuid();
        var pendingId = Guid.NewGuid();
        var scheduledId = Guid.NewGuid();
        var oldId = Guid.NewGuid();
        var completed = CreateItemState(completedId, "Quest aktif selesai", true, 0, now.AddMinutes(-2));
        var pending = CreateItemState(pendingId, "Quest aktif pending", false, 1, now.AddMinutes(-1));
        var storage = new FakeStorageUsageService(
            _ => new StorageUsageSnapshot(0, 0, 0));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            Items = [completed, pending],
            ScheduledQuests =
            [
                CreateScheduledQuestState(
                    scheduledId,
                    "Quest untuk besok",
                    "2026-09-17",
                    0,
                    now)
            ],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-16",
                    Items = [completed, pending]
                },
                new DailyHistoryState
                {
                    Date = "2026-09-15",
                    Items =
                    [
                        CreateItemState(oldId, "Quest kemarin", true, 0, now.AddDays(-1))
                    ]
                }
            ]
        });
        var viewModel = new MainViewModel(store, () => now, storage);

        AssertEx.True(viewModel.ClearHistoryCommand.CanExecute(null));
        viewModel.ClearHistoryCommand.Execute(null);

        AssertEx.SequenceEqual(
            [pendingId, completedId],
            viewModel.Items.Select(item => item.Id));
        AssertEx.SequenceEqual(
            [false, true],
            viewModel.Items.Select(item => item.IsCompleted));
        AssertEx.Equal(0, viewModel.HistoryEntries.Count);
        AssertEx.Equal(1, viewModel.UpcomingQuests.Count);
        AssertEx.False(viewModel.ClearHistoryCommand.CanExecute(null));
        AssertEx.Equal(2, store.SaveCount);
        var cleared = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(0, cleared.History.Count);
        AssertEx.SequenceEqual([scheduledId], cleared.ScheduledQuests.Select(item => item.Id));
        AssertEx.Equal(1, storage.MeasureCount);
        AssertEx.Equal(0, storage.LastHistoryCount);

        viewModel.Items.Single(item => item.Id == pendingId).IsCompleted = true;

        var recreatedHistory = HistoryFor(AssertEx.NotNull(store.Snapshot), "2026-09-16");
        AssertEx.SequenceEqual(
            [completedId, pendingId],
            recreatedHistory.Items.Select(item => item.Id));
        AssertEx.True(
            recreatedHistory.Items.All(item => item.IsCompleted),
            "A later checklist mutation should rebuild today's history from the preserved active quests.");
        AssertEx.SequenceEqual(
            [scheduledId],
            AssertEx.NotNull(store.Snapshot).ScheduledQuests.Select(item => item.Id));
    }

    private static void SettingsRefreshesInjectedStorageUsage()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var historyId = Guid.NewGuid();
        var storage = new FakeStorageUsageService(
            _ => new StorageUsageSnapshot(
                ApplicationBytes: 512,
                DataBytes: 2 * 1024,
                HistoryBytes: 3 * 1024 * 1024));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16",
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-15",
                    Items =
                    [
                        CreateItemState(historyId, "Riwayat", true, 0, now.AddDays(-1))
                    ]
                }
            ]
        });
        var viewModel = new MainViewModel(store, () => now, storage);

        AssertEx.Equal("0 B", viewModel.ApplicationStorageText);
        AssertEx.Equal("0 B", viewModel.DataStorageText);
        AssertEx.Equal("0 B", viewModel.HistoryStorageText);

        viewModel.ShowSettingsCommand.Execute(null);

        AssertEx.Equal(1, storage.MeasureCount);
        AssertEx.Equal(1, storage.LastHistoryCount);
        AssertEx.Equal("512 B", viewModel.ApplicationStorageText);
        AssertEx.Equal("2 KB", viewModel.DataStorageText);
        AssertEx.Equal("3 MB", viewModel.HistoryStorageText);
        AssertEx.True(viewModel.IsSettingsView);
        AssertEx.Equal(0, store.SaveCount);
    }

    private static void StorageUsageFailuresDegradeGracefully()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var storage = new FakeStorageUsageService(
            _ => throw new IOException("Storage is temporarily unavailable."));
        var store = new InMemoryStateStore(new AppState
        {
            CurrentDate = "2026-09-16"
        });
        var viewModel = new MainViewModel(store, () => now, storage);

        viewModel.ShowSettingsCommand.Execute(null);

        AssertEx.Equal(1, storage.MeasureCount);
        AssertEx.Equal("\u2014", viewModel.ApplicationStorageText);
        AssertEx.Equal("\u2014", viewModel.DataStorageText);
        AssertEx.Equal("\u2014", viewModel.HistoryStorageText);
        AssertEx.True(viewModel.IsSettingsView, "Settings should still open if usage measurement fails.");
        AssertEx.Equal(0, store.SaveCount);
    }

    private static void DailyRolloverArchivesOnceWithoutDuplicates()
    {
        var clock = new MutableClock(
            new DateTimeOffset(2026, 9, 16, 6, 45, 0, TimeSpan.FromHours(7)));
        var completedId = Guid.NewGuid();
        var pendingId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-15",
            Items =
            [
                CreateItemState(completedId, "Olahraga", true, 0, clock.Now.AddDays(-1)),
                CreateItemState(pendingId, "Baca agenda", false, 1, clock.Now.AddDays(-1))
            ]
        });

        var viewModel = new MainViewModel(store, () => clock.Now);

        AssertEx.Equal(1, store.SaveCount);
        AssertEx.Equal(0, viewModel.CompletedCount);
        AssertEx.True(viewModel.Items.All(item => !item.IsCompleted), "New-day active items should be unchecked.");

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal("2026-09-16", saved.CurrentDate);
        AssertEx.Equal(2, saved.History.Count);
        AssertEx.Equal(2, saved.History.Select(entry => entry.Date).Distinct().Count());

        var previousDay = HistoryFor(saved, "2026-09-15");
        AssertEx.SequenceEqual([pendingId, completedId], previousDay.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([false, true], previousDay.Items.Select(item => item.IsCompleted));

        var currentDay = HistoryFor(saved, "2026-09-16");
        AssertEx.SequenceEqual([completedId, pendingId], currentDay.Items.Select(item => item.Id));
        AssertEx.True(currentDay.Items.All(item => !item.IsCompleted), "Today's history should start unchecked.");

        var savesBeforeSameDayCheck = store.SaveCount;
        AssertEx.False(viewModel.RollOverToCurrentDay(), "A repeated same-day check should be a no-op.");
        AssertEx.Equal(savesBeforeSameDayCheck, store.SaveCount);
        saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(2, saved.History.Count);
        AssertEx.Equal(1, saved.History.Count(entry => entry.Date == "2026-09-15"));
        AssertEx.Equal(1, saved.History.Count(entry => entry.Date == "2026-09-16"));
        AssertEx.True(
            HistoryFor(saved, "2026-09-15").Items.Single(item => item.Id == completedId).IsCompleted,
            "Resetting active items must not mutate the archived snapshot.");
    }

    private static void DailyRolloverActivatesDueQuestsAfterArchiving()
    {
        var now = new DateTimeOffset(2026, 9, 17, 6, 45, 0, TimeSpan.FromHours(7));
        var activeId = Guid.NewGuid();
        var dueId = Guid.NewGuid();
        var laterId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(activeId, "Quest lama", true, 0, now.AddDays(-1))
            ],
            ScheduledQuests =
            [
                CreateScheduledQuestState(
                    dueId,
                    "Quest jatuh tempo",
                    "2026-09-17",
                    0,
                    now.AddDays(-1).AddMinutes(1)),
                CreateScheduledQuestState(
                    laterId,
                    "Quest untuk nanti",
                    "2026-09-24",
                    1,
                    now.AddDays(-1).AddMinutes(2))
            ]
        });

        var viewModel = new MainViewModel(store, () => now);

        AssertEx.SequenceEqual(
            [activeId, dueId],
            viewModel.Items.Select(item => item.Id));
        AssertEx.True(
            viewModel.Items.All(item => !item.IsCompleted),
            "Both recurring and newly activated quests should start the day unchecked.");
        AssertEx.Equal(1, viewModel.UpcomingQuests.Count);
        AssertEx.Equal(1, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.SequenceEqual(
            [activeId],
            HistoryFor(saved, "2026-09-16").Items.Select(item => item.Id));
        AssertEx.True(
            HistoryFor(saved, "2026-09-16").Items.Single().IsCompleted,
            "The previous day must be archived before the due quest is activated.");
        AssertEx.SequenceEqual(
            [activeId, dueId],
            HistoryFor(saved, "2026-09-17").Items.Select(item => item.Id));
        AssertEx.True(
            HistoryFor(saved, "2026-09-17").Items.All(item => !item.IsCompleted),
            "Today's history should contain the newly activated quest unchecked.");
        AssertEx.SequenceEqual([laterId], saved.ScheduledQuests.Select(item => item.Id));
    }

    private static void OverdueQuestsActivateOnceOnNextLaunch()
    {
        var now = new DateTimeOffset(2026, 9, 20, 9, 0, 0, TimeSpan.FromHours(7));
        var activeId = Guid.NewGuid();
        var overdueId = Guid.NewGuid();
        var futureId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 7,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(activeId, "Quest berulang", false, 0, now.AddDays(-4))
            ],
            ScheduledQuests =
            [
                CreateScheduledQuestState(
                    overdueId,
                    "Quest yang terlewat",
                    "2026-09-18",
                    0,
                    now.AddDays(-4).AddMinutes(1)),
                CreateScheduledQuestState(
                    futureId,
                    "Quest masa depan",
                    "2026-09-24",
                    1,
                    now.AddDays(-4).AddMinutes(2))
            ]
        });

        var firstLaunch = new MainViewModel(store, () => now);

        AssertEx.SequenceEqual(
            [activeId, overdueId],
            firstLaunch.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([futureId], AssertEx.NotNull(store.Snapshot).ScheduledQuests.Select(item => item.Id));
        AssertEx.SequenceEqual(
            ["2026-09-20", "2026-09-16"],
            AssertEx.NotNull(store.Snapshot).History.Select(entry => entry.Date));
        AssertEx.Equal(1, store.SaveCount);

        var reloadStore = new InMemoryStateStore(AssertEx.NotNull(store.Snapshot));
        var secondLaunch = new MainViewModel(reloadStore, () => now);

        AssertEx.Equal(0, reloadStore.SaveCount);
        AssertEx.Equal(1, secondLaunch.Items.Count(item => item.Id == overdueId));
        AssertEx.SequenceEqual(
            [activeId, overdueId],
            secondLaunch.Items.Select(item => item.Id));
        AssertEx.SequenceEqual(
            [futureId],
            AssertEx.NotNull(reloadStore.Snapshot).ScheduledQuests.Select(item => item.Id));
    }

    private static void LegacyV3StateMigratesToLightTheme()
    {
        WithTemporaryDirectory(directory =>
        {
            var now = new DateTimeOffset(2026, 9, 16, 9, 0, 0, TimeSpan.FromHours(7));
            var statePath = Path.Combine(directory, "state.json");
            var activeId = Guid.NewGuid();
            var scheduledId = Guid.NewGuid();
            var historicalId = Guid.NewGuid();
            var legacyJson = $$"""
                {
                  "SchemaVersion": 3,
                  "CurrentDate": "2026-09-16",
                  "Items": [
                    {
                      "Id": "{{activeId}}",
                      "Text": "Quest aktif v3",
                      "IsCompleted": false,
                      "SortOrder": 0,
                      "CreatedAt": "2026-09-16T08:55:00+07:00"
                    }
                  ],
                  "History": [
                    {
                      "Date": "2026-09-15",
                      "Items": [
                        {
                          "Id": "{{historicalId}}",
                          "Text": "Riwayat v3",
                          "IsCompleted": true,
                          "SortOrder": 0,
                          "CreatedAt": "2026-09-15T08:55:00+07:00"
                        }
                      ]
                    }
                  ],
                  "ScheduledQuests": [
                    {
                      "Id": "{{scheduledId}}",
                      "Text": "Quest mendatang v3",
                      "ScheduledDate": "2026-09-18",
                      "SortOrder": 0,
                      "CreatedAt": "2026-09-16T08:56:00+07:00"
                    }
                  ],
                  "Window": { "Left": 120, "Top": 80, "Width": 430, "Height": 650 },
                  "Settings": { "AlwaysOnTop": false, "LanguageCode": "en-US" }
                }
                """;
            WriteUtf8File(statePath, legacyJson);
            var durableStore = new JsonStateStore(statePath);
            var store = new RecordingStateStore(durableStore);
            var theme = new RecordingThemeService();

            var viewModel = new MainViewModel(store, () => now, null, theme);

            AssertEx.Equal("light", viewModel.ThemeCode);
            AssertEx.True(viewModel.IsLightTheme);
            AssertEx.SequenceEqual(["light"], theme.AppliedThemes);
            AssertEx.SequenceEqual([activeId], viewModel.Items.Select(item => item.Id));
            AssertEx.SequenceEqual([scheduledId], viewModel.UpcomingQuests.Select(item => item.Id));
            AssertEx.Equal(1, store.SaveCount);

            var migrated = AssertEx.NotNull(durableStore.Load());
            AssertEx.Equal(7, migrated.SchemaVersion);
            AssertEx.Equal("light", migrated.Settings.ThemeCode);
            AssertEx.Equal("en-US", migrated.Settings.LanguageCode);
            AssertEx.False(migrated.Settings.AlwaysOnTop, "Migration should preserve the pin preference.");
            AssertEx.Equal(120d, migrated.Window.Left);
            AssertEx.Equal(80d, migrated.Window.Top);
            AssertEx.Equal(430d, migrated.Window.Width);
            AssertEx.Equal(650d, migrated.Window.Height);
            AssertEx.SequenceEqual([activeId], migrated.Items.Select(item => item.Id));
            AssertEx.SequenceEqual([scheduledId], migrated.ScheduledQuests.Select(item => item.Id));
            AssertEx.SequenceEqual(
                [historicalId],
                HistoryFor(migrated, "2026-09-15").Items.Select(item => item.Id));
            AssertEx.True(
                File.ReadAllText(statePath).Contains("\"ThemeCode\": \"light\"", StringComparison.Ordinal),
                "The migrated state should persist the normalized theme explicitly.");

            var reloadStore = new RecordingStateStore(durableStore);
            var reloadTheme = new RecordingThemeService();
            var reloaded = new MainViewModel(reloadStore, () => now, null, reloadTheme);

            AssertEx.Equal("light", reloaded.ThemeCode);
            AssertEx.SequenceEqual(["light"], reloadTheme.AppliedThemes);
            AssertEx.Equal(0, reloadStore.SaveCount);
        });
    }

    private static void LegacyV2StateMigratesWithEmptyFutureQueue()
    {
        var now = new DateTimeOffset(2026, 9, 16, 9, 0, 0, TimeSpan.FromHours(7));
        var activeId = Guid.NewGuid();
        var historicalId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 2,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(activeId, "Quest dari v2", false, 0, now.AddMinutes(-1))
            ],
            History =
            [
                new DailyHistoryState
                {
                    Date = "2026-09-15",
                    Items =
                    [
                        CreateItemState(historicalId, "Riwayat v2", true, 0, now.AddDays(-1))
                    ]
                }
            ]
        });

        var viewModel = new MainViewModel(store, () => now);

        AssertEx.SequenceEqual([activeId], viewModel.Items.Select(item => item.Id));
        AssertEx.Equal(0, viewModel.UpcomingQuests.Count);
        AssertEx.Equal(1, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(7, saved.SchemaVersion);
        AssertEx.Equal("en-US", saved.Settings.LanguageCode);
        AssertEx.Equal("light", saved.Settings.ThemeCode);
        AssertEx.Equal(0, saved.ScheduledQuests.Count);
        AssertEx.SequenceEqual([activeId], saved.Items.Select(item => item.Id));
        AssertEx.SequenceEqual(
            [historicalId],
            HistoryFor(saved, "2026-09-15").Items.Select(item => item.Id));
        AssertEx.Equal(1, saved.History.Count);

        var reloadStore = new InMemoryStateStore(saved);
        _ = new MainViewModel(reloadStore, () => now);
        AssertEx.Equal(0, reloadStore.SaveCount);
    }

    private static void LegacyV1StateMigratesWithoutHistoryLoss()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 0, 0, TimeSpan.FromHours(7));
        var itemId = Guid.NewGuid();
        var createdAt = now.AddDays(-1).AddMinutes(-10);
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 1,
            CurrentDate = "2026-09-15",
            Items =
            [
                CreateItemState(itemId, "Checklist lama", true, 0, createdAt)
            ],
            Window = new WidgetWindowState
            {
                Left = 120,
                Top = 80,
                Width = 430,
                Height = 650
            },
            Settings = new AppSettings
            {
                AlwaysOnTop = false,
                LanguageCode = "id-ID"
            }
        });

        var viewModel = new MainViewModel(store, () => now);

        AssertEx.Equal(1, store.SaveCount);
        AssertEx.Equal(0, viewModel.CompletedCount);
        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(7, saved.SchemaVersion);
        AssertEx.Equal("2026-09-16", saved.CurrentDate);
        AssertEx.Equal("id-ID", saved.Settings.LanguageCode);
        AssertEx.Equal("light", saved.Settings.ThemeCode);
        AssertEx.False(saved.Settings.AlwaysOnTop, "Pin preference should survive migration.");
        AssertEx.Equal(120d, saved.Window.Left);
        AssertEx.Equal(80d, saved.Window.Top);
        AssertEx.Equal(430d, saved.Window.Width);
        AssertEx.Equal(650d, saved.Window.Height);

        var migratedDay = HistoryFor(saved, "2026-09-15");
        var migratedItem = migratedDay.Items.Single();
        AssertEx.Equal(itemId, migratedItem.Id);
        AssertEx.Equal("Checklist lama", migratedItem.Text);
        AssertEx.True(migratedItem.IsCompleted, "Migration must snapshot completion before rollover.");
        AssertEx.Equal(createdAt, migratedItem.CreatedAt);

        var today = HistoryFor(saved, "2026-09-16");
        AssertEx.False(today.Items.Single().IsCompleted, "The new active day should be reset after migration.");

        var reloadStore = new InMemoryStateStore(saved);
        _ = new MainViewModel(reloadStore, () => now);
        AssertEx.Equal(0, reloadStore.SaveCount);
        var reloaded = AssertEx.NotNull(reloadStore.Snapshot);
        AssertEx.Equal(2, reloaded.History.Count);
        AssertEx.Equal(2, reloaded.History.Select(entry => entry.Date).Distinct().Count());
    }

    private static void FutureSchemaIsRejectedWithoutSaving()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 0, 0, TimeSpan.FromHours(7));
        var itemId = Guid.NewGuid();
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 8,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(itemId, "Data versi masa depan", true, 0, now.AddMinutes(-5))
            ],
            Settings = new AppSettings
            {
                LanguageCode = "en-US",
                ThemeCode = "dark"
            }
        });

        var exception = AssertEx.Throws<NotSupportedException>(
            () => _ = new MainViewModel(store, () => now));

        AssertEx.True(
            exception.Message.Contains("schema 8", StringComparison.Ordinal),
            "The error should identify the unsupported future schema.");
        AssertEx.Equal(0, store.SaveCount);

        var untouched = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(8, untouched.SchemaVersion);
        AssertEx.Equal(itemId, untouched.Items.Single().Id);
        AssertEx.True(untouched.Items.Single().IsCompleted, "Rejected future state must remain untouched.");
        AssertEx.Equal("en-US", untouched.Settings.LanguageCode);
        AssertEx.Equal("dark", untouched.Settings.ThemeCode);
    }

    private static void JsonStateStoreRoundTripsHistoryLanguageAndTheme()
    {
        WithTemporaryDirectory(directory =>
        {
            var statePath = Path.Combine(directory, "nested", "state.json");
            var now = new DateTimeOffset(2026, 9, 16, 9, 10, 11, TimeSpan.FromHours(7));
            var itemId = Guid.NewGuid();
            var scheduledItemId = Guid.NewGuid();
            var historicalItemId = Guid.NewGuid();
            var expected = new AppState
            {
                SchemaVersion = 7,
                CurrentDate = "2026-09-16",
                Items =
                [
                    CreateItemState(
                        itemId,
                        "Tulis jurnal",
                        false,
                        0,
                        now,
                        plannedDurationMinutes: 25,
                        remainingSeconds: 1_000,
                        timerStartedAt: now.AddSeconds(-5))
                ],
                ScheduledQuests =
                [
                    CreateScheduledQuestState(
                        scheduledItemId,
                        "Siapkan presentasi",
                        "2026-09-20",
                        0,
                        now.AddMinutes(1),
                        plannedDurationMinutes: 45)
                ],
                History =
                [
                    new DailyHistoryState
                    {
                        Date = "2026-09-15",
                        Items =
                        [
                            CreateItemState(historicalItemId, "Olahraga", false, 0, now.AddDays(-1))
                        ]
                    }
                ],
                Window = new WidgetWindowState
                {
                    Left = 123.5,
                    Top = 76.25,
                    Width = 420,
                    Height = 640
                },
                Settings = new AppSettings
                {
                    AlwaysOnTop = false,
                    LanguageCode = "en-US",
                    ThemeCode = "dark"
                }
            };
            var store = new JsonStateStore(statePath);

            store.Save(expected);
            var actual = AssertEx.NotNull(store.Load());

            AssertEx.True(File.Exists(statePath), "Save should create the state file and parent directory.");
            AssertEx.Equal(expected.SchemaVersion, actual.SchemaVersion);
            AssertEx.Equal(expected.CurrentDate, actual.CurrentDate);
            AssertEx.Equal(itemId, actual.Items.Single().Id);
            AssertEx.Equal("Tulis jurnal", actual.Items.Single().Text);
            AssertEx.False(actual.Items.Single().IsCompleted, "Completion should round-trip.");
            AssertEx.Equal(now, actual.Items.Single().CreatedAt);
            AssertEx.Equal(25, actual.Items.Single().PlannedDurationMinutes);
            AssertEx.Equal(1_000, actual.Items.Single().RemainingSeconds);
            AssertEx.Equal(now.AddSeconds(-5), actual.Items.Single().TimerStartedAt);
            AssertEx.Equal(1, actual.ScheduledQuests.Count);
            AssertEx.Equal(scheduledItemId, actual.ScheduledQuests.Single().Id);
            AssertEx.Equal("Siapkan presentasi", actual.ScheduledQuests.Single().Text);
            AssertEx.Equal("2026-09-20", actual.ScheduledQuests.Single().ScheduledDate);
            AssertEx.Equal(0, actual.ScheduledQuests.Single().SortOrder);
            AssertEx.Equal(now.AddMinutes(1), actual.ScheduledQuests.Single().CreatedAt);
            AssertEx.Equal(45, actual.ScheduledQuests.Single().PlannedDurationMinutes);
            AssertEx.Equal(1, actual.History.Count);
            AssertEx.Equal("2026-09-15", actual.History.Single().Date);
            AssertEx.Equal(historicalItemId, actual.History.Single().Items.Single().Id);
            AssertEx.Equal("Olahraga", actual.History.Single().Items.Single().Text);
            AssertEx.False(actual.History.Single().Items.Single().IsCompleted, "Historical completion should round-trip.");
            AssertEx.Equal(now.AddDays(-1), actual.History.Single().Items.Single().CreatedAt);
            AssertEx.Equal(123.5, actual.Window.Left);
            AssertEx.Equal(76.25, actual.Window.Top);
            AssertEx.Equal(420d, actual.Window.Width);
            AssertEx.Equal(640d, actual.Window.Height);
            AssertEx.False(actual.Settings.AlwaysOnTop, "Settings should round-trip.");
            AssertEx.Equal("en-US", actual.Settings.LanguageCode);
            AssertEx.Equal("dark", actual.Settings.ThemeCode);
            AssertEx.False(File.Exists(statePath + ".tmp"), "Atomic-save temporary file should be cleaned up.");
        });
    }

    private static void JsonStateStoreRecoversFromCorruptState()
    {
        WithTemporaryDirectory(directory =>
        {
            var statePath = Path.Combine(directory, "state.json");
            const string corruptJson = "{ this is not valid JSON";
            File.WriteAllText(statePath, corruptJson);
            var store = new JsonStateStore(statePath);

            var loaded = store.Load();

            AssertEx.Null(loaded);
            var backups = Directory.GetFiles(directory, "state.json.broken-*");
            AssertEx.Equal(1, backups.Length);
            AssertEx.Equal(corruptJson, File.ReadAllText(backups[0]));

            var recovered = new AppState
            {
                CurrentDate = "2026-09-16",
                Items = []
            };
            store.Save(recovered);

            AssertEx.Equal("2026-09-16", AssertEx.NotNull(store.Load()).CurrentDate);
        });
    }

    private static void JsonStateStoreRecoversFromNullRootState()
    {
        WithTemporaryDirectory(directory =>
        {
            var statePath = Path.Combine(directory, "state.json");
            const string nullJson = "null";
            File.WriteAllText(statePath, nullJson);
            var store = new JsonStateStore(statePath);

            var loaded = store.Load();

            AssertEx.Null(loaded);
            var backups = Directory.GetFiles(directory, "state.json.broken-*");
            AssertEx.Equal(1, backups.Length);
            AssertEx.Equal(nullJson, File.ReadAllText(backups[0]));

            var recovered = new AppState
            {
                SchemaVersion = 4,
                CurrentDate = "2026-09-16",
                Items = [],
                History = [],
                Settings = new AppSettings
                {
                    LanguageCode = "id-ID",
                    ThemeCode = "dark"
                }
            };
            store.Save(recovered);

            var reloaded = AssertEx.NotNull(store.Load());
            AssertEx.Equal(4, reloaded.SchemaVersion);
            AssertEx.Equal("2026-09-16", reloaded.CurrentDate);
            AssertEx.Equal("id-ID", reloaded.Settings.LanguageCode);
            AssertEx.Equal("dark", reloaded.Settings.ThemeCode);
            AssertEx.Equal(0, reloaded.Items.Count);
            AssertEx.Equal(0, reloaded.History.Count);
            AssertEx.Equal(nullJson, File.ReadAllText(backups[0]));
            AssertEx.False(File.Exists(statePath + ".tmp"), "Recovery save should clean up its temporary file.");
        });
    }

    private static void JsonStateStoreMigratesValidLegacyBytesAndPreservesSource()
    {
        WithTemporaryDirectory(directory =>
        {
            var targetPath = Path.Combine(directory, "DailyQuest", "state.json");
            var legacyPath = Path.Combine(directory, "MorningCheckIn", "state.json");
            const string legacyJson = """
                {
                  "SchemaVersion": 2,
                  "CurrentDate": "2026-09-15",
                  "Items": [],
                  "History": [],
                  "Window": { "Left": 101.25, "Top": 42.5, "Width": 430, "Height": 650 },
                  "Settings": { "AlwaysOnTop": false, "LanguageCode": "en-US" },
                  "IgnoredLegacyProperty": "must survive the byte copy"
                }
                """;
            var originalBytes = WriteUtf8File(legacyPath, legacyJson);
            var store = new JsonStateStore(targetPath, legacyPath);

            var loaded = AssertEx.NotNull(store.Load());

            AssertEx.Equal("2026-09-15", loaded.CurrentDate);
            AssertEx.Equal("en-US", loaded.Settings.LanguageCode);
            AssertEx.False(loaded.Settings.AlwaysOnTop, "Migrated settings should be loaded.");
            AssertEx.Equal(101.25, loaded.Window.Left);
            AssertEx.True(File.Exists(targetPath), "A valid legacy state should be copied to the new target.");
            AssertEx.True(File.Exists(legacyPath), "Migration must retain the legacy source as a rollback copy.");
            AssertEx.SequenceEqual(originalBytes, File.ReadAllBytes(targetPath));
            AssertEx.SequenceEqual(originalBytes, File.ReadAllBytes(legacyPath));
            AssertEx.Equal(
                0,
                Directory.GetFiles(Path.GetDirectoryName(targetPath)!, "state.json.migration-*").Length);
        });
    }

    private static void JsonStateStorePrefersExistingTargetOverLegacyState()
    {
        WithTemporaryDirectory(directory =>
        {
            var targetPath = Path.Combine(directory, "DailyQuest", "state.json");
            var legacyPath = Path.Combine(directory, "MorningCheckIn", "state.json");
            const string targetJson = """
                {
                  "SchemaVersion": 2,
                  "CurrentDate": "target-wins",
                  "Items": [],
                  "History": [],
                  "Window": {},
                  "Settings": { "AlwaysOnTop": true, "LanguageCode": "en-US" }
                }
                """;
            const string legacyJson = """
                {
                  "SchemaVersion": 2,
                  "CurrentDate": "legacy-must-not-win",
                  "Items": [],
                  "History": [],
                  "Window": {},
                  "Settings": { "AlwaysOnTop": false, "LanguageCode": "id-ID" }
                }
                """;
            var targetBytes = WriteUtf8File(targetPath, targetJson);
            var legacyBytes = WriteUtf8File(legacyPath, legacyJson);
            var store = new JsonStateStore(targetPath, legacyPath);

            var loaded = AssertEx.NotNull(store.Load());

            AssertEx.Equal("target-wins", loaded.CurrentDate);
            AssertEx.Equal("en-US", loaded.Settings.LanguageCode);
            AssertEx.SequenceEqual(targetBytes, File.ReadAllBytes(targetPath));
            AssertEx.SequenceEqual(legacyBytes, File.ReadAllBytes(legacyPath));
            AssertEx.Equal(
                0,
                Directory.GetFiles(Path.GetDirectoryName(targetPath)!, "state.json.migration-*").Length);
        });
    }

    private static void JsonStateStoreCustomPathDoesNotProbeLegacyState()
    {
        WithTemporaryDirectory(directory =>
        {
            var profileDirectory = Path.Combine(directory, "profile");
            var customPath = Path.Combine(profileDirectory, "DailyQuest", "state.json");
            var temptingLegacyPath = Path.Combine(profileDirectory, "MorningCheckIn", "state.json");
            const string legacyJson = """
                {
                  "SchemaVersion": 2,
                  "CurrentDate": "must-not-be-probed",
                  "Items": [],
                  "History": [],
                  "Window": {},
                  "Settings": {}
                }
                """;
            var legacyBytes = WriteUtf8File(temptingLegacyPath, legacyJson);
            var store = new JsonStateStore(customPath);
            var legacyPathField = AssertEx.NotNull(
                typeof(JsonStateStore).GetField(
                    "_legacyStatePath",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            AssertEx.Null(legacyPathField.GetValue(store));
            AssertEx.Null(store.Load());
            AssertEx.False(File.Exists(customPath), "A custom target must not be populated from any legacy path.");
            AssertEx.SequenceEqual(legacyBytes, File.ReadAllBytes(temptingLegacyPath));
        });
    }

    private static void JsonStateStoreSnapshotsCorruptLegacyMigration() =>
        AssertFailedLegacyMigrationPreservesSource("{ this is not valid JSON");

    private static void JsonStateStoreSnapshotsNullLegacyMigration() =>
        AssertFailedLegacyMigrationPreservesSource("null");

    private static void AssertFailedLegacyMigrationPreservesSource(string legacyJson)
    {
        WithTemporaryDirectory(directory =>
        {
            var targetPath = Path.Combine(directory, "DailyQuest", "state.json");
            var legacyPath = Path.Combine(directory, "MorningCheckIn", "state.json");
            var originalBytes = WriteUtf8File(legacyPath, legacyJson);
            var store = new JsonStateStore(targetPath, legacyPath);

            var loaded = store.Load();

            AssertEx.Null(loaded);
            AssertEx.False(File.Exists(targetPath), "An invalid legacy state must not become the active target.");
            AssertEx.True(File.Exists(legacyPath), "A failed migration must retain its legacy source.");
            AssertEx.SequenceEqual(originalBytes, File.ReadAllBytes(legacyPath));

            var targetDirectory = Path.GetDirectoryName(targetPath)!;
            var brokenSnapshots = Directory.GetFiles(targetDirectory, "state.json.migration-broken-*");
            AssertEx.Equal(1, brokenSnapshots.Length);
            AssertEx.SequenceEqual(originalBytes, File.ReadAllBytes(brokenSnapshots[0]));
            AssertEx.Equal(0, Directory.GetFiles(targetDirectory, "state.json.migration-*.tmp").Length);
        });
    }

    private static byte[] WriteUtf8File(string path, string contents)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var bytes = Encoding.UTF8.GetBytes(contents);
        File.WriteAllBytes(path, bytes);
        return bytes;
    }

    private static DailyHistoryState HistoryFor(AppState state, string date) =>
        state.History.Single(entry => string.Equals(entry.Date, date, StringComparison.Ordinal));

    private static ChecklistItemState CreateItemState(
        Guid id,
        string text,
        bool isCompleted,
        int sortOrder,
        DateTimeOffset createdAt,
        int? plannedDurationMinutes = null,
        int? remainingSeconds = null,
        DateTimeOffset? timerStartedAt = null,
        Guid? labelId = null,
        int? manualSortOrder = null,
        bool isOvertime = false,
        int overtimeSeconds = 0) => new()
        {
            Id = id,
            Text = text,
            IsCompleted = isCompleted,
            SortOrder = sortOrder,
            ManualSortOrder = manualSortOrder ?? sortOrder,
            CreatedAt = createdAt,
            PlannedDurationMinutes = plannedDurationMinutes,
            RemainingSeconds = remainingSeconds,
            TimerStartedAt = timerStartedAt,
            IsOvertime = isOvertime,
            OvertimeSeconds = overtimeSeconds,
            LabelId = labelId
        };

    private static ScheduledQuestState CreateScheduledQuestState(
        Guid id,
        string text,
        string scheduledDate,
        int sortOrder,
        DateTimeOffset createdAt,
        int? plannedDurationMinutes = null,
        Guid? labelId = null) => new()
        {
            Id = id,
            Text = text,
            ScheduledDate = scheduledDate,
            SortOrder = sortOrder,
            CreatedAt = createdAt,
            PlannedDurationMinutes = plannedDurationMinutes,
            LabelId = labelId
        };

    private static void WithTemporaryDirectory(Action<string> test)
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "DailyQuest.LogicTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            test(directory);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}

internal sealed class MutableClock(DateTimeOffset now)
{
    public DateTimeOffset Now { get; set; } = now;
}

internal sealed class InMemoryStateStore : IStateStore
{
    private static readonly JsonSerializerOptions CloneOptions = new();
    private AppState? _state;

    public InMemoryStateStore(AppState? initialState = null)
    {
        _state = Clone(initialState);
    }

    public int SaveCount { get; private set; }

    public AppState? Snapshot => Clone(_state);

    public AppState? Load() => Clone(_state);

    public void Save(AppState state)
    {
        _state = Clone(state);
        SaveCount++;
    }

    private static AppState? Clone(AppState? state)
    {
        if (state is null)
        {
            return null;
        }

        var json = JsonSerializer.Serialize(state, CloneOptions);
        return JsonSerializer.Deserialize<AppState>(json, CloneOptions)
            ?? throw new InvalidOperationException("Could not clone state for the in-memory test store.");
    }
}

internal sealed class RecordingStateStore(IStateStore inner) : IStateStore
{
    public int SaveCount { get; private set; }

    public AppState? Load() => inner.Load();

    public void Save(AppState state)
    {
        inner.Save(state);
        SaveCount++;
    }
}

internal sealed class RecordingThemeService : IThemeService
{
    public List<string> AppliedThemes { get; } = [];

    public void Apply(string themeCode) => AppliedThemes.Add(themeCode);
}

internal sealed class RecordingQuestAlarmService : IQuestAlarmService
{
    public List<string> Notifications { get; } = [];

    public int StopCount { get; private set; }

    public void NotifyTimerCompleted(string questText) => Notifications.Add(questText);

    public void StopTimerAlarm() => StopCount++;
}

internal sealed class FakeStorageUsageService(
    Func<IReadOnlyCollection<DailyHistoryState>, StorageUsageSnapshot> measure) : IStorageUsageService
{
    public int MeasureCount { get; private set; }

    public int LastHistoryCount { get; private set; } = -1;

    public StorageUsageSnapshot Measure(IReadOnlyCollection<DailyHistoryState> history)
    {
        MeasureCount++;
        LastHistoryCount = history.Count;
        return measure(history);
    }
}

internal static class AssertEx
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message ?? "Expected true, but was false.");
        }
    }

    public static void False(bool condition, string? message = null) =>
        True(!condition, message ?? "Expected false, but was true.");

    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected <{expected}>, but was <{actual}>.");
        }
    }

    public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        var expectedItems = expected.ToArray();
        var actualItems = actual.ToArray();
        if (!expectedItems.SequenceEqual(actualItems))
        {
            throw new InvalidOperationException(
                $"Expected sequence <{string.Join(", ", expectedItems)}>, " +
                $"but was <{string.Join(", ", actualItems)}>.");
        }
    }

    public static TException Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Expected {typeof(TException).Name}, but caught {exception.GetType().Name}.",
                exception);
        }

        throw new InvalidOperationException(
            $"Expected {typeof(TException).Name}, but no exception was thrown.");
    }

    public static T NotNull<T>(T? value)
        where T : class
    {
        if (value is null)
        {
            throw new InvalidOperationException("Expected a non-null value.");
        }

        return value;
    }

    public static void Null<T>(T? value)
        where T : class
    {
        if (value is not null)
        {
            throw new InvalidOperationException($"Expected null, but was <{value}>.");
        }
    }
}
