#if _LINUX

namespace Typo.Linux;

using System;
using System.Runtime.InteropServices;

public class WindowManager
{
    private const ulong X11_PROP_ATOM = 31; // XA_ATOM
    private IntPtr mDisplay;
    private IntPtr mWindow;

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XChangeProperty(
        IntPtr display, IntPtr window, IntPtr property,
        IntPtr type, int format, int mode,
        ref IntPtr data, int nelements);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

    public WindowManager(IntPtr window)
    {
        mDisplay = XOpenDisplay(IntPtr.Zero);
        if (mDisplay == IntPtr.Zero)
            throw new Exception("Failed to open X11 display.");
        
        mWindow = window;
    }

    public void SetAlwaysOnTop(bool alwaysOnTop)
    {
        IntPtr atomNetWmState = XInternAtom(mDisplay, "_NET_WM_STATE", false);
        IntPtr atomNetWmStateAbove = XInternAtom(mDisplay, "_NET_WM_STATE_ABOVE", false);
        IntPtr data = alwaysOnTop ? atomNetWmStateAbove : IntPtr.Zero;
        
        XChangeProperty(mDisplay, mWindow, atomNetWmState, (IntPtr)X11_PROP_ATOM, 32, 0, ref data, 1);
    }

    ~WindowManager()
    {
        if (mDisplay != IntPtr.Zero)
            XCloseDisplay(mDisplay);
    }
}

#endif