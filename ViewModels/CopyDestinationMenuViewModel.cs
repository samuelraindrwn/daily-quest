namespace DailyQuest.ViewModels;

public sealed class CopyDestinationMenuViewModel
{
    public required bool ShowQuestActions { get; init; }

    public required string EditQuestText { get; init; }

    public required string Title { get; init; }

    public required string EmptyMessage { get; init; }

    public required string AllFutureDaysText { get; init; }

    public required string AllFutureDaysRangeText { get; init; }

    public required IReadOnlyList<ScheduleOptionViewModel> ScheduleOptions { get; init; }

    public required bool HasSourceQuests { get; init; }
}
