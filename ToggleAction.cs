using Newtonsoft.Json;

namespace Typo;

[JsonObject(MemberSerialization.OptIn)]
public sealed class ToggleAction : IAction, IActionMetadata
{
    [JsonProperty]
    public IAction? action { get; set; }

    [JsonProperty]
    public ActionKind kind { get; set; } = ActionKind.Modifier;

    [JsonProperty]
    public LatchReleasePolicy releasePolicy { get; set; } = LatchReleasePolicy.AfterNonModifier;

    private readonly ActionCoordinator _coordinator = ActionCoordinator.GetInstance();

    public ActionKind Kind => kind;
    public LatchReleasePolicy ReleasePolicy => releasePolicy;

    public void Start()
    {
        if (action == null)
        {
            return;
        }

        _coordinator.Toggle(action);
    }

    public void Stop()
    {
    }
}
