#if _WINDOWS

namespace CrossPlatformApp.Windows;

using System;
using System.Runtime.InteropServices;

public class WindowManager
{
    // Constants for window positioning
    private const int HWND_TOPMOST = -1;
    private const int HWND_NOTOPMOST = -2;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;

    // Import the SetWindowPos function from user32.dll
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd, 
        IntPtr hWndInsertAfter, 
        int X, 
        int Y, 
        int cx, 
        int cy, 
        uint uFlags);

    private IntPtr mWindowHandle;

    // Constructor using a provided window handle.
    public WindowManager(IntPtr windowHandle)
    {
        mWindowHandle = windowHandle;
    }

    public void SetAlwaysOnTop(bool alwaysOnTop)
    {
        // Choose appropriate parameter based on the alwaysOnTop value
        IntPtr insertAfter = new IntPtr(alwaysOnTop ? HWND_TOPMOST : HWND_NOTOPMOST);
        
        // Call SetWindowPos with flags to prevent moving or resizing
        SetWindowPos(mWindowHandle, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
    }
}

#endif