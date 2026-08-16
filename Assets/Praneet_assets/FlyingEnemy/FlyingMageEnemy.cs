using UnityEngine;

public class FlyingMageEnemy : MonoBehaviour
{
    [Header("Targeting & Animation")]
    public Animator animator;

    [Header("Flight Mechanics")]
    public float floatSpeed = 2f;
    public float floatAmplitude = 1f;
    public float initialHoverHeight = 3f;
    private Vector3 basePos;

    [Header("Chase / Range Keeping")]
    public float preferredRange = 8f;
    public float rangeTolerance = 0.5f;
    public float moveSpeed = 3f;

    [Header("Combat Mechanics")]
    public GameObject spellPrefab;
    public Transform castPoint;
    public float fireRate = 3f;
    public float projectileSpeed = 15f;
    private float nextFireTime;

    [Header("Death VFX")]
    [Tooltip("Placeholder VFX prefab spawned at the enemy's position when it dies.")]
    public GameObject deathVFXPrefab;

    private bool isDead = false;

    // Injected by WaveSpawner right after Instantiate — do NOT expose these
    // in the Inspector. Prefab assets cannot hold scene references.
    private Transform _playerTransform;
    private Health _playerHealth;

    public void InjectPlayerReferences(Transform playerTransform, Health playerHealth)
    {
        _playerTransform = playerTransform;
        _playerHealth = playerHealth;
    }

    void Start()
    {
        basePos = transform.position + Vector3.up * initialHoverHeight;
    }

    void Update()
    {
        if (isDead) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (_playerTransform == null) return;

        Vector3 toPlayer = transform.position - _playerTransform.position;
        Vector3 toPlayerFlat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float currentDistance = toPlayerFlat.magnitude;

        Vector3 desiredPos = transform.position;

        if (currentDistance > preferredRange + rangeTolerance)
        {
            Vector3 dirToPlayer = -toPlayerFlat.normalized;
            desiredPos += dirToPlayer * moveSpeed * Time.deltaTime;
        }
        else if (currentDistance < preferredRange - rangeTolerance)
        {
            Vector3 dirAwayFromPlayer = toPlayerFlat.normalized;
            desiredPos += dirAwayFromPlayer * moveSpeed * Time.deltaTime;
        }

        basePos = new Vector3(desiredPos.x, basePos.y, desiredPos.z);

        float newY = basePos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(basePos.x, newY, basePos.z);

        Vector3 directionToPlayer = _playerTransform.position - transform.position;
        directionToPlayer.y = 0;
        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        if (Time.time >= nextFireTime)
        {
            if (animator != null)
                animator.SetTrigger("Cast");
            SpawnProjectile();
            nextFireTime = Time.time + fireRate;
        }
    }

    public void SpawnProjectile()
    {
        if (spellPrefab == null || castPoint == null || _playerTransform == null) return;

        Vector3 direction = (_playerTransform.position - castPoint.position).normalized;
        Quaternion aimRotation = Quaternion.LookRotation(direction);

        GameObject proj = Instantiate(spellPrefab, castPoint.position, aimRotation);

        if (proj.TryGetComponent(out Rigidbody rb))
            rb.linearVelocity = direction * projectileSpeed;

        // The player has no Collider anywhere in its hierarchy, so the
        // projectile can't rely on OnCollisionEnter/OnTriggerEnter to hit it —
        // give it a Transform to proximity-check instead.
        if (proj.TryGetComponent(out EnemySpellProjectile spell))
            spell.target = _playerTransform;
    }



    // Call this from Health.OnDeath() when this enemy's health reaches 0
    public void TakeDamage()
    {
        if (isDead) return;
        isDead = true;

        if (deathVFXPrefab != null)
        {
            GameObject vfx = Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f); // adjust duration to match the effect's actual length
        }

        if (animator != null && animator.layerCount > 1)
            animator.SetLayerWeight(1, 0f);

        if (animator != null)
            animator.SetTrigger("Die");

        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        Destroy(gameObject, 3f);
    }

}
