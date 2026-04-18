using System.Collections.Generic;

namespace Typo;

public sealed class ActionCoordinator
{
    private static readonly ActionCoordinator Instance = new();

    private readonly HashSet<LatchedAction> _latchedActions = [];
    private readonly object _syncRoot = new();

    private ActionCoordinator()
    {
    }

    public static ActionCoordinator GetInstance()
    {
        return Instance;
    }

    public void Toggle(IAction action)
    {
        lock (_syncRoot)
        {
            LatchedAction latchedAction = new(action, GetMetadata(action));
            if (_latchedActions.Remove(latchedAction))
            {
                action.Stop();
                return;
            }

            action.Start();
            _latchedActions.Add(latchedAction);
        }
    }

    public void NotifyActionCompleted(IAction? action)
    {
        if (action == null)
        {
            return;
        }

        IActionMetadata metadata = GetMetadata(action);
        if (metadata.Kind == ActionKind.Modifier)
        {
            return;
        }

        List<LatchedAction> actionsToRelease = [];

        lock (_syncRoot)
        {
            foreach (LatchedAction latchedAction in _latchedActions)
            {
                if (latchedAction.Metadata.ReleasePolicy == LatchReleasePolicy.AfterNonModifier)
                {
                    actionsToRelease.Add(latchedAction);
                }
            }

            foreach (LatchedAction latchedAction in actionsToRelease)
            {
                _latchedActions.Remove(latchedAction);
            }
        }

        foreach (LatchedAction latchedAction in actionsToRelease)
        {
            latchedAction.Action.Stop();
        }
    }

    private static IActionMetadata GetMetadata(IAction action)
    {
        return action as IActionMetadata ?? DefaultActionMetadata.Instance;
    }

    private sealed record LatchedAction(IAction Action, IActionMetadata Metadata);

    private sealed class DefaultActionMetadata : IActionMetadata
    {
        public static readonly DefaultActionMetadata Instance = new();

        public ActionKind Kind => ActionKind.NormalKey;
        public LatchReleasePolicy ReleasePolicy => LatchReleasePolicy.AfterNonModifier;
    }
}
