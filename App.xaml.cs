using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GoldPriceMonitor.Utils;
using GoldPriceMonitor.Views;

namespace GoldPriceMonitor;

public partial class App : System.Windows.Application
{
    private NotifyIconManager? _notifyIcon;
    private MainWindow? _mainWindow;
    private bool _isExiting;
    private Mutex? _instanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 添加全局异常处理
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        StartupLogger.Log("OnStartup begin");

        _instanceMutex = new Mutex(true, "GoldPriceMonitor_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            _instanceMutex.Dispose();
            _instanceMutex = null;
            System.Windows.MessageBox.Show("应用已在运行中", "金价监控", MessageBoxButton.OK);
            Shutdown();
            return;
        }

        base.OnStartup(e);

        try
        {
            _mainWindow = new MainWindow();
            _mainWindow.OnHideRequest += HideMainWindow;
            MainWindow = _mainWindow;
            _mainWindow.Show();
            StartupLogger.Log("MainWindow shown");
        }
        catch (Exception ex)
        {
            StartupLogger.Log("MainWindow init failed", ex);
            System.Windows.MessageBox.Show($"启动失败: {ex.Message}\n{ex.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        try
        {
            _notifyIcon = new NotifyIconManager();
            _notifyIcon.OnShowWindow += ShowMainWindow;
            _notifyIcon.OnHideWindow += HideMainWindow;
            _notifyIcon.OnExit += ExitApplication;
            _notifyIcon.ShowBalloonTip("金价监控器", "应用已启动 - 按 Win+Alt+U 显示/隐藏窗口", GoldPriceMonitor.Utils.ToolTipIcon.Info);
            StartupLogger.Log("NotifyIcon created");
        }
        catch (Exception ex)
        {
            StartupLogger.Log("NotifyIcon init failed", ex);
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        StartupLogger.Log("Unhandled exception", ex);
        System.Windows.MessageBox.Show($"未捕获的异常: {ex?.Message}\n{ex?.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        StartupLogger.Log("Dispatcher unhandled exception", e.Exception);
        System.Windows.MessageBox.Show($"未捕获的UI异常: {e.Exception.Message}\n{e.Exception.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        StartupLogger.Log("Unobserved task exception", e.Exception);
        e.SetObserved();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow != null)
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }
    }

    private void HideMainWindow()
    {
        _mainWindow?.Hide();
    }

    private void ExitApplication()
    {
        if (!_isExiting)
        {
            _isExiting = true;
            _mainWindow?.Close();
            _notifyIcon?.Dispose();
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _notifyIcon?.Dispose();
        if (_instanceMutex != null)
        {
            try
            {
                _instanceMutex.ReleaseMutex();
            }
            catch
            {
                // ignore release errors
            }
            _instanceMutex.Dispose();
            _instanceMutex = null;
        }
        base.OnExit(e);
    }
}
