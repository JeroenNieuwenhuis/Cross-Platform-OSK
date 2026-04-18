using Newtonsoft.Json;

namespace Typo;

[JsonObject(MemberSerialization.OptIn)]
public class KeyPressAction : IAction, IActionMetadata
{
    [JsonProperty] 
    public string? key;

    [JsonProperty]
    public ActionKind kind { get; set; } = ActionKind.NormalKey;

    [JsonProperty]
    public LatchReleasePolicy releasePolicy { get; set; } = LatchReleasePolicy.AfterNonModifier;
    
    private IKeyPresserInterface _keyPresser = IKeyPresserInterface.GetInstance();

    public ActionKind Kind => kind;
    public LatchReleasePolicy ReleasePolicy => releasePolicy;
    
    public void Start()
    {
        if (key != null) _keyPresser.PressKey(key);
    }

    public void Stop()
    {
        if (key != null) _keyPresser.ReleaseKey(key);
    }
}
