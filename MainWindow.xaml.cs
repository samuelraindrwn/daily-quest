using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DailyQuest.ViewModels;

namespace DailyQuest;

public partial class MainWindow : Window
{
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
        Width = Math.Clamp(saved.Width, MinWidth, MaxWidth);
        Height = Math.Clamp(saved.Height, MinHeight, MaxHeight);

        if (saved.Left.HasValue &&
            saved.Top.HasValue &&
            IsPositionVisible(saved.Left.Value, saved.Top.Value, Width, Height))
        {
            Left = saved.Left.Value;
            Top = saved.Top.Value;
        }
        else
        {
            var workArea = SystemParameters.WorkArea;
            Left = Math.Max(workArea.Left + 16, workArea.Right - Width - 24);
            Top = workArea.Top + 24;
        }

        NewItemTextBox.Focus();
    }

    private static bool IsPositionVisible(double left, double top, double width, double height)
    {
        const double minimumVisibleSize = 80;

        var right = left + width;
        var bottom = top + height;
        var virtualLeft = SystemParameters.VirtualScreenLeft;
        var virtualTop = SystemParameters.VirtualScreenTop;
        var virtualRight = virtualLeft + SystemParameters.VirtualScreenWidth;
        var virtualBottom = virtualTop + SystemParameters.VirtualScreenHeight;

        var visibleWidth = Math.Min(right, virtualRight) - Math.Max(left, virtualLeft);
        var visibleHeight = Math.Min(bottom, virtualBottom) - Math.Max(top, virtualTop);
        return visibleWidth >= minimumVisibleSize && visibleHeight >= minimumVisibleSize;
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
        _viewModel.SaveWindowState(Left, Top, ActualWidth, ActualHeight);
    }
}
