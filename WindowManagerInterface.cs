using Avalonia.Controls;

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
#endif
        }
        return _instance!;
    }
}