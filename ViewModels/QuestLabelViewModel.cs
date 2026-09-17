using System.ComponentModel;
using DailyQuest.Models;

namespace DailyQuest.ViewModels;

public sealed class QuestLabelViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private int _sortOrder;

    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string ColorHex { get; init; }

    public required int SortOrder
    {
        get => _sortOrder;
        init => _sortOrder = value;
    }

    public bool IsSelected
    {
        get => _isSelected;
        internal set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    internal void UpdateSortOrder(int sortOrder)
    {
        if (_sortOrder == sortOrder)
        {
            return;
        }

        _sortOrder = sortOrder;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SortOrder)));
    }
}

public sealed record QuestLabelDraft(string Name, string ColorHex);

public sealed record QuestLabelUpdateRequest(Guid Id, string Name, string ColorHex);

public sealed record QuestItemLabelRequest(ChecklistItem Item, Guid? LabelId);
