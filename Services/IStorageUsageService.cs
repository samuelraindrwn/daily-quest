using DailyQuest.Models;

namespace DailyQuest.Services;

public interface IStorageUsageService
{
    StorageUsageSnapshot Measure(IReadOnlyCollection<DailyHistoryState> history);
}
