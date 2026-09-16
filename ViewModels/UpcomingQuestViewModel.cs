namespace DailyQuest.ViewModels;

public sealed class UpcomingQuestViewModel
{
    public required Guid Id { get; init; }

    public required string Text { get; init; }

    public required string ScheduledDate { get; init; }

    public required string DateText { get; init; }

    public required string ScheduleLabel { get; init; }

    public required string DisplayDate { get; init; }
}
