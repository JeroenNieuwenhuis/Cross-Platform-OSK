namespace Typo;

public interface IAction
{
    public void Start();
    public void Stop();
}

public enum ActionKind
{
    NormalKey,
    Modifier,
    PersistentToggle
}

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
