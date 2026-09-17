using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DailyQuest.Models;

public sealed class ChecklistItem : INotifyPropertyChanged
{
    private bool _isCompleted;
    private int _remainingSeconds;
    private DateTimeOffset? _timerStartedAt;
    private Guid? _labelId;
    private string? _labelName;
    private string? _labelColorHex;
    private bool _isOvertime;
    private int _overtimeSeconds;

    public ChecklistItem(
        Guid id,
        string text,
        bool isCompleted,
        DateTimeOffset createdAt,
        int? plannedDurationMinutes = null,
        int? remainingSeconds = null,
        DateTimeOffset? timerStartedAt = null,
        Guid? labelId = null,
        string? labelName = null,
        string? labelColorHex = null,
        bool isOvertime = false,
        int overtimeSeconds = 0)
    {
        Id = id;
        Text = text;
        _isCompleted = isCompleted;
        CreatedAt = createdAt;
        PlannedDurationMinutes = plannedDurationMinutes is >= 1 and <= 480
            ? plannedDurationMinutes
            : null;

        var fullDurationSeconds = PlannedDurationMinutes.GetValueOrDefault() * 60;
        _remainingSeconds = PlannedDurationMinutes.HasValue
            ? Math.Clamp(remainingSeconds ?? fullDurationSeconds, 0, fullDurationSeconds)
            : 0;
        _isOvertime = !isCompleted && PlannedDurationMinutes.HasValue &&
                      _remainingSeconds == 0 && isOvertime;
        _overtimeSeconds = _isOvertime ? Math.Max(0, overtimeSeconds) : 0;
        _timerStartedAt = !isCompleted && (_remainingSeconds > 0 || _isOvertime)
            ? timerStartedAt
            : null;
        _labelId = labelId;
        _labelName = labelName;
        _labelColorHex = labelColorHex;
    }

    public Guid Id { get; }

    public string Text { get; }

    public DateTimeOffset CreatedAt { get; }

    public int? PlannedDurationMinutes { get; }

    public bool HasTimer => PlannedDurationMinutes.HasValue;

    public int RemainingSeconds => _remainingSeconds;

    public DateTimeOffset? TimerStartedAt => _timerStartedAt;

    public bool IsTimerRunning => _timerStartedAt.HasValue;

    public bool IsTimerExpired => HasTimer && !IsCompleted && !IsOvertime && RemainingSeconds == 0;

    public bool IsOvertime => _isOvertime;

    public int OvertimeSeconds => _overtimeSeconds;

    public bool CanStartOvertime => IsTimerExpired;

    public Guid? LabelId => _labelId;

    public bool HasLabel => _labelId.HasValue;

    public string? LabelName => _labelName;

    public string? LabelColorHex => _labelColorHex;

    public double TimerProgressPercent => !HasTimer || PlannedDurationMinutes == 0
        ? 0
        : IsOvertime
            ? 100
        : (double)(PlannedDurationMinutes.GetValueOrDefault() * 60 - RemainingSeconds) /
          (PlannedDurationMinutes.GetValueOrDefault() * 60) * 100;

    public string RemainingTimeText
    {
        get
        {
            var displayedSeconds = IsOvertime
                ? ((long)PlannedDurationMinutes.GetValueOrDefault() * 60) + OvertimeSeconds
                : RemainingSeconds;
            var duration = TimeSpan.FromSeconds(displayedSeconds);
            var formatted = PlannedDurationMinutes >= 60 || duration.TotalHours >= 1
                ? $"{(int)duration.TotalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}"
                : $"{duration.Minutes:00}:{duration.Seconds:00}";
            return IsOvertime ? $"+{formatted}" : formatted;
        }
    }

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
            OnPropertyChanged(nameof(IsTimerExpired));
            OnPropertyChanged(nameof(CanStartOvertime));
        }
    }

    internal bool StartTimer(DateTimeOffset now)
    {
        if (!HasTimer || IsCompleted || (!IsOvertime && RemainingSeconds <= 0) || IsTimerRunning)
        {
            return false;
        }

        _timerStartedAt = now;
        OnPropertyChanged(nameof(TimerStartedAt));
        OnPropertyChanged(nameof(IsTimerRunning));
        return true;
    }

    internal bool StartOvertime(DateTimeOffset now)
    {
        if (!CanStartOvertime || IsTimerRunning)
        {
            return false;
        }

        _isOvertime = true;
        _overtimeSeconds = 0;
        _timerStartedAt = now;
        OnPropertyChanged(nameof(IsOvertime));
        OnPropertyChanged(nameof(OvertimeSeconds));
        OnPropertyChanged(nameof(CanStartOvertime));
        OnPropertyChanged(nameof(IsTimerExpired));
        OnPropertyChanged(nameof(TimerStartedAt));
        OnPropertyChanged(nameof(IsTimerRunning));
        OnPropertyChanged(nameof(RemainingTimeText));
        OnPropertyChanged(nameof(TimerProgressPercent));
        return true;
    }

    internal bool ApplyLabel(Guid? labelId, string? labelName, string? labelColorHex)
    {
        if (_labelId == labelId &&
            string.Equals(_labelName, labelName, StringComparison.Ordinal) &&
            string.Equals(_labelColorHex, labelColorHex, StringComparison.Ordinal))
        {
            return false;
        }

        _labelId = labelId;
        _labelName = labelName;
        _labelColorHex = labelColorHex;
        OnPropertyChanged(nameof(LabelId));
        OnPropertyChanged(nameof(HasLabel));
        OnPropertyChanged(nameof(LabelName));
        OnPropertyChanged(nameof(LabelColorHex));
        return true;
    }

    internal bool PauseTimer()
    {
        if (!_timerStartedAt.HasValue)
        {
            return false;
        }

        _timerStartedAt = null;
        OnPropertyChanged(nameof(TimerStartedAt));
        OnPropertyChanged(nameof(IsTimerRunning));
        return true;
    }

    internal bool ResetTimer()
    {
        if (!HasTimer)
        {
            return false;
        }

        var fullDurationSeconds = PlannedDurationMinutes!.Value * 60;
        var changed = _timerStartedAt.HasValue ||
                      _remainingSeconds != fullDurationSeconds ||
                      _isOvertime ||
                      _overtimeSeconds != 0;
        if (!changed)
        {
            return false;
        }

        var wasRunning = _timerStartedAt.HasValue;
        var wasOvertime = _isOvertime;
        var previousOvertimeSeconds = _overtimeSeconds;
        _timerStartedAt = null;
        _remainingSeconds = fullDurationSeconds;
        _isOvertime = false;
        _overtimeSeconds = 0;

        OnPropertyChanged(nameof(TimerStartedAt));
        if (wasRunning)
        {
            OnPropertyChanged(nameof(IsTimerRunning));
        }

        if (wasOvertime)
        {
            OnPropertyChanged(nameof(IsOvertime));
        }

        if (previousOvertimeSeconds != 0)
        {
            OnPropertyChanged(nameof(OvertimeSeconds));
        }

        NotifyRemainingTimeChanged();
        return true;
    }

    internal bool ClearOvertime()
    {
        if (!IsOvertime)
        {
            return false;
        }

        var wasRunning = IsTimerRunning;
        _isOvertime = false;
        _overtimeSeconds = 0;
        _timerStartedAt = null;
        OnPropertyChanged(nameof(IsOvertime));
        OnPropertyChanged(nameof(OvertimeSeconds));
        OnPropertyChanged(nameof(CanStartOvertime));
        OnPropertyChanged(nameof(IsTimerExpired));
        OnPropertyChanged(nameof(TimerStartedAt));
        if (wasRunning)
        {
            OnPropertyChanged(nameof(IsTimerRunning));
        }

        OnPropertyChanged(nameof(RemainingTimeText));
        OnPropertyChanged(nameof(TimerProgressPercent));
        return true;
    }

    internal TimerAdvanceResult AdvanceTimer(DateTimeOffset now)
    {
        if (!_timerStartedAt.HasValue)
        {
            return TimerAdvanceResult.None;
        }

        var elapsed = now - _timerStartedAt.Value;
        if (elapsed < TimeSpan.Zero)
        {
            _timerStartedAt = now;
            OnPropertyChanged(nameof(TimerStartedAt));
            return TimerAdvanceResult.ClockAdjusted;
        }

        var elapsedSeconds = elapsed.TotalSeconds;
        if (elapsedSeconds < 1)
        {
            return TimerAdvanceResult.None;
        }

        if (IsOvertime)
        {
            var overtimeElapsedSeconds = (int)Math.Min(int.MaxValue, Math.Floor(elapsedSeconds));
            _overtimeSeconds = (int)Math.Min(
                int.MaxValue,
                (long)_overtimeSeconds + overtimeElapsedSeconds);
            _timerStartedAt = _timerStartedAt.Value.AddSeconds(overtimeElapsedSeconds);
            OnPropertyChanged(nameof(TimerStartedAt));
            OnPropertyChanged(nameof(OvertimeSeconds));
            OnPropertyChanged(nameof(RemainingTimeText));
            return TimerAdvanceResult.Updated;
        }

        if (elapsedSeconds >= _remainingSeconds)
        {
            _remainingSeconds = 0;
            _timerStartedAt = null;
            OnPropertyChanged(nameof(TimerStartedAt));
            OnPropertyChanged(nameof(IsTimerRunning));
            NotifyRemainingTimeChanged();
            return TimerAdvanceResult.Elapsed;
        }

        // RemainingSeconds is capped at eight hours, so this conversion is safe
        // after the elapsed-vs-remaining guard above.
        var elapsedWholeSeconds = (int)Math.Floor(elapsedSeconds);
        _remainingSeconds -= elapsedWholeSeconds;
        _timerStartedAt = _timerStartedAt.Value.AddSeconds(elapsedWholeSeconds);
        OnPropertyChanged(nameof(TimerStartedAt));
        NotifyRemainingTimeChanged();
        return TimerAdvanceResult.Updated;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyRemainingTimeChanged()
    {
        OnPropertyChanged(nameof(RemainingSeconds));
        OnPropertyChanged(nameof(RemainingTimeText));
        OnPropertyChanged(nameof(IsTimerExpired));
        OnPropertyChanged(nameof(CanStartOvertime));
        OnPropertyChanged(nameof(TimerProgressPercent));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

internal enum TimerAdvanceResult
{
    None,
    Updated,
    Elapsed,
    ClockAdjusted
}
