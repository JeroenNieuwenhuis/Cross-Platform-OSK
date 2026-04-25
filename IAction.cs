namespace Typo;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public interface IAction
{
    public void Start();
    public void Stop();
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ActionKind
{
    NormalKey,
    Modifier,
    PersistentToggle
}

[JsonConverter(typeof(StringEnumConverter))]
public enum LatchReleasePolicy
{
    AfterNonModifier,
    ManualOnly
}

public interface IActionMetadata
{
    public ActionKind Kind { get; }
    public LatchReleasePolicy ReleasePolicy { get; }
}
