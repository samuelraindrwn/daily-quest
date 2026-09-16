namespace DailyQuest.Models;

public sealed class AppState
{
    public int SchemaVersion { get; set; } = 7;

    public string CurrentDate { get; set; } = string.Empty;

    public List<ChecklistItemState> Items { get; set; } = [];

    public List<DailyHistoryState> History { get; set; } = [];

    public List<ScheduledQuestState> ScheduledQuests { get; set; } = [];

    public List<QuestLabelState> Labels { get; set; } = [];

    public WidgetWindowState Window { get; set; } = new();

    public AppSettings Settings { get; set; } = new();
}

public sealed class ScheduledQuestState
{
    public Guid Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public string ScheduledDate { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int? PlannedDurationMinutes { get; set; }

    public Guid? LabelId { get; set; }
}

public sealed class ChecklistItemState
{
    public Guid Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public int SortOrder { get; set; }

    public int? ManualSortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int? PlannedDurationMinutes { get; set; }

    public int? RemainingSeconds { get; set; }

    public DateTimeOffset? TimerStartedAt { get; set; }

    public bool IsOvertime { get; set; }

    public int OvertimeSeconds { get; set; }

    public Guid? LabelId { get; set; }
}

public sealed class QuestLabelState
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ColorHex { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

public sealed class DailyHistoryState
{
    public string Date { get; set; } = string.Empty;

    public List<ChecklistItemState> Items { get; set; } = [];
}

public sealed class WidgetWindowState
{
    public double? Left { get; set; }

    public double? Top { get; set; }

    public double Width { get; set; } = 520;

    public double Height { get; set; } = 680;
}

public sealed class AppSettings
{
    public bool AlwaysOnTop { get; set; } = true;

    public string LanguageCode { get; set; } = "en-US";

    public string ThemeCode { get; set; } = "light";

    public string QuestSortMode { get; set; } = "manual";

    public bool OvertimeEnabled { get; set; }
}
