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
    private const int RestoreWindowCommand = 9;
    private Mutex? _singleInstanceMutex;
    private bool _ownsSingleInstanceMutex;
    private WindowsQuestAlarmService? _questAlarmService;

    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstanceMutex = new Mutex(
            initiallyOwned: true,
            SingleInstanceMutexName,
            out var createdNew);

        if (!createdNew)
        {
            ActivateExistingWindow();
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
            Shutdown();
            return;
        }

        _ownsSingleInstanceMutex = true;
        base.OnStartup(e);

        var themeService = new WpfThemeService(this);
        var runAtStartupService = new WindowsRunAtStartupService();
        _questAlarmService = new WindowsQuestAlarmService();
        var viewModel = new MainViewModel(
            themeService: themeService,
            alarmService: _questAlarmService,
            runAtStartupService: runAtStartupService);
        var mainWindow = new MainWindow(viewModel);
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _questAlarmService?.Dispose();
        _questAlarmService = null;

        if (_ownsSingleInstanceMutex)
        {
            _singleInstanceMutex?.ReleaseMutex();
        }

        _singleInstanceMutex?.Dispose();
        _singleInstanceMutex = null;
        _ownsSingleInstanceMutex = false;
        base.OnExit(e);
    }

    private static void ActivateExistingWindow()
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

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindowAsync(IntPtr windowHandle, int command);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);
}
