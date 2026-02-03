using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinFormsToolTipIcon = System.Windows.Forms.ToolTipIcon;

namespace GoldPriceMonitor.Utils;

public class NotifyIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly Icon _icon;
    private bool _disposed;

    public event Action? OnShowWindow;
    public event Action? OnHideWindow;
    public event Action? OnExit;

    public NotifyIconManager()
    {
        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.Add("显示窗口", null, (_, __) => OnShowWindow?.Invoke());
        _contextMenu.Items.Add("隐藏窗口", null, (_, __) => OnHideWindow?.Invoke());
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add("退出", null, (_, __) => OnExit?.Invoke());

        _icon = GetAppIcon() ?? SystemIcons.Application;

        _notifyIcon = new NotifyIcon
        {
            Icon = _icon,
            Text = "金价监控器",
            Visible = true,
            ContextMenuStrip = _contextMenu
        };

        _notifyIcon.MouseClick += OnMouseClick;
        _notifyIcon.MouseDoubleClick += OnMouseDoubleClick;
    }

    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            OnShowWindow?.Invoke();
        }
    }

    private void OnMouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            OnShowWindow?.Invoke();
        }
    }

    private static Icon? GetAppIcon()
    {
        try
        {
            // 尝试从文件加载
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "gold.ico");
            if (File.Exists(iconPath))
            {
                return new Icon(iconPath);
            }

            // 尝试从程序集资源加载
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            var iconResource = resourceNames.FirstOrDefault(n => n.EndsWith("gold.ico", StringComparison.OrdinalIgnoreCase));

            if (iconResource != null)
            {
                using var stream = assembly.GetManifestResourceStream(iconResource);
                if (stream != null)
                {
                    return new Icon(stream);
                }
            }
        }
        catch
        {
            // 忽略图标加载失败
        }

        return null;
    }

    public void ShowBalloonTip(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
    {
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = text;
        _notifyIcon.BalloonTipIcon = icon switch
        {
            ToolTipIcon.Warning => WinFormsToolTipIcon.Warning,
            ToolTipIcon.Error => WinFormsToolTipIcon.Error,
            _ => WinFormsToolTipIcon.Info
        };
        _notifyIcon.ShowBalloonTip(3000);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
        _icon.Dispose();
        _disposed = true;
    }
}

public enum ToolTipIcon
{
    Info = 0,
    Warning = 1,
    Error = 2
}
