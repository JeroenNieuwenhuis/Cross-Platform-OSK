using Avalonia.Controls;
using System;

namespace Typo;

public interface IWindowManagerInterface
{
    private static IWindowManagerInterface? _instance;
    public void SetAlwaysOnTop();
    public void SetUnfocusable();
    
    public static IWindowManagerInterface GetInstance(Window window)
    {
        if (_instance == null)
        {
#if _WINDOWS
            _instance = new Windows.WindowManager(window);
#elif _LINUX
            _instance = CreateLinuxWindowManager(window);
#else
            _instance = new NoOpWindowManager();
#endif
        }
        return _instance;
    }

#if _LINUX
    private static IWindowManagerInterface CreateLinuxWindowManager(Window window)
    {
        string? sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
        bool isWaylandSession = string.Equals(sessionType, "wayland", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));
        bool isX11Session = string.Equals(sessionType, "x11", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY"));

        if (isX11Session && !isWaylandSession)
        {
            return new Linux.WindowManager(window);
        }

        if (isWaylandSession)
        {
            return new NoOpWindowManager();
        }

        return isX11Session ? new Linux.WindowManager(window) : new NoOpWindowManager();
    }
#endif
}
