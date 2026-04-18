#if _LINUX

namespace Typo.Linux;

using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;

public sealed class WindowManager : IWindowManagerInterface, IDisposable
{
    private const int ClientMessage = 33;
    private const int SubstructureRedirectMask = 1 << 20;
    private const int SubstructureNotifyMask = 1 << 19;
    private const long NetWmStateRemove = 0;
    private const long NetWmStateAdd = 1;
    private const long InputHint = 1;

    private readonly Window _window;
    private readonly IntPtr _display;
    private readonly IntPtr _rootWindow;
    private readonly IntPtr _x11Window;
    private readonly bool _isX11Session;
    private bool _disposed;

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr displayName);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XDefaultRootWindow(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XInternAtom(IntPtr display, string atomName, bool onlyIfExists);

    [DllImport("libX11.so.6")]
    private static extern int XSendEvent(
        IntPtr display,
        IntPtr window,
        bool propagate,
        IntPtr eventMask,
        ref XEvent sendEvent);

    [DllImport("libX11.so.6")]
    private static extern int XSetWMHints(IntPtr display, IntPtr window, ref XWMHints wmHints);

    [DllImport("libX11.so.6")]
    private static extern int XFlush(IntPtr display);

    public WindowManager(Window window)
    {
        _window = window;
        _isX11Session = IsX11Session();

        if (!_isX11Session)
        {
            return;
        }

        _display = XOpenDisplay(IntPtr.Zero);
        if (_display == IntPtr.Zero)
        {
            return;
        }

        _rootWindow = XDefaultRootWindow(_display);
        _x11Window = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
    }

    public void SetAlwaysOnTop()
    {
        if (!CanUseX11())
        {
            return;
        }

        SetNetWmState(enabled: true, "_NET_WM_STATE_ABOVE");
    }

    public void SetUnfocusable()
    {
        if (!CanUseX11())
        {
            return;
        }

        // On X11, tell the WM we do not want input focus.
        XWMHints wmHints = new()
        {
            flags = InputHint,
            input = false
        };

        XSetWMHints(_display, _x11Window, ref wmHints);

        SetNetWmState(enabled: true, "_NET_WM_STATE_SKIP_TASKBAR");
        SetNetWmState(enabled: true, "_NET_WM_STATE_SKIP_PAGER");

        XFlush(_display);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_display != IntPtr.Zero)
        {
            XCloseDisplay(_display);
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~WindowManager()
    {
        Dispose();
    }

    private bool CanUseX11()
    {
        if (!_isX11Session)
        {
            return false;
        }

        if (_display == IntPtr.Zero)
        {
            return false;
        }

        if (_x11Window == IntPtr.Zero)
        {
            IntPtr handle = _window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            return handle != IntPtr.Zero;
        }

        return true;
    }

    private void SetNetWmState(bool enabled, string stateAtomName)
    {
        IntPtr targetWindow = _window.TryGetPlatformHandle()?.Handle ?? _x11Window;
        if (targetWindow == IntPtr.Zero)
        {
            return;
        }

        IntPtr netWmState = XInternAtom(_display, "_NET_WM_STATE", false);
        IntPtr stateAtom = XInternAtom(_display, stateAtomName, false);
        if (netWmState == IntPtr.Zero || stateAtom == IntPtr.Zero)
        {
            return;
        }

        XEvent xEvent = new()
        {
            type = ClientMessage,
            xclient = new XClientMessageEvent
            {
                type = ClientMessage,
                serial = IntPtr.Zero,
                send_event = true,
                display = _display,
                window = targetWindow,
                message_type = netWmState,
                format = 32,
                ptr1 = new IntPtr(enabled ? NetWmStateAdd : NetWmStateRemove),
                ptr2 = stateAtom,
                ptr3 = IntPtr.Zero,
                ptr4 = IntPtr.Zero,
                ptr5 = IntPtr.Zero
            }
        };

        IntPtr eventMask = new(SubstructureRedirectMask | SubstructureNotifyMask);
        XSendEvent(_display, _rootWindow, false, eventMask, ref xEvent);
        XFlush(_display);
    }

    private static bool IsX11Session()
    {
        string? sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
        if (string.Equals(sessionType, "x11", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY"))
            && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XClientMessageEvent
    {
        public int type;
        public IntPtr serial;
        [MarshalAs(UnmanagedType.I1)]
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public IntPtr message_type;
        public int format;
        public IntPtr ptr1;
        public IntPtr ptr2;
        public IntPtr ptr3;
        public IntPtr ptr4;
        public IntPtr ptr5;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct XEvent
    {
        [FieldOffset(0)]
        public int type;

        [FieldOffset(0)]
        public XClientMessageEvent xclient;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XWMHints
    {
        public long flags;
        [MarshalAs(UnmanagedType.I1)]
        public bool input;
        public int initial_state;
        public IntPtr icon_pixmap;
        public IntPtr icon_window;
        public int icon_x;
        public int icon_y;
        public IntPtr icon_mask;
        public IntPtr window_group;
    }
}

#endif
