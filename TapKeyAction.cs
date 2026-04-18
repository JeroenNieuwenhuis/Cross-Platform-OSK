using Newtonsoft.Json;

namespace Typo;

[JsonObject(MemberSerialization.OptIn)]
public sealed class TapKeyAction : IAction, IActionMetadata
{
    [JsonProperty]
    public string? key { get; set; }

    [JsonProperty]
    public ActionKind kind { get; set; } = ActionKind.NormalKey;

    [JsonProperty]
    public LatchReleasePolicy releasePolicy { get; set; } = LatchReleasePolicy.AfterNonModifier;

    private readonly IKeyPresserInterface _keyPresser = IKeyPresserInterface.GetInstance();

    public ActionKind Kind => kind;
    public LatchReleasePolicy ReleasePolicy => releasePolicy;

    public void Start()
    {
        if (key == null)
        {
            return;
        }

        _keyPresser.PressKey(key);
        _keyPresser.ReleaseKey(key);
    }

    public void Stop()
    {
    }
}
