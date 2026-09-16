using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace DailyQuest.Services;

public sealed class WindowsQuestAlarmService : IQuestAlarmService, IDisposable
{
    private const int BalloonDisplayMilliseconds = 8_000;
    private const int MaxBalloonTextLength = 255;
    private static readonly TimeSpan AlarmPulseInterval = TimeSpan.FromMilliseconds(700);
    private static readonly TimeSpan RepeatAlarmInterval = TimeSpan.FromSeconds(10);
    // Windows may keep an accessibility-extended balloon visible well beyond the
    // requested display duration, so keep the tray host alive until Windows closes
    // it. The timer is only a safety fallback for shells that omit that event.
    private static readonly TimeSpan TrayIconLifetime = TimeSpan.FromMinutes(10);

    private readonly Dispatcher _dispatcher;
    private Forms.NotifyIcon? _notifyIcon;
    private Icon? _applicationIcon;
    private DispatcherTimer? _hideTrayIconTimer;
    private DispatcherTimer? _alarmPulseTimer;
    private int _remainingAlarmPulses;
    private bool _repeatUntilStopped;
    private bool _disposed;

    public WindowsQuestAlarmService()
    {
        _dispatcher = System.Windows.Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        RunOnDispatcher(Initialize);
    }

    public void NotifyTimerCompleted(string questText, bool repeatUntilStopped)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var notificationText = CreateNotificationText(questText);
        RunOnDispatcher(() =>
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // A newly completed timer owns the alarm channel. This prevents an
            // earlier repeating alarm from continuing behind the new alert.
            StopTimerAlarmCore(hideTrayIcon: true);
            StartAlarmCore(repeatUntilStopped);

            _notifyIcon!.BalloonTipTitle = "Daily Quest";
            _notifyIcon.BalloonTipText = notificationText;
            _notifyIcon.BalloonTipIcon = Forms.ToolTipIcon.Info;
            _notifyIcon.Visible = true;
            _notifyIcon.ShowBalloonTip(BalloonDisplayMilliseconds);

            _hideTrayIconTimer!.Stop();
            _hideTrayIconTimer.Start();
        });
    }

    public void StopTimerAlarm()
    {
        if (_disposed)
        {
            return;
        }

        RunOnDispatcher(() =>
        {
            if (!_disposed)
            {
                StopTimerAlarmCore(hideTrayIcon: true);
            }
        });
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        RunOnDispatcher(DisposeCore);
        GC.SuppressFinalize(this);
    }

    private void Initialize()
    {
        _applicationIcon = LoadApplicationIcon();
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = _applicationIcon,
            Text = "Daily Quest",
            Visible = false,
        };
        _notifyIcon.BalloonTipClosed += NotifyIcon_BalloonTipClosed;
        _notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;

        _hideTrayIconTimer = new DispatcherTimer(DispatcherPriority.Background, _dispatcher)
        {
            Interval = TrayIconLifetime,
        };
        _hideTrayIconTimer.Tick += HideTrayIconTimer_Tick;

        _alarmPulseTimer = new DispatcherTimer(DispatcherPriority.Background, _dispatcher)
        {
            Interval = AlarmPulseInterval,
        };
        _alarmPulseTimer.Tick += AlarmPulseTimer_Tick;
    }

    private void AlarmPulseTimer_Tick(object? sender, EventArgs e)
    {
        if (_remainingAlarmPulses > 0)
        {
            SystemSounds.Exclamation.Play();
            _remainingAlarmPulses--;

            if (_remainingAlarmPulses == 0)
            {
                if (_repeatUntilStopped)
                {
                    _alarmPulseTimer!.Interval = RepeatAlarmInterval;
                }
                else
                {
                    _alarmPulseTimer!.Stop();
                }
            }

            return;
        }

        if (!_repeatUntilStopped)
        {
            _alarmPulseTimer!.Stop();
            return;
        }

        // Repeating alarms use the same short three-pulse pattern as a regular
        // alert, with a quiet interval between bursts to avoid constant noise.
        PlayInitialAlarmPulse();
    }

    private void HideTrayIconTimer_Tick(object? sender, EventArgs e)
    {
        HideTrayIcon();
    }

    private void NotifyIcon_BalloonTipClosed(object? sender, EventArgs e)
    {
        HideTrayIcon();
    }

    private void NotifyIcon_BalloonTipClicked(object? sender, EventArgs e)
    {
        HideTrayIcon();
    }

    private void HideTrayIcon()
    {
        _hideTrayIconTimer?.Stop();
        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
        }
    }

    private void StartAlarmCore(bool repeatUntilStopped)
    {
        _repeatUntilStopped = repeatUntilStopped;
        PlayInitialAlarmPulse();
    }

    private void PlayInitialAlarmPulse()
    {
        SystemSounds.Exclamation.Play();
        _remainingAlarmPulses = 2;
        _alarmPulseTimer!.Stop();
        _alarmPulseTimer.Interval = AlarmPulseInterval;
        _alarmPulseTimer.Start();
    }

    private void StopTimerAlarmCore(bool hideTrayIcon)
    {
        _alarmPulseTimer?.Stop();
        _remainingAlarmPulses = 0;
        _repeatUntilStopped = false;

        if (hideTrayIcon)
        {
            HideTrayIcon();
        }
    }

    private void DisposeCore()
    {
        if (_disposed)
        {
            return;
        }

        if (_hideTrayIconTimer is not null)
        {
            _hideTrayIconTimer.Stop();
            _hideTrayIconTimer.Tick -= HideTrayIconTimer_Tick;
            _hideTrayIconTimer = null;
        }

        StopTimerAlarmCore(hideTrayIcon: true);

        if (_alarmPulseTimer is not null)
        {
            _alarmPulseTimer.Tick -= AlarmPulseTimer_Tick;
            _alarmPulseTimer = null;
        }

        if (_notifyIcon is not null)
        {
            _notifyIcon.BalloonTipClosed -= NotifyIcon_BalloonTipClosed;
            _notifyIcon.BalloonTipClicked -= NotifyIcon_BalloonTipClicked;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        _applicationIcon?.Dispose();
        _applicationIcon = null;
        _disposed = true;
    }

    private void RunOnDispatcher(Action action)
    {
        if (_dispatcher.CheckAccess())
        {
            action();
            return;
        }

        _dispatcher.Invoke(action);
    }

    private static string CreateNotificationText(string questText)
    {
        const string prefix = "Timer  |  ";
        const string truncationMarker = "...";

        var normalizedQuestText = string.IsNullOrWhiteSpace(questText)
            ? "Daily Quest"
            : questText.Trim();
        var maximumQuestTextLength = MaxBalloonTextLength
            - prefix.Length
            - truncationMarker.Length;

        if (normalizedQuestText.Length > maximumQuestTextLength)
        {
            normalizedQuestText = $"{normalizedQuestText[..maximumQuestTextLength]}{truncationMarker}";
        }

        return $"{prefix}{normalizedQuestText}";
    }

    private static Icon LoadApplicationIcon()
    {
        try
        {
            var executablePath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(executablePath) && File.Exists(executablePath))
            {
                using var associatedIcon = Icon.ExtractAssociatedIcon(executablePath);
                if (associatedIcon is not null)
                {
                    return (Icon)associatedIcon.Clone();
                }
            }
        }
        catch (Exception exception) when (exception is ArgumentException or ExternalException)
        {
            // A system icon is a safe fallback if Windows cannot read the executable icon.
        }

        return (Icon)SystemIcons.Application.Clone();
    }
}
