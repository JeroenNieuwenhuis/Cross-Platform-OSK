namespace Typo;

using System;

public interface IKeyPresserInterface
{
    private static IKeyPresserInterface? _instance;
    
    public void PressKey(string key);
    public void ReleaseKey(string key);

    public static IKeyPresserInterface GetInstance()
    {
        if (_instance == null)
        {
            #if _WINDOWS
                _instance = new Windows.KeyPresser();
            #elif _LINUX
                _instance = CreateLinuxKeyPresser();
            #else
                _instance = new NoOpKeyPresser();
            #endif
        }
        return _instance;
    }

#if _LINUX
    private static IKeyPresserInterface CreateLinuxKeyPresser()
    {
        string? sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
        bool isWaylandSession = string.Equals(sessionType, "wayland", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));
        bool isX11Session = string.Equals(sessionType, "x11", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY"));

        if (isWaylandSession)
        {
            if (Linux.WaylandKeyPresser.IsSupported())
            {
                return new Linux.WaylandKeyPresser();
            }
        }

        if (isX11Session)
        {
            if (Linux.X11KeyPresser.IsSupported())
            {
                return new Linux.X11KeyPresser();
            }
        }

        return new NoOpKeyPresser();
    }
#endif
}
