using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using GoldPriceMonitor.Models;
using GoldPriceMonitor.Services;
using GoldPriceMonitor.Utils;
using GoldPriceMonitor.ViewModels;

namespace GoldPriceMonitor.Views;

public partial class ChartWindow : Window
{
    private readonly ChartViewModel _viewModel;
    private readonly ObservableCollection<PricePoint> _sourceHistory;

    public ChartWindow(Window ownerWindow, MainViewModel mainViewModel)
    {
        InitializeComponent();

        Owner = ownerWindow;

        var apiService = new GoldApiService(AppSettings.Default.ApiToken);
        _viewModel = new ChartViewModel(apiService, mainViewModel.PriceHistory);
        _sourceHistory = mainViewModel.PriceHistory;

        DataContext = _viewModel;

        // 监听历史数据变化
        mainViewModel.PriceHistory.CollectionChanged += OnHistoryChanged;

        // 初始加载
        _viewModel.UpdateFromHistory(_sourceHistory);

        // 窗口位置与主窗口对齐
        Left = ownerWindow.Left;
        Top = ownerWindow.Top;
    }

    private void OnHistoryChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => _viewModel.UpdateFromHistory(_sourceHistory));
            return;
        }

        _viewModel.UpdateFromHistory(_sourceHistory);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TimeRange_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ChartViewModel vm)
        {
            _viewModel.UpdateFromHistory(_sourceHistory);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
    }
}
