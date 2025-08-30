namespace Typo;

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
            #endif
        }
        return _instance!;
    }
}