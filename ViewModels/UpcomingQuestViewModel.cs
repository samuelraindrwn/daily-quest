namespace DailyQuest.ViewModels;

public sealed class UpcomingQuestViewModel
{
    public required Guid Id { get; init; }

    public required string Text { get; init; }

    public required string ScheduledDate { get; init; }

    public required string DateText { get; init; }

    public required string ScheduleLabel { get; init; }

    public int? PlannedDurationMinutes { get; init; }

    public bool HasTimer => PlannedDurationMinutes.HasValue;

    public string? DurationText { get; init; }

    public Guid? LabelId { get; init; }

    public bool HasLabel => LabelId.HasValue;

    public string? LabelName { get; init; }

    public string? LabelColorHex { get; init; }

    public required string DisplayDate { get; init; }
}
