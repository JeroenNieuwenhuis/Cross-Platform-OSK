using System.ComponentModel;
using System.Text;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Threading;
using Tmds.DBus.Protocol;
using Typo;

#if _WINDOWS

namespace Typo.Windows;

using System;
using System.Runtime.InteropServices;

public class WindowManager : IWindowManagerInterface
{
    private const int HwndTopmost = -1;
    private const int HwndNotopmost = -2;
    private const uint SwpNomove = 0x0002;
    private const uint SwpNosize = 0x0001;
    private const int GwlExstyle = -20;
    private const int WsExLayered = 0x80000;
    private const int WsExNoactivate = 0x08000000;
    private const int WsExAppwindow = 0x00040000;
    private const uint LwaAlpha = 0x2;
    private const uint SwpShowwindow = 0x0040;
    
    private const uint SwpNoactivate = 0x0010;
    private const uint SwpNoownerzorder = 0x0200;
    
    // Import the SetWindowPos function from user32.dll
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd, 
        IntPtr hWndInsertAfter, 
        int x, 
        int y, 
        int cx, 
        int cy, 
        uint uFlags);
    
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool RegisterShellHookWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern uint RegisterWindowMessage(string lpString);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private int WM_SHELLHOOKMESSAGE;

    private IntPtr _mWindowHandle;
    private Window _window;
    
    private DispatcherTimer timer;
    // Constructor using a provided window handle.
    public WindowManager(Window window)
    {
        _window = window;
        _mWindowHandle = window.TryGetPlatformHandle().Handle;
        Win32Properties.AddWndProcHookCallback(window, WndProc);
        ShellHook_Load();
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        timer.Start();
    }
    
    private void Timer_Tick(object sender, EventArgs e)
    {
        SetAlwaysOnTop();
    }
    private void ShellHook_Load()
    {
        // Register for shell hook messages
        WM_SHELLHOOKMESSAGE = (int)RegisterWindowMessage("SHELLHOOK");
        if (WM_SHELLHOOKMESSAGE == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        if (!RegisterShellHookWindow(_mWindowHandle))
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    protected   IntPtr WndProc(
        nint hWnd, 
        uint msg, 
        nint wParam, 
        nint lParam, 
        ref bool handled)
    {
        if (msg == WM_SHELLHOOKMESSAGE)
        {

            // Handle HSHELL_WINDOWACTIVATED (eventType = 4)
            if (wParam == 4 || wParam == 32772)
            {
                SetAlwaysOnTop();
            }
        }
        
        return IntPtr.Zero;
    }

    public void SetAlwaysOnTop()
    {
        // First, remove the topmost flag to reset Z-order
        SetWindowPos(_mWindowHandle, HwndNotopmost, 0, 0, 0, 0, SwpNomove | SwpNosize | SwpShowwindow);
        
        // Then reapply the topmost flag to force it above all windows
        SetWindowPos(_mWindowHandle, HwndTopmost, 0, 0, 0, 0, SwpNomove | SwpNosize | SwpShowwindow);
    }

    public void SetUnfocusable()
    {
        IntPtr style = GetWindowLongPtr(_mWindowHandle, GwlExstyle);
        style |= WsExNoactivate;
        SetWindowLongPtr(_mWindowHandle, GwlExstyle, style);
    }
}

#endif