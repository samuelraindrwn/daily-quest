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
using Forms = System.Windows.Forms;

namespace DailyQuest;

public partial class MainWindow : Window
{
    private const double ScreenEdgeGap = 24;
    private const double ExpandedDefaultWidth = 520;
    private const double ExpandedDefaultHeight = 680;
    private const double ExpandedMinWidth = 390;
    private const double ExpandedMinHeight = 500;
    private const double ExpandedMaxWidth = 1200;
    private const double ExpandedMaxHeight = 1200;
    private const double CompactWidth = 300;
    private const double CompactHeight = 76;
    private const string QuestDragFormat = "DailyQuest.ChecklistItem";
    private const string LabelDragFormat = "DailyQuest.QuestLabelId";
    private const string QnaUrl =
        "https://github.com/samuelraindrwn/daily-quest/blob/main/docs/FAQ.md";
    private const string BugReportUrl =
        "https://github.com/samuelraindrwn/daily-quest/issues/new?template=bug_report.yml";

    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _dayChangeTimer;
    private readonly DispatcherTimer _timerTickTimer;
    private bool _isCompact;
    private double _expandedWidth = ExpandedDefaultWidth;
    private double _expandedHeight = ExpandedDefaultHeight;
    private Point _dragStart;
    private ChecklistItem? _dragCandidate;
    private Border? _dropTarget;
    private Point _labelDragStart;
    private Guid? _labelDragCandidateId;
    private Border? _labelDropTarget;
    private bool? _schedulePopupWasOpenOnPointerDown;
    private bool? _durationPopupWasOpenOnPointerDown;
    private bool? _labelPopupWasOpenOnPointerDown;
    private bool? _sortPopupWasOpenOnPointerDown;
    private bool? _itemLabelPopupWasOpenOnPointerDown;
    private ChecklistItem? _itemLabelTarget;
    private ContextMenu? _activeQuestCopyMenu;
    private ChecklistItem? _questCopySource;
    private ContextMenu? _activeScheduleDayCopyMenu;
    private int? _scheduleDayCopySourceOffset;

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

        _timerTickTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _timerTickTimer.Tick += (_, _) => _viewModel.TickTimers();
        _timerTickTimer.Start();
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

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (SchedulePopup is not null)
        {
            SchedulePopup.IsOpen = false;
        }

        if (DurationPopup is not null)
        {
            DurationPopup.IsOpen = false;
        }

        if (LabelPopup is not null)
        {
            LabelPopup.IsOpen = false;
        }

        if (SortPopup is not null)
        {
            SortPopup.IsOpen = false;
        }

        if (ItemLabelPopup is not null)
        {
            ItemLabelPopup.IsOpen = false;
        }

        if (_activeQuestCopyMenu is not null)
        {
            _activeQuestCopyMenu.IsOpen = false;
        }

        if (_activeScheduleDayCopyMenu is not null)
        {
            _activeScheduleDayCopyMenu.IsOpen = false;
        }
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

    private void ScheduleOption_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button { DataContext: ScheduleOptionViewModel })
        {
            SchedulePopup.StaysOpen = true;
        }
    }

    private void SchedulePickerButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // A StaysOpen="False" popup can close before this button receives its
        // Click event. Preserve the state from this pointer gesture so Click
        // can still distinguish "open" from "close" reliably.
        _schedulePopupWasOpenOnPointerDown =
            SchedulePopup.IsOpen || SchedulePickerButton.IsChecked == true;
    }

    private void SchedulePickerButton_Click(object sender, RoutedEventArgs e)
    {
        var shouldOpen = _schedulePopupWasOpenOnPointerDown is bool wasOpen
            ? !wasOpen
            : SchedulePickerButton.IsChecked == true;

        _schedulePopupWasOpenOnPointerDown = null;
        if (shouldOpen)
        {
            DurationPopup.IsOpen = false;
            LabelPopup.IsOpen = false;
            SortPopup.IsOpen = false;
            ItemLabelPopup.IsOpen = false;
        }

        SchedulePopup.IsOpen = shouldOpen;
        SchedulePickerButton.IsChecked = shouldOpen;
    }

    private void SchedulePopup_Opened(object? sender, EventArgs e)
    {
        DurationPopup.IsOpen = false;
        LabelPopup.IsOpen = false;
        SortPopup.IsOpen = false;
        ItemLabelPopup.IsOpen = false;
        SchedulePickerButton.IsChecked = true;
    }

    private void SchedulePopup_Closed(object? sender, EventArgs e)
    {
        SchedulePopup.StaysOpen = false;
        if (_activeScheduleDayCopyMenu is not null)
        {
            _activeScheduleDayCopyMenu.IsOpen = false;
        }

        // StaysOpen="False" can close the popup on mouse-down before the
        // ToggleButton processes the same click. Defer the visual reset so a
        // second click still toggles from checked to unchecked instead of
        // immediately reopening the popup.
        Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            () =>
            {
                if (!SchedulePopup.IsOpen)
                {
                    SchedulePickerButton.IsChecked = false;
                }
            });
    }

    private void DurationPickerButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _durationPopupWasOpenOnPointerDown =
            DurationPopup.IsOpen || DurationPickerButton.IsChecked == true;
    }

    private void DurationPickerButton_Click(object sender, RoutedEventArgs e)
    {
        var shouldOpen = _durationPopupWasOpenOnPointerDown is bool wasOpen
            ? !wasOpen
            : DurationPickerButton.IsChecked == true;

        _durationPopupWasOpenOnPointerDown = null;
        if (shouldOpen)
        {
            SchedulePopup.IsOpen = false;
            LabelPopup.IsOpen = false;
            SortPopup.IsOpen = false;
            ItemLabelPopup.IsOpen = false;
        }

        DurationPopup.IsOpen = shouldOpen;
        DurationPickerButton.IsChecked = shouldOpen;
    }

    private void DurationPopup_Opened(object? sender, EventArgs e)
    {
        SchedulePopup.IsOpen = false;
        LabelPopup.IsOpen = false;
        SortPopup.IsOpen = false;
        ItemLabelPopup.IsOpen = false;
        DurationPickerButton.IsChecked = true;
    }

    private void DurationPopup_Closed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            () =>
            {
                if (!DurationPopup.IsOpen)
                {
                    DurationPickerButton.IsChecked = false;
                }
            });
    }

    private void DurationOption_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: int minutes })
        {
            _viewModel.SetDurationCommand.Execute(minutes);
        }

        DurationPopup.IsOpen = false;
        NewItemTextBox.Focus();
    }

    private void ApplyCustomDuration_Click(object sender, RoutedEventArgs e) =>
        ApplyCustomDuration();

    private void CustomDurationTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyCustomDuration();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            DurationPopup.IsOpen = false;
            NewItemTextBox.Focus();
            e.Handled = true;
        }
    }

    private void CustomDurationTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) =>
        e.Handled = e.Text.Any(character => !char.IsDigit(character));

    private void ApplyCustomDuration()
    {
        if (!int.TryParse(CustomDurationTextBox.Text, out var minutes) ||
            minutes < 1 || minutes > _viewModel.MaximumDurationMinutes)
        {
            CustomDurationTextBox.Focus();
            CustomDurationTextBox.SelectAll();
            return;
        }

        _viewModel.SetDurationCommand.Execute(minutes);
        CustomDurationTextBox.Clear();
        DurationPopup.IsOpen = false;
        NewItemTextBox.Focus();
    }

    private void LabelPickerButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _labelPopupWasOpenOnPointerDown =
            LabelPopup.IsOpen || LabelPickerButton.IsChecked == true;
    }

    private void LabelPickerButton_Click(object sender, RoutedEventArgs e)
    {
        var shouldOpen = _labelPopupWasOpenOnPointerDown is bool wasOpen
            ? !wasOpen
            : LabelPickerButton.IsChecked == true;

        _labelPopupWasOpenOnPointerDown = null;
        if (shouldOpen)
        {
            SchedulePopup.IsOpen = false;
            DurationPopup.IsOpen = false;
            SortPopup.IsOpen = false;
            ItemLabelPopup.IsOpen = false;
        }

        LabelPopup.IsOpen = shouldOpen;
        LabelPickerButton.IsChecked = shouldOpen;
    }

    private void LabelPopup_Opened(object? sender, EventArgs e)
    {
        SchedulePopup.IsOpen = false;
        DurationPopup.IsOpen = false;
        SortPopup.IsOpen = false;
        ItemLabelPopup.IsOpen = false;
        LabelPickerButton.IsChecked = true;
    }

    private void LabelPopup_Closed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            () =>
            {
                if (!LabelPopup.IsOpen)
                {
                    LabelPickerButton.IsChecked = false;
                }
            });
    }

    private void LabelOption_Click(object sender, RoutedEventArgs e)
    {
        LabelPopup.IsOpen = false;
        NewItemTextBox.Focus();
    }

    private void ManageLabels_Click(object sender, RoutedEventArgs e)
    {
        LabelPopup.IsOpen = false;
        _viewModel.ShowSettingsCommand.Execute(null);
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            () => LabelSettingsCard.BringIntoView());
    }

    private void SortPickerButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _sortPopupWasOpenOnPointerDown =
            SortPopup.IsOpen || SortPickerButton.IsChecked == true;
    }

    private void SortPickerButton_Click(object sender, RoutedEventArgs e)
    {
        var shouldOpen = _sortPopupWasOpenOnPointerDown is bool wasOpen
            ? !wasOpen
            : SortPickerButton.IsChecked == true;

        _sortPopupWasOpenOnPointerDown = null;
        if (shouldOpen)
        {
            SchedulePopup.IsOpen = false;
            DurationPopup.IsOpen = false;
            LabelPopup.IsOpen = false;
            ItemLabelPopup.IsOpen = false;
        }

        SortPopup.IsOpen = shouldOpen;
        SortPickerButton.IsChecked = shouldOpen;
    }

    private void SortPopup_Opened(object? sender, EventArgs e)
    {
        SchedulePopup.IsOpen = false;
        DurationPopup.IsOpen = false;
        LabelPopup.IsOpen = false;
        ItemLabelPopup.IsOpen = false;
        SortPickerButton.IsChecked = true;
    }

    private void SortPopup_Closed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            () =>
            {
                if (!SortPopup.IsOpen)
                {
                    SortPickerButton.IsChecked = false;
                }
            });
    }

    private void SortOption_Click(object sender, RoutedEventArgs e) => SortPopup.IsOpen = false;

    private void ItemLabelButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var item = (sender as FrameworkElement)?.DataContext as ChecklistItem;
        _itemLabelPopupWasOpenOnPointerDown =
            ItemLabelPopup.IsOpen && ReferenceEquals(_itemLabelTarget, item);
    }

    private void ItemLabelButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: ChecklistItem item } button)
        {
            return;
        }

        var shouldOpen = _itemLabelPopupWasOpenOnPointerDown is bool wasOpen
            ? !wasOpen
            : !ItemLabelPopup.IsOpen || !ReferenceEquals(_itemLabelTarget, item);
        _itemLabelPopupWasOpenOnPointerDown = null;

        if (!shouldOpen)
        {
            ItemLabelPopup.IsOpen = false;
            return;
        }

        SchedulePopup.IsOpen = false;
        DurationPopup.IsOpen = false;
        LabelPopup.IsOpen = false;
        SortPopup.IsOpen = false;
        _itemLabelTarget = item;
        ItemLabelPopup.PlacementTarget = button;
        ItemLabelPopup.IsOpen = true;
    }

    private void ItemLabelNoneOption_Click(object sender, RoutedEventArgs e) =>
        AssignItemLabelAndClose(null);

    private void ItemLabelOption_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: QuestLabelViewModel label })
        {
            AssignItemLabelAndClose(label.Id);
        }
    }

    private void AssignItemLabelAndClose(Guid? labelId)
    {
        if (_itemLabelTarget is not null)
        {
            _viewModel.AssignItemLabel(_itemLabelTarget, labelId);
        }

        ItemLabelPopup.IsOpen = false;
        _itemLabelTarget = null;
    }

    private void QuestContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu menu ||
            menu.PlacementTarget is not FrameworkElement { DataContext: ChecklistItem item } ||
            !_viewModel.Items.Contains(item))
        {
            if (sender is ContextMenu invalidMenu)
            {
                invalidMenu.IsOpen = false;
            }

            return;
        }

        _viewModel.RollOverToCurrentDay();
        if (!_viewModel.Items.Contains(item))
        {
            menu.IsOpen = false;
            return;
        }

        if (_activeScheduleDayCopyMenu is not null)
        {
            _activeScheduleDayCopyMenu.IsOpen = false;
        }

        _activeScheduleDayCopyMenu = null;
        _scheduleDayCopySourceOffset = null;
        SchedulePopup.StaysOpen = false;
        _activeQuestCopyMenu = menu;
        _questCopySource = item;
        menu.DataContext = CreateCopyDestinationMenu(
            _viewModel.Copy.CopyTo,
            hasSourceQuests: true);
    }

    private void QuestContextMenu_Closed(object sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(sender, _activeQuestCopyMenu))
        {
            _activeQuestCopyMenu = null;
            _questCopySource = null;
        }
    }

    private void ScheduleDayContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu menu ||
            menu.PlacementTarget is not FrameworkElement
            {
                DataContext: ScheduleOptionViewModel sourceOption
            })
        {
            if (sender is ContextMenu invalidMenu)
            {
                invalidMenu.IsOpen = false;
            }

            return;
        }

        _viewModel.RollOverToCurrentDay();
        if (_activeQuestCopyMenu is not null)
        {
            _activeQuestCopyMenu.IsOpen = false;
        }

        _activeQuestCopyMenu = null;
        _questCopySource = null;
        _activeScheduleDayCopyMenu = menu;
        _scheduleDayCopySourceOffset = sourceOption.Offset;
        SchedulePopup.StaysOpen = true;
        menu.DataContext = CreateCopyDestinationMenu(
            _viewModel.Copy.CopyAllTo,
            _viewModel.HasScheduleDayQuests(sourceOption.Offset));
    }

    private void ScheduleDayContextMenu_Closed(object sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(sender, _activeScheduleDayCopyMenu))
        {
            _activeScheduleDayCopyMenu = null;
            _scheduleDayCopySourceOffset = null;
        }

        SchedulePopup.StaysOpen = false;
    }

    private CopyDestinationMenuViewModel CreateCopyDestinationMenu(
        string title,
        bool hasSourceQuests) => new()
        {
            Title = title,
            EmptyMessage = _viewModel.Copy.NoQuestsToCopy,
            ScheduleOptions = [.. _viewModel.ScheduleOptions],
            HasSourceQuests = hasSourceQuests
        };

    private void CopyDestinationOption_Click(object sender, RoutedEventArgs e)
    {
        var questMenu = _activeQuestCopyMenu;
        var scheduleDayMenu = _activeScheduleDayCopyMenu;
        var isScheduleDayCopy = _scheduleDayCopySourceOffset.HasValue;

        if (sender is Button { DataContext: ScheduleOptionViewModel option })
        {
            if (_questCopySource is not null)
            {
                var request = new QuestCopyRequest(_questCopySource, option.Offset);
                if (_viewModel.CopyItemCommand.CanExecute(request))
                {
                    _viewModel.CopyItemCommand.Execute(request);
                }
            }
            else if (_scheduleDayCopySourceOffset is int sourceOffset)
            {
                var request = new ScheduleDayCopyRequest(sourceOffset, option.Offset);
                if (_viewModel.CopyScheduleDayCommand.CanExecute(request))
                {
                    _viewModel.CopyScheduleDayCommand.Execute(request);
                }
            }
        }

        if (questMenu is not null)
        {
            questMenu.IsOpen = false;
        }

        if (scheduleDayMenu is not null)
        {
            scheduleDayMenu.IsOpen = false;
        }

        if (isScheduleDayCopy)
        {
            SchedulePopup.StaysOpen = false;
            SchedulePopup.IsOpen = false;
            SchedulePickerButton.IsChecked = false;
        }

        e.Handled = true;
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
        if (!_viewModel.IsManualSort ||
            sender is not FrameworkElement element ||
            element.DataContext is not ChecklistItem item)
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
        if (!_viewModel.IsManualSort ||
            sender is not Border card ||
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
            if (!_viewModel.IsManualSort ||
                sender is not Border card ||
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

    private void LabelDragHandle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element ||
            element.DataContext is not QuestLabelViewModel label)
        {
            return;
        }

        _labelDragStart = e.GetPosition(this);
        _labelDragCandidateId = label.Id;
        Mouse.Capture(element);
        e.Handled = true;
    }

    private void LabelDragHandle_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            _labelDragCandidateId = null;
            Mouse.Capture(null);
            return;
        }

        if (!_labelDragCandidateId.HasValue)
        {
            return;
        }

        var current = e.GetPosition(this);
        if (Math.Abs(current.X - _labelDragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(current.Y - _labelDragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        var sourceId = _labelDragCandidateId.Value;
        _labelDragCandidateId = null;
        Mouse.Capture(null);

        var data = new DataObject();
        data.SetData(LabelDragFormat, sourceId);
        try
        {
            DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);
        }
        finally
        {
            ClearLabelDropIndicators();
        }

        e.Handled = true;
    }

    private void LabelDragHandle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _labelDragCandidateId = null;
        Mouse.Capture(null);
        e.Handled = true;
    }

    private void LabelDragHandle_LostMouseCapture(object sender, MouseEventArgs e)
    {
        _labelDragCandidateId = null;
    }

    private void LabelEditorRow_DragOver(object sender, DragEventArgs e)
    {
        if (sender is not Border row ||
            row.DataContext is not QuestLabelViewModel target ||
            e.Data.GetData(LabelDragFormat) is not Guid sourceId ||
            _viewModel.Labels.FirstOrDefault(label => label.Id == sourceId) is not { } source ||
            !_viewModel.Labels.Contains(target))
        {
            ClearLabelDropIndicators();
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var dropAfter = e.GetPosition(row).Y > row.ActualHeight / 2;
        if (!TryGetLabelMoveDestination(source, target, dropAfter, out _))
        {
            ClearLabelDropIndicators();
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        ShowLabelDropIndicator(row, dropAfter);
        AutoScrollLabelSettings(e);
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void LabelEditorRow_DragLeave(object sender, DragEventArgs e)
    {
        if (sender is Border row && ReferenceEquals(row, _labelDropTarget))
        {
            ClearLabelDropIndicators();
        }
    }

    private void LabelEditorRow_Drop(object sender, DragEventArgs e)
    {
        try
        {
            if (sender is not Border row ||
                row.DataContext is not QuestLabelViewModel target ||
                e.Data.GetData(LabelDragFormat) is not Guid sourceId ||
                _viewModel.Labels.FirstOrDefault(label => label.Id == sourceId) is not { } source)
            {
                return;
            }

            var dropAfter = e.GetPosition(row).Y > row.ActualHeight / 2;
            if (ReorderLabel(source, target, dropAfter))
            {
                e.Effects = DragDropEffects.Move;
            }

            e.Handled = true;
        }
        finally
        {
            ClearLabelDropIndicators();
        }
    }

    private bool ReorderLabel(
        QuestLabelViewModel source,
        QuestLabelViewModel target,
        bool dropAfter)
    {
        return TryGetLabelMoveDestination(source, target, dropAfter, out var destinationIndex) &&
               _viewModel.MoveLabel(source.Id, destinationIndex);
    }

    private bool TryGetLabelMoveDestination(
        QuestLabelViewModel source,
        QuestLabelViewModel target,
        bool dropAfter,
        out int destinationIndex)
    {
        destinationIndex = -1;
        var sourceIndex = _viewModel.Labels.IndexOf(source);
        var targetIndex = _viewModel.Labels.IndexOf(target);
        if (sourceIndex < 0 || targetIndex < 0)
        {
            return false;
        }

        destinationIndex = targetIndex + (dropAfter ? 1 : 0);
        if (sourceIndex < destinationIndex)
        {
            destinationIndex--;
        }

        return sourceIndex != destinationIndex;
    }

    private void ShowLabelDropIndicator(Border row, bool after)
    {
        if (!ReferenceEquals(_labelDropTarget, row))
        {
            ClearLabelDropIndicators();
            _labelDropTarget = row;
        }

        var beforeIndicator = FindVisualChildByName<Border>(row, "LabelDropBeforeIndicator");
        var afterIndicator = FindVisualChildByName<Border>(row, "LabelDropAfterIndicator");
        if (beforeIndicator is not null)
        {
            beforeIndicator.Visibility = after ? Visibility.Collapsed : Visibility.Visible;
        }

        if (afterIndicator is not null)
        {
            afterIndicator.Visibility = after ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ClearLabelDropIndicators()
    {
        if (_labelDropTarget is not null)
        {
            var beforeIndicator = FindVisualChildByName<Border>(
                _labelDropTarget,
                "LabelDropBeforeIndicator");
            var afterIndicator = FindVisualChildByName<Border>(
                _labelDropTarget,
                "LabelDropAfterIndicator");
            if (beforeIndicator is not null)
            {
                beforeIndicator.Visibility = Visibility.Collapsed;
            }

            if (afterIndicator is not null)
            {
                afterIndicator.Visibility = Visibility.Collapsed;
            }
        }

        _labelDropTarget = null;
    }

    private void AutoScrollLabelSettings(DragEventArgs e)
    {
        var position = e.GetPosition(SettingsScrollViewer);
        const double edge = 42;
        const double step = 20;

        if (position.Y < edge)
        {
            SettingsScrollViewer.ScrollToVerticalOffset(
                Math.Max(0, SettingsScrollViewer.VerticalOffset - step));
        }
        else if (position.Y > SettingsScrollViewer.ViewportHeight - edge)
        {
            SettingsScrollViewer.ScrollToVerticalOffset(
                Math.Min(
                    SettingsScrollViewer.ScrollableHeight,
                    SettingsScrollViewer.VerticalOffset + step));
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

    private void AddLabel_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.TryAddLabel(NewLabelNameTextBox.Text, NewLabelColorTextBox.Text))
        {
            NewLabelNameTextBox.Clear();
            NewLabelColorTextBox.Text = "#5B8A72";
            HideLabelEditorError();
            NewLabelNameTextBox.Focus();
            return;
        }

        ShowLabelEditorError();
        NewLabelNameTextBox.Focus();
        NewLabelNameTextBox.SelectAll();
    }

    private void NewLabelNameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddLabel_Click(sender, e);
            e.Handled = true;
        }
    }

    private void SaveLabel_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: QuestLabelViewModel label } element ||
            !TryGetLabelEditorFields(element, out var nameEditor, out var colorEditor))
        {
            return;
        }

        if (_viewModel.TryUpdateLabel(label.Id, nameEditor.Text, colorEditor.Text))
        {
            HideLabelEditorError();
            return;
        }

        ShowLabelEditorError();
        nameEditor.Focus();
        nameEditor.SelectAll();
    }

    private void DeleteLabel_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: QuestLabelViewModel label })
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            _viewModel.Copy.DeleteLabelConfirmMessage,
            _viewModel.Copy.DeleteLabelConfirmTitle,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (result == MessageBoxResult.Yes)
        {
            _viewModel.DeleteLabel(label.Id);
            HideLabelEditorError();
        }
    }

    private void PickExistingLabelColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element ||
            !TryGetLabelEditorFields(element, out _, out var colorEditor))
        {
            return;
        }

        PickColorInto(colorEditor);
    }

    private void PickNewLabelColor_Click(object sender, RoutedEventArgs e) =>
        PickColorInto(NewLabelColorTextBox);

    private bool TryGetLabelEditorFields(
        DependencyObject element,
        out TextBox nameEditor,
        out TextBox colorEditor)
    {
        var container = ItemsControl.ContainerFromElement(LabelSettingsList, element);
        nameEditor = container is null
            ? null!
            : FindVisualChildByName<TextBox>(container, "LabelNameEditor")!;
        colorEditor = container is null
            ? null!
            : FindVisualChildByName<TextBox>(container, "LabelColorEditor")!;
        return nameEditor is not null && colorEditor is not null;
    }

    private void PickColorInto(TextBox target)
    {
        using var dialog = new Forms.ColorDialog
        {
            AllowFullOpen = true,
            AnyColor = true,
            FullOpen = true
        };

        try
        {
            if (System.Windows.Media.ColorConverter.ConvertFromString(target.Text) is Color color)
            {
                dialog.Color = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            }
        }
        catch (FormatException)
        {
            // Invalid text simply falls back to the native picker's default color.
        }

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            target.Text = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
            HideLabelEditorError();
        }
    }

    private void ShowLabelEditorError()
    {
        LabelEditorErrorText.Text = _viewModel.Copy.InvalidLabel;
        LabelEditorErrorText.Visibility = Visibility.Visible;
    }

    private void HideLabelEditorError()
    {
        LabelEditorErrorText.Visibility = Visibility.Collapsed;
    }

    private void OpenQna_Click(object sender, RoutedEventArgs e) => OpenExternalUrl(QnaUrl);

    private void OpenBugReport_Click(object sender, RoutedEventArgs e) => OpenExternalUrl(BugReportUrl);

    private void ResetWindowSize_Click(object sender, RoutedEventArgs e)
    {
        var workArea = SystemParameters.WorkArea;
        var availableWidth = Math.Max(ExpandedMinWidth, workArea.Width - (ScreenEdgeGap * 2));
        var availableHeight = Math.Max(ExpandedMinHeight, workArea.Height - (ScreenEdgeGap * 2));

        _expandedWidth = Math.Min(ExpandedDefaultWidth, availableWidth);
        _expandedHeight = Math.Min(ExpandedDefaultHeight, availableHeight);
        Width = _expandedWidth;
        Height = _expandedHeight;
        PositionAtTopRight();
        _viewModel.SaveWindowSize(_expandedWidth, _expandedHeight);
    }

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
        _timerTickTimer.Stop();
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
