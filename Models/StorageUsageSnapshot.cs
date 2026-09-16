namespace DailyQuest.Models;

public readonly record struct StorageUsageSnapshot(
    long ApplicationBytes,
    long DataBytes,
    long HistoryBytes);
