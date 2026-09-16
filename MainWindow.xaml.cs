using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DailyQuest.ViewModels;

namespace DailyQuest;

public partial class MainWindow : Window
{
    private const double ScreenEdgeGap = 24;

    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _dayChangeTimer;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        _dayChangeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _dayChangeTimer.Tick += (_, _) => _viewModel.RollOverToCurrentDay();
        _dayChangeTimer.Start();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var saved = _viewModel.SavedWindow;
        var workArea = SystemParameters.WorkArea;
        var startupMaxWidth = Math.Max(
            MinWidth,
            Math.Min(MaxWidth, workArea.Width - (ScreenEdgeGap * 2)));
        var startupMaxHeight = Math.Max(
            MinHeight,
            Math.Min(MaxHeight, workArea.Height - (ScreenEdgeGap * 2)));

        Width = Math.Clamp(saved.Width, MinWidth, startupMaxWidth);
        Height = Math.Clamp(saved.Height, MinHeight, startupMaxHeight);
        Left = Math.Max(
            workArea.Left + ScreenEdgeGap,
            workArea.Right - Width - ScreenEdgeGap);
        Top = workArea.Top + ScreenEdgeGap;

        NewItemTextBox.Focus();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
            // The mouse may have been released before DragMove starts.
        }
    }

    private void NewItemTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _viewModel.AddItemCommand.CanExecute(null))
        {
            _viewModel.AddItemCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            _viewModel.NewItemText = string.Empty;
            e.Handled = true;
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        _dayChangeTimer.Stop();
        _viewModel.SaveWindowSize(ActualWidth, ActualHeight);
    }
}
