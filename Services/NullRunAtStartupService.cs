namespace DailyQuest.Services;

public sealed class NullRunAtStartupService : IRunAtStartupService
{
    public static NullRunAtStartupService Instance { get; } = new();

    private NullRunAtStartupService()
    {
    }

    public bool TrySetEnabled(bool enabled) => true;
}
