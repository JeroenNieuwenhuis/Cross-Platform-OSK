using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using Typo.Windows;


namespace Typo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        CanResize = false;
        this.Width = 400;
        this.Height = 120;
    
        IntPtr? handle = TryGetPlatformHandle()?.Handle;
        if (handle.HasValue)
        {
            //var windowManager = new WindowManager(this, handle.Value);
            //windowManager.SetAlwaysOnTop(true);
            //windowManager.SetTransparancy(50);
            //this.Opened += (s, e) => windowManager.SetUnfocusable(true);
        }

        //Injector.InjectDllInto(
          //  "C:\\Users\\jeroe\\OneDrive\\Projects\\Cross-Platform-OSK\\bin\\Debug\\net9.0\\DllInjection.dll",
            //"explorer.exe");
    }
}