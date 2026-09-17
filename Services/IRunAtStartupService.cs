namespace DailyQuest.Services;

public interface IRunAtStartupService
{
    bool TrySetEnabled(bool enabled);
}
