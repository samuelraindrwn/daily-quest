namespace DailyQuest.Theming;

public static class ThemeCatalog
{
    public const string LightCode = "light";
    public const string DarkCode = "dark";

    public static string Normalize(string? code) =>
        TryNormalizeSelection(code, out var normalized) ? normalized : LightCode;

    public static bool TryNormalizeSelection(string? code, out string normalized)
    {
        normalized = code?.Trim().ToLowerInvariant() switch
        {
            LightCode => LightCode,
            DarkCode => DarkCode,
            _ => string.Empty
        };

        return normalized.Length > 0;
    }
}
