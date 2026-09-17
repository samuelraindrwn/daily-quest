namespace DailyQuest.Services;

public interface IQuestAlarmService
{
    void NotifyTimerCompleted(string questText);

    void StopTimerAlarm();
}
