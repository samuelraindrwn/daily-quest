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
    internal const string RingtoneResourceName =
        "DailyQuest.Assets.Ringtone.FacilityAlarm.wav";
    internal static readonly TimeSpan MaximumAlarmDuration = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan FallbackAlarmInterval = TimeSpan.FromMilliseconds(850);
    // Windows may keep an accessibility-extended balloon visible well beyond the
    // requested display duration, so keep the tray host alive until Windows closes
    // it. The timer is only a safety fallback for shells that omit that event.
    private static readonly TimeSpan TrayIconLifetime = TimeSpan.FromMinutes(10);

    private readonly Dispatcher _dispatcher;
    private Forms.NotifyIcon? _notifyIcon;
    private Icon? _applicationIcon;
    private DispatcherTimer? _hideTrayIconTimer;
    private DispatcherTimer? _alarmStopTimer;
    private DispatcherTimer? _fallbackAlarmTimer;
    private MemoryStream? _alarmWaveStream;
    private SoundPlayer? _alarmPlayer;
    private int _fallbackPulseIndex;
    private bool _disposed;

    public WindowsQuestAlarmService()
    {
        _dispatcher = System.Windows.Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        RunOnDispatcher(Initialize);
    }

    public void NotifyTimerCompleted(string questText)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var notificationText = CreateNotificationText(questText);
        RunOnDispatcher(() =>
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // A newly completed timer owns the alarm channel. This prevents an
            // earlier alarm from continuing behind the new alert.
            StopTimerAlarmCore(hideTrayIcon: true);
            StartAlarmCore();

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

        _alarmStopTimer = new DispatcherTimer(DispatcherPriority.Background, _dispatcher)
        {
            Interval = MaximumAlarmDuration,
        };
        _alarmStopTimer.Tick += AlarmStopTimer_Tick;

        _fallbackAlarmTimer = new DispatcherTimer(DispatcherPriority.Background, _dispatcher)
        {
            Interval = FallbackAlarmInterval,
        };
        _fallbackAlarmTimer.Tick += FallbackAlarmTimer_Tick;

        TryInitializeAlarmPlayer();
    }

    private void AlarmStopTimer_Tick(object? sender, EventArgs e)
    {
        _alarmStopTimer?.Stop();
        StopAlarmPlaybackCore();
    }

    private void FallbackAlarmTimer_Tick(object? sender, EventArgs e)
    {
        PlayFallbackAlarmPulse();
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

    private void StartAlarmCore()
    {
        var startedLoop = false;
        if (_alarmPlayer is not null)
        {
            try
            {
                _alarmPlayer.PlayLooping();
                startedLoop = true;
            }
            catch (Exception exception) when (
                exception is InvalidOperationException or TimeoutException)
            {
                // Fall through to Windows system sounds when wave playback is
                // unavailable. A timer alert must never take down the checklist.
            }
        }

        if (!startedLoop)
        {
            _fallbackPulseIndex = 0;
            PlayFallbackAlarmPulse();
            _fallbackAlarmTimer!.Start();
        }

        _alarmStopTimer!.Stop();
        _alarmStopTimer.Interval = MaximumAlarmDuration;
        _alarmStopTimer.Start();
    }

    private void PlayFallbackAlarmPulse()
    {
        var sound = _fallbackPulseIndex++ % 2 == 0
            ? SystemSounds.Exclamation
            : SystemSounds.Asterisk;
        sound.Play();
    }

    private void TryInitializeAlarmPlayer()
    {
        try
        {
            _alarmWaveStream = new MemoryStream(LoadAlarmWave(), writable: false);
            _alarmPlayer = new SoundPlayer(_alarmWaveStream);
            _alarmPlayer.Load();
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or TimeoutException)
        {
            _alarmPlayer?.Dispose();
            _alarmPlayer = null;
            _alarmWaveStream?.Dispose();
            _alarmWaveStream = null;
        }
    }

    private void StopTimerAlarmCore(bool hideTrayIcon)
    {
        _alarmStopTimer?.Stop();
        StopAlarmPlaybackCore();

        if (hideTrayIcon)
        {
            HideTrayIcon();
        }
    }

    private void StopAlarmPlaybackCore()
    {
        _fallbackAlarmTimer?.Stop();
        _fallbackPulseIndex = 0;

        try
        {
            _alarmPlayer?.Stop();
        }
        catch (InvalidOperationException)
        {
            // Playback may already have ended because the audio device changed.
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

        if (_alarmStopTimer is not null)
        {
            _alarmStopTimer.Tick -= AlarmStopTimer_Tick;
            _alarmStopTimer = null;
        }

        if (_fallbackAlarmTimer is not null)
        {
            _fallbackAlarmTimer.Stop();
            _fallbackAlarmTimer.Tick -= FallbackAlarmTimer_Tick;
            _fallbackAlarmTimer = null;
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

        _alarmPlayer?.Dispose();
        _alarmPlayer = null;
        _alarmWaveStream?.Dispose();
        _alarmWaveStream = null;
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

    internal static bool HasEmbeddedRingtone =>
        typeof(WindowsQuestAlarmService).Assembly
            .GetManifestResourceInfo(RingtoneResourceName) is not null;

    internal static byte[] LoadAlarmWave()
    {
        using var resourceStream = typeof(WindowsQuestAlarmService).Assembly
            .GetManifestResourceStream(RingtoneResourceName);
        if (resourceStream is null)
        {
            throw new InvalidOperationException(
                $"Embedded timer ringtone '{RingtoneResourceName}' was not found.");
        }

        using var buffer = resourceStream.CanSeek && resourceStream.Length <= int.MaxValue
            ? new MemoryStream((int)resourceStream.Length)
            : new MemoryStream();
        resourceStream.CopyTo(buffer);
        return buffer.ToArray();
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
