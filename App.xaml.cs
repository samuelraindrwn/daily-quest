using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using DailyQuest.Services;
using DailyQuest.ViewModels;

namespace DailyQuest;

public partial class App : Application
{
    // Kept stable so the pre-rename Morning Check-in build cannot write state concurrently.
    private const string SingleInstanceMutexName = @"Local\MorningCheckIn.SingleInstance.8F915C25";
    private const string ActivationEventName = @"Local\MorningCheckIn.Activate.8F915C25";
    private const string ExitEventName = @"Local\MorningCheckIn.Exit.8F915C25";
    private const int RestoreWindowCommand = 9;

    private Mutex? _singleInstanceMutex;
    private bool _ownsSingleInstanceMutex;
    private EventWaitHandle? _activationEvent;
    private RegisteredWaitHandle? _activationRegistration;
    private EventWaitHandle? _exitEvent;
    private RegisteredWaitHandle? _exitRegistration;
    private WindowsQuestAlarmService? _questAlarmService;
    private MainViewModel? _viewModel;
    private MainWindow? _mainWindow;
    private bool _isExiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        var exitRequested = e.Args.Any(argument =>
            string.Equals(argument, "--exit", StringComparison.OrdinalIgnoreCase));
        _activationEvent = new EventWaitHandle(
            initialState: false,
            EventResetMode.AutoReset,
            ActivationEventName);
        _exitEvent = new EventWaitHandle(
            initialState: false,
            EventResetMode.AutoReset,
            ExitEventName);
        _singleInstanceMutex = new Mutex(
            initiallyOwned: true,
            SingleInstanceMutexName,
            out var createdNew);

        if (!createdNew)
        {
            if (exitRequested)
            {
                _exitEvent.Set();
            }
            else
            {
                _activationEvent.Set();
                ActivateExistingWindowFallback();
            }

            _activationEvent.Dispose();
            _activationEvent = null;
            _exitEvent.Dispose();
            _exitEvent = null;
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
            Shutdown();
            return;
        }

        _ownsSingleInstanceMutex = true;
        if (exitRequested)
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);

        var themeService = new WpfThemeService(this);
        var runAtStartupService = new WindowsRunAtStartupService();
        _questAlarmService = new WindowsQuestAlarmService();
        _viewModel = new MainViewModel(
            themeService: themeService,
            alarmService: _questAlarmService,
            runAtStartupService: runAtStartupService);
        _mainWindow = new MainWindow(_viewModel);

        _questAlarmService.OpenRequested += QuestAlarmService_OpenRequested;
        _questAlarmService.ExitRequested += QuestAlarmService_ExitRequested;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        UpdateTrayMenuText();

        MainWindow = _mainWindow;
        _mainWindow.Show();

        _activationRegistration = ThreadPool.RegisterWaitForSingleObject(
            _activationEvent,
            ActivationEventSignaled,
            state: null,
            Timeout.InfiniteTimeSpan,
            executeOnlyOnce: false);
        _exitRegistration = ThreadPool.RegisterWaitForSingleObject(
            _exitEvent,
            ExitEventSignaled,
            state: null,
            Timeout.InfiniteTimeSpan,
            executeOnlyOnce: false);
    }

    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
        _isExiting = true;
        _mainWindow?.PrepareForApplicationExit();
        base.OnSessionEnding(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _isExiting = true;
        _mainWindow?.PrepareForApplicationExit();

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        if (_questAlarmService is not null)
        {
            _questAlarmService.OpenRequested -= QuestAlarmService_OpenRequested;
            _questAlarmService.ExitRequested -= QuestAlarmService_ExitRequested;
        }

        _activationRegistration?.Unregister(null);
        _activationRegistration = null;
        _activationEvent?.Dispose();
        _activationEvent = null;
        _exitRegistration?.Unregister(null);
        _exitRegistration = null;
        _exitEvent?.Dispose();
        _exitEvent = null;

        _questAlarmService?.Dispose();
        _questAlarmService = null;
        _viewModel = null;
        _mainWindow = null;

        if (_ownsSingleInstanceMutex)
        {
            _singleInstanceMutex?.ReleaseMutex();
        }

        _singleInstanceMutex?.Dispose();
        _singleInstanceMutex = null;
        _ownsSingleInstanceMutex = false;
        base.OnExit(e);
    }

    private void ActivationEventSignaled(object? state, bool timedOut)
    {
        if (!timedOut)
        {
            RunOnApplicationDispatcher(RestoreMainWindow);
        }
    }

    private void ExitEventSignaled(object? state, bool timedOut)
    {
        if (!timedOut)
        {
            RunOnApplicationDispatcher(ExitApplication);
        }
    }

    // v1.8+ listens to the named activation event so a hidden tray window can
    // restore itself. Keep the Win32 path while upgrading from v1.7, whose
    // running process only understands foreground-window activation.
    private static void ActivateExistingWindowFallback()
    {
        var currentProcessId = Environment.ProcessId;
        var processNames = new[] { "DailyQuest", "MorningCheckIn" };
        foreach (var process in processNames.SelectMany(Process.GetProcessesByName))
        {
            using (process)
            {
                if (process.Id == currentProcessId || process.MainWindowHandle == IntPtr.Zero)
                {
                    continue;
                }

                ShowWindowAsync(process.MainWindowHandle, RestoreWindowCommand);
                SetForegroundWindow(process.MainWindowHandle);
                return;
            }
        }
    }

    private void QuestAlarmService_OpenRequested(object? sender, EventArgs e) =>
        RunOnApplicationDispatcher(RestoreMainWindow);

    private void QuestAlarmService_ExitRequested(object? sender, EventArgs e) =>
        RunOnApplicationDispatcher(ExitApplication);

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.Copy) or nameof(MainViewModel.LanguageCode))
        {
            UpdateTrayMenuText();
        }
    }

    private void UpdateTrayMenuText()
    {
        if (_viewModel is not null && _questAlarmService is not null)
        {
            _questAlarmService.SetTrayMenuText(
                _viewModel.Copy.TrayOpen,
                _viewModel.Copy.TrayExit);
        }
    }

    private void RestoreMainWindow()
    {
        if (!_isExiting)
        {
            _mainWindow?.RestoreFromTray();
        }
    }

    private void ExitApplication()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;
        _mainWindow?.PrepareForApplicationExit();
        Shutdown();
    }

    private void RunOnApplicationDispatcher(Action action)
    {
        if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
        {
            return;
        }

        if (Dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.BeginInvoke(action);
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindowAsync(IntPtr windowHandle, int command);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);
}
