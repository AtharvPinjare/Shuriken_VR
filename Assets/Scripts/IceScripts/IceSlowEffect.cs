using UnityEngine;

[CreateAssetMenu(fileName = "NewIceSlowEffect", menuName = "Shuriken VR/Status Effects/Ice Slow")]
public class IceSlowEffect : StatusEffect
{
    [Range(0f, 1f)] public float slowMultiplier = 0.5f;
    public float duration = 3f;

    public override void Apply(GameObject target)
    {
        if (target.TryGetComponent(out EnemyMove enemyMove))
            enemyMove.ApplySlow(slowMultiplier, duration);
    }
}