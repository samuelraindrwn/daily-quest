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
    private readonly Dispatcher _dispatcher;
    private Forms.NotifyIcon? _notifyIcon;
    private Forms.ContextMenuStrip? _trayMenu;
    private Forms.ToolStripMenuItem? _openMenuItem;
    private Forms.ToolStripMenuItem? _exitMenuItem;
    private Icon? _applicationIcon;
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

    public event EventHandler? OpenRequested;

    public event EventHandler? ExitRequested;

    public void SetTrayMenuText(string openText, string exitText)
    {
        if (_disposed)
        {
            return;
        }

        RunOnDispatcher(() =>
        {
            if (_disposed)
            {
                return;
            }

            if (_openMenuItem is not null)
            {
                _openMenuItem.Text = string.IsNullOrWhiteSpace(openText)
                    ? "Open Daily Quest"
                    : openText.Trim();
            }

            if (_exitMenuItem is not null)
            {
                _exitMenuItem.Text = string.IsNullOrWhiteSpace(exitText)
                    ? "Exit"
                    : exitText.Trim();
            }
        });
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
            StopTimerAlarmCore();
            StartAlarmCore();

            _notifyIcon!.BalloonTipTitle = "Daily Quest";
            _notifyIcon.BalloonTipText = notificationText;
            _notifyIcon.BalloonTipIcon = Forms.ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(BalloonDisplayMilliseconds);
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
                StopTimerAlarmCore();
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
        _openMenuItem = new Forms.ToolStripMenuItem("Open Daily Quest");
        _exitMenuItem = new Forms.ToolStripMenuItem("Exit");
        _openMenuItem.Click += OpenMenuItem_Click;
        _exitMenuItem.Click += ExitMenuItem_Click;
        _trayMenu = new Forms.ContextMenuStrip();
        _trayMenu.Items.Add(_openMenuItem);
        _trayMenu.Items.Add(new Forms.ToolStripSeparator());
        _trayMenu.Items.Add(_exitMenuItem);

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = _applicationIcon,
            Text = "Daily Quest",
            ContextMenuStrip = _trayMenu,
            Visible = true,
        };
        _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        _notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;

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

    private void NotifyIcon_BalloonTipClicked(object? sender, EventArgs e)
    {
        OpenRequested?.Invoke(this, EventArgs.Empty);
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e) =>
        OpenRequested?.Invoke(this, EventArgs.Empty);

    private void OpenMenuItem_Click(object? sender, EventArgs e) =>
        OpenRequested?.Invoke(this, EventArgs.Empty);

    private void ExitMenuItem_Click(object? sender, EventArgs e) =>
        ExitRequested?.Invoke(this, EventArgs.Empty);

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

    private void StopTimerAlarmCore()
    {
        _alarmStopTimer?.Stop();
        StopAlarmPlaybackCore();
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

        StopTimerAlarmCore();

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
            _notifyIcon.DoubleClick -= NotifyIcon_DoubleClick;
            _notifyIcon.BalloonTipClicked -= NotifyIcon_BalloonTipClicked;
            _notifyIcon.ContextMenuStrip = null;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        if (_openMenuItem is not null)
        {
            _openMenuItem.Click -= OpenMenuItem_Click;
            _openMenuItem = null;
        }

        if (_exitMenuItem is not null)
        {
            _exitMenuItem.Click -= ExitMenuItem_Click;
            _exitMenuItem = null;
        }

        _trayMenu?.Dispose();
        _trayMenu = null;

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
