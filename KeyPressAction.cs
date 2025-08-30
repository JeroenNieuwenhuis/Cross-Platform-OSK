using Newtonsoft.Json;

namespace Typo;

[JsonObject(MemberSerialization.OptIn)]
public class KeyPressAction : IAction
{
    [JsonProperty] 
    public string? key;
    
    private IKeyPresserInterface _keyPresser = IKeyPresserInterface.GetInstance();
    
    public void Start()
    {
        if (key != null) _keyPresser.PressKey(key);
    }

    public void Stop()
    {
        if (key != null) _keyPresser.ReleaseKey(key);
    }
}