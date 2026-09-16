using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DailyQuest.Models;

public sealed class ChecklistItem : INotifyPropertyChanged
{
    private bool _isCompleted;

    public ChecklistItem(Guid id, string text, bool isCompleted, DateTimeOffset createdAt)
    {
        Id = id;
        Text = text;
        _isCompleted = isCompleted;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public string Text { get; }

    public DateTimeOffset CreatedAt { get; }

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (_isCompleted == value)
            {
                return;
            }

            _isCompleted = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
