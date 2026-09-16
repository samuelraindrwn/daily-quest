namespace DailyQuest.ViewModels;

public enum QuestSortMode
{
    Manual,
    Label,
    DurationAscending,
    DurationDescending
}

public static class QuestSortModeCodes
{
    public const string Manual = "manual";
    public const string Label = "label";
    public const string DurationAscending = "duration-asc";
    public const string DurationDescending = "duration-desc";

    public static string ToCode(QuestSortMode mode) => mode switch
    {
        QuestSortMode.Label => Label,
        QuestSortMode.DurationAscending => DurationAscending,
        QuestSortMode.DurationDescending => DurationDescending,
        _ => Manual
    };

    public static QuestSortMode Parse(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        Label => QuestSortMode.Label,
        DurationAscending => QuestSortMode.DurationAscending,
        DurationDescending => QuestSortMode.DurationDescending,
        _ => QuestSortMode.Manual
    };
}
