using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    // Implementations must NOT store per-target runtime state here (timers, flags).
    // This asset is shared across every enemy hit by it — dispatch to a component
    // on the target and let the target own its own state.
    public abstract void Apply(GameObject target);
}