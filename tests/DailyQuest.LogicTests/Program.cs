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
        ("today, upcoming, history, and settings navigation is mutually exclusive", ViewNavigationIsMutuallyExclusive),
        ("unknown language falls back to Indonesian", UnknownLanguageFallsBackToIndonesian),
        ("language normalization preserves history-only completed items", LanguageNormalizationPreservesHistoryOnlyCompletedItems),
        ("current-day history tracks add, toggle, remove, and clear", CurrentDayHistoryTracksChecklistMutations),
        ("composer schedules only today through H plus eight", ComposerSchedulesOnlyTodayThroughEightDays),
        ("scheduled quests stay outside today's checklist until due", ScheduledQuestsStayOutsideTodayUntilDue),
        ("upcoming quests can be canceled without changing today", UpcomingQuestsCanBeCanceledWithoutChangingToday),
        ("next pending item follows quest order and completion", NextPendingItemFollowsQuestOrderAndCompletion),
        ("quest reorder persists to active state and current history", QuestReorderPersistsToActiveStateAndCurrentHistory),
        ("quest reorder retains completed history-only items", QuestReorderRetainsCompletedHistoryOnlyItems),
        ("quest reorder rejects foreign and unchanged moves", QuestReorderRejectsForeignAndUnchangedMoves),
        ("persisted sort order is normalized stably", PersistedSortOrderIsNormalizedStably),
        ("clear history preserves active quests", ClearHistoryPreservesActiveQuests),
        ("settings refreshes injected storage usage", SettingsRefreshesInjectedStorageUsage),
        ("storage usage failures degrade gracefully", StorageUsageFailuresDegradeGracefully),
        ("daily rollover archives once without duplicates", DailyRolloverArchivesOnceWithoutDuplicates),
        ("daily rollover activates due quests after archiving", DailyRolloverActivatesDueQuestsAfterArchiving),
        ("overdue quests activate once on the next launch", OverdueQuestsActivateOnceOnNextLaunch),
        ("legacy v2 state migrates with an empty future queue", LegacyV2StateMigratesWithEmptyFutureQueue),
        ("legacy v1 state migrates without history loss", LegacyV1StateMigratesWithoutHistoryLoss),
        ("future schema is rejected without saving", FutureSchemaIsRejectedWithoutSaving),
        ("JSON state store round-trips history and language", JsonStateStoreRoundTripsHistoryAndLanguage),
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

        var viewModel = new MainViewModel(store, () => now);

        AssertEx.Equal(0, viewModel.TotalCount);
        AssertEx.Equal(0, viewModel.CompletedCount);
        AssertEx.Equal(0d, viewModel.ProgressPercent);
        AssertEx.Equal("Belum ada aktivitas", viewModel.ProgressText);
        AssertEx.Equal(0, viewModel.Items.Count);
        AssertEx.Equal(0, viewModel.HistoryEntries.Count);
        AssertEx.Equal(1, store.SaveCount);

        var saved = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(3, saved.SchemaVersion);
        AssertEx.Equal("2026-09-16", saved.CurrentDate);
        AssertEx.Equal(0, saved.Items.Count);
        AssertEx.Equal(0, saved.ScheduledQuests.Count);
        AssertEx.Equal(0, saved.History.Count);
        AssertEx.True(saved.Settings.AlwaysOnTop, "Always-on-top should default to enabled.");
        AssertEx.Equal("id-ID", saved.Settings.LanguageCode);

        var reloadStore = new InMemoryStateStore(saved);
        var reloaded = new MainViewModel(reloadStore, () => now);

        AssertEx.Equal(0, reloaded.TotalCount);
        AssertEx.Equal(0, reloaded.Items.Count);
        AssertEx.Equal(0, reloaded.HistoryEntries.Count);
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

    private static void UnknownLanguageFallsBackToIndonesian()
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

        AssertEx.Equal("id-ID", viewModel.LanguageCode);
        AssertEx.Equal("ID", viewModel.LanguageBadge);
        AssertEx.Equal("Selamat pagi!", viewModel.Greeting);
        AssertEx.Equal("id-ID", AssertEx.NotNull(store.Snapshot).Settings.LanguageCode);
        AssertEx.Equal(1, store.SaveCount);
    }

    private static void LanguageNormalizationPreservesHistoryOnlyCompletedItems()
    {
        var now = new DateTimeOffset(2026, 9, 16, 7, 30, 0, TimeSpan.FromHours(7));
        var orphanId = Guid.NewGuid();
        var orphanCreatedAt = now.AddMinutes(-20);
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 3,
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

        AssertEx.Equal("id-ID", viewModel.LanguageCode);
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
        AssertEx.Equal("id-ID", saved.Settings.LanguageCode);
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
            [removedAfterCompletion.Id, cleared.Id, active.Id],
            afterClear.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([true, true, false], afterClear.Items.Select(item => item.IsCompleted));

        viewModel.RemoveItemCommand.Execute(active);
        saved = AssertEx.NotNull(store.Snapshot);
        var finalHistory = HistoryFor(saved, "2026-09-16");
        AssertEx.SequenceEqual(
            [removedAfterCompletion.Id, cleared.Id],
            finalHistory.Items.Select(item => item.Id));
        AssertEx.True(finalHistory.Items.All(item => item.IsCompleted), "Only completed historical items should remain.");
        AssertEx.Equal(1, viewModel.HistoryEntries.Count);
        AssertEx.Equal("2 dari 2 selesai", viewModel.HistoryEntries.Single().SummaryText);
        AssertEx.Equal(10, store.SaveCount);
    }

    private static void ComposerSchedulesOnlyTodayThroughEightDays()
    {
        var now = new DateTimeOffset(2026, 9, 16, 23, 45, 0, TimeSpan.FromHours(7));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 3,
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

    private static void ScheduledQuestsStayOutsideTodayUntilDue()
    {
        var now = new DateTimeOffset(2026, 9, 16, 8, 0, 0, TimeSpan.FromHours(7));
        var activeId = Guid.NewGuid();
        var activeState = CreateItemState(activeId, "Quest hari ini", false, 0, now.AddMinutes(-1));
        var store = new InMemoryStateStore(new AppState
        {
            SchemaVersion = 3,
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
            SchemaVersion = 3,
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
        second.IsCompleted = true;
        AssertEx.Null(viewModel.NextPendingItem);
        AssertEx.False(viewModel.HasPendingItem, "No pending quest should remain after every quest is complete.");

        var first = viewModel.Items.Single(item => item.Id == firstId);
        first.IsCompleted = false;
        AssertEx.Equal(firstId, AssertEx.NotNull(viewModel.NextPendingItem).Id);
        AssertEx.True(viewModel.HasPendingItem);
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
            [completedId, pendingId],
            viewModel.Items.Select(item => item.Id));
        AssertEx.SequenceEqual(
            [true, false],
            viewModel.Items.Select(item => item.IsCompleted));
        AssertEx.Equal(0, viewModel.HistoryEntries.Count);
        AssertEx.Equal(1, viewModel.UpcomingQuests.Count);
        AssertEx.False(viewModel.ClearHistoryCommand.CanExecute(null));
        AssertEx.Equal(1, store.SaveCount);
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
            SchemaVersion = 3,
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
        AssertEx.SequenceEqual([completedId, pendingId], previousDay.Items.Select(item => item.Id));
        AssertEx.SequenceEqual([true, false], previousDay.Items.Select(item => item.IsCompleted));

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
            SchemaVersion = 3,
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
            SchemaVersion = 3,
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
        AssertEx.Equal(3, saved.SchemaVersion);
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
        AssertEx.Equal(3, saved.SchemaVersion);
        AssertEx.Equal("2026-09-16", saved.CurrentDate);
        AssertEx.Equal("id-ID", saved.Settings.LanguageCode);
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
            SchemaVersion = 4,
            CurrentDate = "2026-09-16",
            Items =
            [
                CreateItemState(itemId, "Data versi masa depan", true, 0, now.AddMinutes(-5))
            ],
            Settings = new AppSettings
            {
                LanguageCode = "en-US"
            }
        });

        var exception = AssertEx.Throws<NotSupportedException>(
            () => _ = new MainViewModel(store, () => now));

        AssertEx.True(
            exception.Message.Contains("schema 4", StringComparison.Ordinal),
            "The error should identify the unsupported future schema.");
        AssertEx.Equal(0, store.SaveCount);

        var untouched = AssertEx.NotNull(store.Snapshot);
        AssertEx.Equal(4, untouched.SchemaVersion);
        AssertEx.Equal(itemId, untouched.Items.Single().Id);
        AssertEx.True(untouched.Items.Single().IsCompleted, "Rejected future state must remain untouched.");
        AssertEx.Equal("en-US", untouched.Settings.LanguageCode);
    }

    private static void JsonStateStoreRoundTripsHistoryAndLanguage()
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
                SchemaVersion = 3,
                CurrentDate = "2026-09-16",
                Items =
                [
                    CreateItemState(itemId, "Tulis jurnal", true, 0, now)
                ],
                ScheduledQuests =
                [
                    CreateScheduledQuestState(
                        scheduledItemId,
                        "Siapkan presentasi",
                        "2026-09-20",
                        0,
                        now.AddMinutes(1))
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
                    LanguageCode = "en-US"
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
            AssertEx.True(actual.Items.Single().IsCompleted, "Completion should round-trip.");
            AssertEx.Equal(now, actual.Items.Single().CreatedAt);
            AssertEx.Equal(1, actual.ScheduledQuests.Count);
            AssertEx.Equal(scheduledItemId, actual.ScheduledQuests.Single().Id);
            AssertEx.Equal("Siapkan presentasi", actual.ScheduledQuests.Single().Text);
            AssertEx.Equal("2026-09-20", actual.ScheduledQuests.Single().ScheduledDate);
            AssertEx.Equal(0, actual.ScheduledQuests.Single().SortOrder);
            AssertEx.Equal(now.AddMinutes(1), actual.ScheduledQuests.Single().CreatedAt);
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
                SchemaVersion = 3,
                CurrentDate = "2026-09-16",
                Items = [],
                History = [],
                Settings = new AppSettings
                {
                    LanguageCode = "id-ID"
                }
            };
            store.Save(recovered);

            var reloaded = AssertEx.NotNull(store.Load());
            AssertEx.Equal(3, reloaded.SchemaVersion);
            AssertEx.Equal("2026-09-16", reloaded.CurrentDate);
            AssertEx.Equal("id-ID", reloaded.Settings.LanguageCode);
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
        DateTimeOffset createdAt) => new()
        {
            Id = id,
            Text = text,
            IsCompleted = isCompleted,
            SortOrder = sortOrder,
            CreatedAt = createdAt
        };

    private static ScheduledQuestState CreateScheduledQuestState(
        Guid id,
        string text,
        string scheduledDate,
        int sortOrder,
        DateTimeOffset createdAt) => new()
        {
            Id = id,
            Text = text,
            ScheduledDate = scheduledDate,
            SortOrder = sortOrder,
            CreatedAt = createdAt
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
