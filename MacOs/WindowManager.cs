#if _OSX

namespace Typo.MacOS;

using System;
using System.Runtime.InteropServices;
using Foundation;
using AppKit;

public class WindowManager
{
    private IntPtr mWindowHandle;

    [DllImport("libobjc.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr GetClass(string className);

    [DllImport("libobjc.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr GetSelector(string selectorName);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")] 
    private static extern void SendMessage(IntPtr receiver, IntPtr selector, bool value);

    public WindowManager(IntPtr windowHandle)
    {
        mWindowHandle = windowHandle;
    }

    public void SetAlwaysOnTop(bool alwaysOnTop)
    {
        IntPtr nsWindowClass = GetClass("NSWindow");
        IntPtr selector = GetSelector("setLevel:");
        IntPtr window = mWindowHandle;
        
        // Define level for always-on-top behavior
        IntPtr level = alwaysOnTop ? new IntPtr((int)NSWindowLevel.Floating) : new IntPtr((int)NSWindowLevel.Normal);
        
        SendMessage(window, selector, level);
    }
}

public enum NSWindowLevel
{
    Normal = 0,
    Floating = 3,
    MainMenu = 24,
    Status = 25,
    PopUpMenu = 101,
    Overlay = 102,
    Help = 200
}

#endif