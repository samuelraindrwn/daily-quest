namespace DailyQuest.ViewModels;

public sealed class CopyDestinationMenuViewModel
{
    public required string Title { get; init; }

    public required string EmptyMessage { get; init; }

    public required IReadOnlyList<ScheduleOptionViewModel> ScheduleOptions { get; init; }

    public required bool HasSourceQuests { get; init; }
}
