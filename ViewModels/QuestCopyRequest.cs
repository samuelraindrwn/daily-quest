using DailyQuest.Models;

namespace DailyQuest.ViewModels;

public sealed record QuestCopyRequest(ChecklistItem Item, int Offset);

public sealed record ScheduleDayCopyRequest(int SourceOffset, int TargetOffset);
