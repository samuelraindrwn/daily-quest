namespace DailyQuest.Services;

public interface IQuestAlarmService
{
    void NotifyTimerCompleted(string questText, bool repeatUntilStopped);

    void StopTimerAlarm();
}
