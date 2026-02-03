using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace GoldPriceMonitor.Services;

public class HotKeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int HotKeyId = 1;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly IntPtr _handle;
    private readonly Action _onHotKeyPressed;
    private bool _disposed;

    public HotKeyService(IntPtr handle, Action onHotKeyPressed)
    {
        _handle = handle;
        _onHotKeyPressed = onHotKeyPressed;
    }

    public bool Register(ModifierKeys modifiers = ModifierKeys.Windows | ModifierKeys.Alt, Key key = Key.G)
    {
        uint fsModifiers = 0;
        if (modifiers.HasFlag(ModifierKeys.Control)) fsModifiers |= 0x0002;
        if (modifiers.HasFlag(ModifierKeys.Shift)) fsModifiers |= 0x0004;
        if (modifiers.HasFlag(ModifierKeys.Alt)) fsModifiers |= 0x0001;
        if (modifiers.HasFlag(ModifierKeys.Windows)) fsModifiers |= 0x0008;

        uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);
        return RegisterHotKey(_handle, HotKeyId, fsModifiers, vk);
    }

    public void Unregister()
    {
        UnregisterHotKey(_handle, HotKeyId);
    }

    public static bool ProcessMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, out Action? hotKeyAction)
    {
        hotKeyAction = null;

        if (msg == WmHotkey && wParam.ToInt32() == HotKeyId)
        {
            hotKeyAction = null; // Action需要在外部处理
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Unregister();
            _disposed = true;
        }
    }
}
