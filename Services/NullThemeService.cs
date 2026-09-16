namespace DailyQuest.Services;

public sealed class NullThemeService : IThemeService
{
    public static NullThemeService Instance { get; } = new();

    private NullThemeService()
    {
    }

    public void Apply(string themeCode)
    {
    }
}
