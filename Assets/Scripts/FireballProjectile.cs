using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private AudioClip hitExplosionClip;

    // Homing — additive, off by default. Set isHoming+target on a spawned
    // instance (e.g. Dragon Stage 2's DragonFireball variant) to enable.
    public bool isHoming = false;
    public float turnRateDegreesPerSecond = 90f;
    public Transform target;

    // Homing targets (e.g. the player rig) may have no Collider at all — every
    // existing damage source hits the player via a direct Health.TakeDamage()
    // call, never physics, so OnCollisionEnter would simply never fire against
    // it. Resolve homing hits by proximity instead once within this radius.
    [SerializeField] private float homingHitRadius = 1f;

    // Shooter-exclusion guard — generic faction check, reused by the ranged
    // enemy's friendly-fire guard (Item 6). Skips damage only; does not
    // prevent the physical collision response (would need a Project Settings
    // physics layer matrix change, which is red-zone).
    public Health.Faction shooterFaction = Health.Faction.Player;

    private SpellData _data;
    private Rigidbody _rb;
    private bool _hasResolved;

    public void Initialize(SpellData data)
    {
        _data = data;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, 20f);
    }

    private void FixedUpdate()
    {
        if (!isHoming || target == null || _rb == null || _hasResolved) return;

        Vector3 currentVelocity = _rb.linearVelocity;
        float speed = currentVelocity.magnitude;
        if (speed < 0.01f) return;

        Vector3 desiredDirection = (target.position - transform.position).normalized;
        Vector3 newDirection = Vector3.RotateTowards(
            currentVelocity.normalized,
            desiredDirection,
            turnRateDegreesPerSecond * Mathf.Deg2Rad * Time.fixedDeltaTime,
            0f);

        _rb.linearVelocity = newDirection * speed;
        transform.rotation = Quaternion.LookRotation(newDirection);

        if (Vector3.Distance(transform.position, target.position) <= homingHitRadius)
            ResolveHit(target.gameObject, transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ResolveHit(collision.gameObject, collision.contacts[0].point);
    }

    private void ResolveHit(GameObject hitObject, Vector3 hitPoint)
    {
        if (_hasResolved) return;

        if (hitObject.TryGetComponent(out Health health))
        {
            if (health.faction == shooterFaction)
                return; // friendly-fire guard — pass through without damaging, exploding, or being destroyed

            health.TakeDamage(_data.damage);
        }

        _hasResolved = true;

        Debug.Log($"[Fireball] Hit: {hitObject.name}" +
                  $" | Damage: {_data?.damage}");

        if (_data.effectOnHit != null)
            _data.effectOnHit.Apply(hitObject);

        if (_data.ImpactPrefabVFX != null)
            Instantiate(_data.ImpactPrefabVFX, hitPoint, Quaternion.identity);

        if (hitExplosionClip != null)
            PlayHitExplosion();

        Destroy(gameObject);
    }

    private void PlayHitExplosion()
    {
        GameObject audioObject = new GameObject("Fireball Hit Explosion SFX");
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.5f;
        audioSource.PlayOneShot(hitExplosionClip);

        Destroy(audioObject, hitExplosionClip.length);
    }
}
