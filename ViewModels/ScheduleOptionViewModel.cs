namespace DailyQuest.ViewModels;

public sealed class ScheduleOptionViewModel
{
    public required int Offset { get; init; }

    public required string Label { get; init; }

    public required string DateText { get; init; }

    public required string DisplayText { get; init; }

    public required bool IsSelected { get; init; }
}
