namespace DailyQuest.Models;

public sealed class AppState
{
    public int SchemaVersion { get; set; } = 2;

    public string CurrentDate { get; set; } = string.Empty;

    public List<ChecklistItemState> Items { get; set; } = [];

    public List<DailyHistoryState> History { get; set; } = [];

    public WidgetWindowState Window { get; set; } = new();

    public AppSettings Settings { get; set; } = new();
}

public sealed class ChecklistItemState
{
    public Guid Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
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

    public double Width { get; set; } = 390;

    public double Height { get; set; } = 610;
}

public sealed class AppSettings
{
    public bool AlwaysOnTop { get; set; } = true;

    public string LanguageCode { get; set; } = "id-ID";
}
