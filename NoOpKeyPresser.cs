namespace Typo;

public sealed class NoOpKeyPresser : IKeyPresserInterface
{
    public void PressKey(string key)
    {
    }

    public void ReleaseKey(string key)
    {
    }
}
