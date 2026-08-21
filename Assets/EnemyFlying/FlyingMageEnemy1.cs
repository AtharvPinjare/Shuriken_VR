using UnityEngine;

public class FlyingMageEnemy1 : MonoBehaviour
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
    public GameObject deathVFXPrefab;

    private bool isDead = false;
    private bool isCasting = false;   // NEW — blocks movement/re-triggering mid-cast

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

        // Only move/reposition if not mid-cast — prevents the model sliding around while the cast animation plays
        if (!isCasting)
        {
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
        }

        float newY = basePos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(basePos.x, newY, basePos.z);

        Vector3 directionToPlayer = _playerTransform.position - transform.position;
        directionToPlayer.y = 0;
        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        if (!isCasting && Time.time >= nextFireTime)
        {
            BeginCast();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void BeginCast()
    {
        isCasting = true;

        if (animator != null)
            animator.SetTrigger("Cast");
        
    }

    // Call this from an Animation Event on the "Two hand Spell" clip, placed at the
    // exact frame where the hands/staff release the spell — NOT called directly from code anymore.
    public void SpawnProjectile()
{
    if (!isCasting)
    {
        Debug.LogWarning("SpawnProjectile called while enemy is NOT casting!");
        return;
    }

    if (spellPrefab == null || castPoint == null || _playerTransform == null)
        return;

    Transform aimTarget = _playerHealth != null
        ? _playerHealth.transform
        : _playerTransform;

    Vector3 direction =
        (aimTarget.position - castPoint.position).normalized;

    Quaternion aimRotation = Quaternion.LookRotation(direction);

    GameObject proj =
        Instantiate(spellPrefab, castPoint.position, aimRotation);

    if (proj.TryGetComponent(out Rigidbody rb))
        rb.linearVelocity = direction * projectileSpeed;

    if (proj.TryGetComponent(out EnemySpellProjectile spell))
        spell.target = aimTarget;
}

    // Call this from an Animation Event at the very END of the "Two hand Spell" clip,
    // so movement resumes exactly when the animation returns to idle.
    public void EndCast()
    {
        isCasting = false;
    }

    public void TakeDamage()
    {
        if (isDead) return;
        isDead = true;

        if (deathVFXPrefab != null)
        {
            GameObject vfx = Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
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