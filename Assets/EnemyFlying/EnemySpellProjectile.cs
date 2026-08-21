using UnityEngine;

public class EnemySpellProjectile : MonoBehaviour
{
    [Header("Spell Settings")]
    public float damage = 15f;
    public GameObject impactVFX;
    public float lifetime = 8f;

    // Shooter-exclusion guard — same Health.Faction pattern FireballProjectile.cs
    // uses (added in Dragon Stage 2). Skips damage only if the hit Health shares
    // this faction.
    public Health.Faction shooterFaction = Health.Faction.Enemy;

    [Header("Player hit detection")]
    [Tooltip("The player has no Collider anywhere in its hierarchy, so OnCollisionEnter/OnTriggerEnter can never fire against it — checked by proximity instead, same pattern FireballProjectile.cs uses for its homing case.")]
    public Transform target;
    [SerializeField] private float hitRadius = 0.25f;

    private bool _hasResolved;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (_hasResolved || target == null) return;

        if (Vector3.Distance(transform.position, target.position) <= hitRadius)
            ResolveHit(target.gameObject, transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ResolveHit(collision.gameObject, collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        ResolveHit(other.gameObject, transform.position);
    }

    private void ResolveHit(GameObject hitObject, Vector3 hitPoint)
    {
        if (_hasResolved) return;

        if (hitObject.TryGetComponent(out Health health))
        {
            if (health.faction == shooterFaction)
                return; // friendly-fire guard — pass through without resolving

            health.TakeDamage(damage);
            _hasResolved = true;
        }
        else if (hitObject.CompareTag("Collide"))
        {
            // Hit environment geometry meant to stop the projectile — resolve here.
            _hasResolved = true;
        }
        else
        {
            return; // hit something irrelevant (floor, random collider) — pass through, keep flying
        }

        if (impactVFX != null)
        {
            GameObject vfx = Instantiate(impactVFX, hitPoint, Quaternion.identity);
            Destroy(vfx, 0.5f);
        }
        Destroy(gameObject);
    }
}
