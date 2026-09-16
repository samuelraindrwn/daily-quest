using DailyQuest.Models;

namespace DailyQuest.Services;

public interface IStateStore
{
    AppState? Load();

    void Save(AppState state);
}
