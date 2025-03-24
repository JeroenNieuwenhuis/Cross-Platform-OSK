using System;
using Avalonia.Controls;
#if _WINDOWS
using CrossPlatformApp.Windows;
#elif _OSX
using CrossPlatformApp.MacOS;
#elif  _LINUX
using CrossPlatformApp.LinuxX11
#endif

namespace CrossPlatformApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        IntPtr? handle = TryGetPlatformHandle()?.Handle;
        if (handle.HasValue)
        {
            var windowManager = new WindowManager(handle.Value);
            windowManager.SetAlwaysOnTop(true);
        }
            
        
    }
}