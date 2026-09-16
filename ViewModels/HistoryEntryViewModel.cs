namespace DailyQuest.ViewModels;

public sealed class HistoryEntryViewModel
{
    public required string DateText { get; init; }

    public required string SummaryText { get; init; }

    public required double ProgressPercent { get; init; }

    public required IReadOnlyList<HistoryItemViewModel> Items { get; init; }
}

public sealed class HistoryItemViewModel
{
    public required string Text { get; init; }

    public required bool IsCompleted { get; init; }

    public string StatusGlyph => IsCompleted ? "✓" : "○";
}
