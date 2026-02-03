using GoldPriceMonitor.Services;
using GoldPriceMonitor.Utils;
using GoldPriceMonitor.ViewModels;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace GoldPriceMonitor.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly HotKeyService? _hotKeyService;
    private readonly AppSettings _settings;
    private ChartWindow? _chartWindow;
    private SettingsWindow? _settingsWindow;
    private bool _isDragging;

    public event Action? OnHideRequest;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HotKeyId = 1001;
    private const uint ModWinAlt = 0x0001 | 0x0008; // Alt + Win
    private const uint VkU = 0x55; // U key

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        _settings = AppSettings.Load();
        Left = _settings.WindowLeft;
        Top = _settings.WindowTop;

        // 确保窗口在可见屏幕范围内
        EnsureWindowVisible();

        // 如果保存的位置无效，使用屏幕中心
        if (Left <= 0 || Top <= 0)
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }

        // 注册全局快捷键 Win+Alt+G
        try
        {
            _hotKeyService = new HotKeyService(new WindowInteropHelper(this).Handle, OnHotKeyPressed);
            _hotKeyService.Register(ModifierKeys.Windows | ModifierKeys.Alt, Key.U);
        }
        catch
        {
            System.Windows.MessageBox.Show("无法注册快捷键，请确保以管理员权限运行", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // 启动自动刷新
        _ = _viewModel.StartAutoRefreshAsync();

        Closing += OnClosing;
    }

    private void EnsureWindowVisible()
    {
        var workingArea = SystemParameters.WorkArea;
        // 确保窗口在可见区域内
        if (Left < workingArea.Left)
            Left = workingArea.Left + 50;
        if (Top < workingArea.Top)
            Top = workingArea.Top + 50;
        if (Left + Width > workingArea.Right)
            Left = workingArea.Right - Width - 50;
        if (Top + Height > workingArea.Bottom)
            Top = workingArea.Bottom - Height - 50;
    }

    private void OnHotKeyPressed()
    {
        Dispatcher.Invoke(() =>
        {
            if (IsVisible)
            {
                Hide();
                OnHideRequest?.Invoke();
            }
            else
            {
                Show();
                Activate();
            }
        });
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isDragging = true;
            DragMove();
        }
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        OnHideRequest?.Invoke();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
        Hide();
        OnHideRequest?.Invoke();
    }

    private void ChartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_chartWindow == null || !_chartWindow.IsVisible)
        {
            _chartWindow = new ChartWindow(this, _viewModel);
            _chartWindow.Closed += (s, args) => _chartWindow = null;
            _chartWindow.Show();
        }
        else
        {
            _chartWindow.Activate();
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_settingsWindow == null || !_settingsWindow.IsVisible)
        {
            _settingsWindow = new SettingsWindow();
            _settingsWindow.Closed += (s, args) => _settingsWindow = null;
            _settingsWindow.Show();
        }
        else
        {
            _settingsWindow.Activate();
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        _viewModel.Stop();
        _hotKeyService?.Dispose();

        // 保存窗口位置
        _settings.WindowLeft = Left;
        _settings.WindowTop = Top;
        _settings.IsWindowVisible = IsVisible;
        _settings.Save();

        _chartWindow?.Close();
        _settingsWindow?.Close();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // 保存位置
        var hwnd = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WmHotkey = 0x0312;
        if (msg == WmHotkey && wParam.ToInt32() == HotKeyId)
        {
            OnHotKeyPressed();
            handled = true;
        }
        return IntPtr.Zero;
    }
}
