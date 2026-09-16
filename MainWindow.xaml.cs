using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DailyQuest.Models;
using DailyQuest.ViewModels;

namespace DailyQuest;

public partial class MainWindow : Window
{
    private const double ScreenEdgeGap = 24;
    private const double ExpandedMinWidth = 430;
    private const double ExpandedMinHeight = 480;
    private const double ExpandedMaxWidth = 760;
    private const double ExpandedMaxHeight = 1000;
    private const double CompactWidth = 300;
    private const double CompactHeight = 76;
    private const string QuestDragFormat = "DailyQuest.ChecklistItem";
    private const string QnaUrl =
        "https://github.com/samuelraindrwn/daily-quest/blob/main/docs/FAQ.md";
    private const string BugReportUrl =
        "https://github.com/samuelraindrwn/daily-quest/issues/new?template=bug_report.yml";

    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _dayChangeTimer;
    private bool _isCompact;
    private double _expandedWidth = 430;
    private double _expandedHeight = 610;
    private Point _dragStart;
    private ChecklistItem? _dragCandidate;
    private Border? _dropTarget;

    public MainWindow()
        : this(new MainViewModel())
    {
    }

    public MainWindow(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();

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
            ExpandedMinWidth,
            Math.Min(ExpandedMaxWidth, workArea.Width - (ScreenEdgeGap * 2)));
        var startupMaxHeight = Math.Max(
            ExpandedMinHeight,
            Math.Min(ExpandedMaxHeight, workArea.Height - (ScreenEdgeGap * 2)));

        Width = Math.Clamp(saved.Width, ExpandedMinWidth, startupMaxWidth);
        Height = Math.Clamp(saved.Height, ExpandedMinHeight, startupMaxHeight);
        _expandedWidth = Width;
        _expandedHeight = Height;
        PositionAtTopRight();

        NewItemTextBox.Focus();
    }

    private void PositionAtTopRight()
    {
        var workArea = SystemParameters.WorkArea;
        Left = Math.Max(
            workArea.Left + ScreenEdgeGap,
            workArea.Right - Width - ScreenEdgeGap);
        Top = workArea.Top + ScreenEdgeGap;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed ||
            FindVisualParent<ButtonBase>(e.OriginalSource as DependencyObject) is not null)
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

    private void ScheduleOption_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ScheduleOptionViewModel option } &&
            _viewModel.SetScheduleOffsetCommand.CanExecute(option.Offset))
        {
            _viewModel.SetScheduleOffsetCommand.Execute(option.Offset);
        }

        SchedulePopup.IsOpen = false;
        NewItemTextBox.Focus();
    }

    private void Compact_Click(object sender, RoutedEventArgs e) => SetCompactMode(true);

    private void Expand_Click(object sender, RoutedEventArgs e) => SetCompactMode(false);

    private void SetCompactMode(bool compact)
    {
        if (_isCompact == compact)
        {
            return;
        }

        if (compact)
        {
            _expandedWidth = ActualWidth > 0 ? ActualWidth : Width;
            _expandedHeight = ActualHeight > 0 ? ActualHeight : Height;
        }

        var workArea = SystemParameters.WorkArea;
        var expandedMaxWidth = Math.Max(
            ExpandedMinWidth,
            Math.Min(ExpandedMaxWidth, workArea.Width - (ScreenEdgeGap * 2)));
        var expandedMaxHeight = Math.Max(
            ExpandedMinHeight,
            Math.Min(ExpandedMaxHeight, workArea.Height - (ScreenEdgeGap * 2)));
        var targetWidth = compact
            ? CompactWidth
            : Math.Clamp(_expandedWidth, ExpandedMinWidth, expandedMaxWidth);
        var targetHeight = compact
            ? CompactHeight
            : Math.Clamp(_expandedHeight, ExpandedMinHeight, expandedMaxHeight);
        var targetLeft = Math.Max(
            workArea.Left + ScreenEdgeGap,
            workArea.Right - targetWidth - ScreenEdgeGap);
        var targetTop = workArea.Top + ScreenEdgeGap;

        PrepareWindowForModeChange();
        ApplyCompactState(compact, targetWidth, targetHeight, targetLeft, targetTop);
    }

    private void PrepareWindowForModeChange()
    {
        MinWidth = 0;
        MinHeight = 0;
        MaxWidth = double.PositiveInfinity;
        MaxHeight = double.PositiveInfinity;
        ResizeMode = ResizeMode.NoResize;
    }

    private void ApplyCompactState(
        bool compact,
        double targetWidth,
        double targetHeight,
        double targetLeft,
        double targetTop)
    {
        Width = targetWidth;
        Height = targetHeight;
        Left = targetLeft;
        Top = targetTop;
        ExpandedWidget.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        CompactWidget.Visibility = compact ? Visibility.Visible : Visibility.Collapsed;
        ExpandedWidget.IsHitTestVisible = !compact;
        CompactWidget.IsHitTestVisible = compact;
        FinishCompactState(compact);
    }

    private void FinishCompactState(bool compact)
    {
        _isCompact = compact;

        if (compact)
        {
            MinWidth = CompactWidth;
            MaxWidth = CompactWidth;
            MinHeight = CompactHeight;
            MaxHeight = CompactHeight;
            Width = CompactWidth;
            Height = CompactHeight;
            ResizeMode = ResizeMode.NoResize;
        }
        else
        {
            MinWidth = ExpandedMinWidth;
            MinHeight = ExpandedMinHeight;
            MaxWidth = ExpandedMaxWidth;
            MaxHeight = ExpandedMaxHeight;
            ResizeMode = ResizeMode.CanResize;
            NewItemTextBox.Focus();
        }
    }

    private void DragHandle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not ChecklistItem item)
        {
            return;
        }

        _dragStart = e.GetPosition(this);
        _dragCandidate = item;
        Mouse.Capture(element);
        e.Handled = true;
    }

    private void DragHandle_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragCandidate is null || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var current = e.GetPosition(this);
        if (Math.Abs(current.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(current.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        var source = _dragCandidate;
        _dragCandidate = null;
        Mouse.Capture(null);

        var data = new DataObject();
        data.SetData(QuestDragFormat, source);
        try
        {
            DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);
        }
        finally
        {
            ClearDropIndicators();
        }

        e.Handled = true;
    }

    private void DragHandle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _dragCandidate = null;
        Mouse.Capture(null);
        e.Handled = true;
    }

    private void QuestCard_DragOver(object sender, DragEventArgs e)
    {
        if (sender is not Border card ||
            card.DataContext is not ChecklistItem target ||
            e.Data.GetData(QuestDragFormat) is not ChecklistItem source ||
            !_viewModel.Items.Contains(source) ||
            !_viewModel.Items.Contains(target))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var dropAfter = e.GetPosition(card).Y > card.ActualHeight / 2;
        ShowDropIndicator(card, dropAfter);
        AutoScrollTaskList(e);
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void QuestCard_DragLeave(object sender, DragEventArgs e)
    {
        if (sender is Border card && ReferenceEquals(card, _dropTarget))
        {
            ClearDropIndicators();
        }
    }

    private void QuestCard_Drop(object sender, DragEventArgs e)
    {
        try
        {
            if (sender is not Border card ||
                card.DataContext is not ChecklistItem target ||
                e.Data.GetData(QuestDragFormat) is not ChecklistItem source)
            {
                return;
            }

            var sourceIndex = _viewModel.Items.IndexOf(source);
            var targetIndex = _viewModel.Items.IndexOf(target);
            if (sourceIndex < 0 || targetIndex < 0)
            {
                return;
            }

            var dropAfter = e.GetPosition(card).Y > card.ActualHeight / 2;
            var destinationIndex = targetIndex + (dropAfter ? 1 : 0);
            if (sourceIndex < destinationIndex)
            {
                destinationIndex--;
            }

            _viewModel.MoveItem(source, destinationIndex);
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }
        finally
        {
            ClearDropIndicators();
        }
    }

    private void ShowDropIndicator(Border card, bool after)
    {
        if (!ReferenceEquals(_dropTarget, card))
        {
            ClearDropIndicators();
            _dropTarget = card;
        }

        var beforeIndicator = FindVisualChildByName<Border>(card, "DropBeforeIndicator");
        var afterIndicator = FindVisualChildByName<Border>(card, "DropAfterIndicator");
        if (beforeIndicator is not null)
        {
            beforeIndicator.Visibility = after ? Visibility.Collapsed : Visibility.Visible;
        }

        if (afterIndicator is not null)
        {
            afterIndicator.Visibility = after ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ClearDropIndicators()
    {
        if (_dropTarget is not null)
        {
            var beforeIndicator = FindVisualChildByName<Border>(_dropTarget, "DropBeforeIndicator");
            var afterIndicator = FindVisualChildByName<Border>(_dropTarget, "DropAfterIndicator");
            if (beforeIndicator is not null)
            {
                beforeIndicator.Visibility = Visibility.Collapsed;
            }

            if (afterIndicator is not null)
            {
                afterIndicator.Visibility = Visibility.Collapsed;
            }
        }

        _dropTarget = null;
    }

    private void AutoScrollTaskList(DragEventArgs e)
    {
        var position = e.GetPosition(TaskScrollViewer);
        const double edge = 34;
        const double step = 18;

        if (position.Y < edge)
        {
            TaskScrollViewer.ScrollToVerticalOffset(
                Math.Max(0, TaskScrollViewer.VerticalOffset - step));
        }
        else if (position.Y > TaskScrollViewer.ViewportHeight - edge)
        {
            TaskScrollViewer.ScrollToVerticalOffset(
                Math.Min(TaskScrollViewer.ScrollableHeight, TaskScrollViewer.VerticalOffset + step));
        }
    }

    private void ClearHistory_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.ClearHistoryCommand.CanExecute(null))
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            _viewModel.Copy.ClearHistoryConfirmMessage,
            _viewModel.Copy.ClearHistoryConfirmTitle,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (result == MessageBoxResult.Yes)
        {
            _viewModel.ClearHistoryCommand.Execute(null);
        }
    }

    private void OpenQna_Click(object sender, RoutedEventArgs e) => OpenExternalUrl(QnaUrl);

    private void OpenBugReport_Click(object sender, RoutedEventArgs e) => OpenExternalUrl(BugReportUrl);

    private static void OpenExternalUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or Win32Exception)
        {
            // Leave the application usable if Windows has no browser association.
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        _dayChangeTimer.Stop();
        _viewModel.SaveWindowSize(
            _isCompact ? _expandedWidth : ActualWidth,
            _isCompact ? _expandedHeight : ActualHeight);
    }

    private static T? FindVisualParent<T>(DependencyObject? child)
        where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T match)
            {
                return match;
            }

            child = VisualTreeHelper.GetParent(child);
        }

        return null;
    }

    private static T? FindVisualChildByName<T>(DependencyObject parent, string name)
        where T : FrameworkElement
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match && string.Equals(match.Name, name, StringComparison.Ordinal))
            {
                return match;
            }

            var descendant = FindVisualChildByName<T>(child, name);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
}
