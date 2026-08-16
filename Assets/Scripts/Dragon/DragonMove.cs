using UnityEngine;
using UnityEngine.Events;

public enum DragonState { Idle, Chase, Attack, Dead }

public class DragonMove : MonoBehaviour
{
    [Header("Volumes (scene objects, assign in Inspector — never searched at runtime)")]
    [SerializeField] Collider loiterVolume;
    [SerializeField] Collider engageTrigger;

    [Header("Idle — ambient loiter")]
    [SerializeField] float loiterSpeed = 2f;
    [SerializeField] float loiterPointReachedThreshold = 1f;
    [SerializeField] float loiterPointHoldTime = 2f;

    [Header("Chase — altitude band (VR comfort: never dives to eye level)")]
    [SerializeField] float chaseSpeed = 6f;
    [SerializeField] float minAltitudeAbovePlayer = 3f;
    [SerializeField] float maxAltitudeAbovePlayer = 6f;
    [SerializeField] float altitudeArrivalTolerance = 1f;
    [SerializeField] float rotationSpeed = 90f;

    [Header("Attack — homing fireball")]
    [SerializeField] float attackRange = 8f;
    [SerializeField] float attackExitRange = 10f;
    [SerializeField] float attackTelegraphDelay = 1f;
    [SerializeField] float attackCooldown = 2.5f;
    [SerializeField] SpellData dragonFireballData;
    [SerializeField] Transform fireballSpawnPoint;

    public UnityEvent OnDragonDefeated;

    Animator _animator;
    Health _health;

    // Injected by GameManager at Start — do NOT expose in the Inspector, same
    // reasoning as EnemyMove: prefab assets can't hold scene references.
    Transform _playerTransform;
    Health _playerHealth;

    [SerializeField] DragonState _currentState = DragonState.Idle;
    DragonState _previousState;
    bool _hasEnteredState;

    Vector3 _currentLoiterTarget;
    float _loiterHoldTimer;
    float _attackTimer;
    bool _isTelegraphing;

    public void InjectPlayerReferences(Transform playerTransform, Health playerHealth)
    {
        _playerTransform = playerTransform;
        _playerHealth = playerHealth;
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
    }

    void Start()
    {
        if (_health != null)
            _health.OnDeath.AddListener(OnDeath);
        else
            Debug.LogError($"{name}: DragonMove requires a Health component (Assets/Scripts/Health.cs) on the same GameObject to detect death.", this);

        if (loiterVolume == null)
            Debug.LogError($"{name}: DragonMove has no loiterVolume assigned — ambient loiter will not move.", this);
        if (engageTrigger == null)
            Debug.LogError($"{name}: DragonMove has no engageTrigger assigned — Chase will never trigger.", this);

        PickNewLoiterTarget();
    }

    void Update()
    {
        if (_currentState == DragonState.Dead) return;

        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        // Guards against a frame-ordering edge case: if Start() runs before
        // GameManager's InjectPlayerReferences() call lands, this stays Idle
        // and non-crashing instead of throwing on a null _playerTransform.
        if (_playerTransform == null) return;

        switch (_currentState)
        {
            case DragonState.Idle:
                EnterStateIfChanged(DragonState.Idle);
                UpdateLoiter();
                if (engageTrigger != null && engageTrigger.bounds.Contains(_playerTransform.position))
                    _currentState = DragonState.Chase;
                break;

            case DragonState.Chase:
                EnterStateIfChanged(DragonState.Chase);
                UpdateChase();
                if (HorizontalDistanceToPlayer() < attackRange && HasReachedAltitudeBand())
                    _currentState = DragonState.Attack;
                break;

            case DragonState.Attack:
                EnterStateIfChanged(DragonState.Attack);
                UpdateAttack();
                if (HorizontalDistanceToPlayer() > attackExitRange)
                    _currentState = DragonState.Chase;
                break;
        }
    }

    void EnterStateIfChanged(DragonState state)
    {
        if (_hasEnteredState && _previousState == state) return;
        _hasEnteredState = true;
        _previousState = state;

        // A trigger only auto-clears once its transition is actually taken. If a
        // trigger fires while its own destination is already the current state
        // (blocked by canTransitionToSelf=false), it stays armed forever and can
        // steal priority from a later, legitimate trigger. Reset all of them
        // before arming the one for the state we're actually entering.
        ResetAllAnimatorTriggers();

        switch (state)
        {
            case DragonState.Idle:
                _animator.SetTrigger("TriggerLoiter");
                break;
            case DragonState.Chase:
                _animator.SetTrigger("TriggerChase");
                break;
            case DragonState.Attack:
                _animator.SetTrigger("TriggerAttack");
                _isTelegraphing = true;
                _attackTimer = attackTelegraphDelay;
                break;
        }
    }

    // Telegraph -> fire -> cooldown -> telegraph, repeating for as long as the
    // player stays in Attack range. Every shot gets its own telegraph delay —
    // non-negotiable VR fairness requirement, not just a one-time wind-up.
    void UpdateAttack()
    {
        _attackTimer -= Time.deltaTime;
        if (_attackTimer > 0f) return;

        if (_isTelegraphing)
        {
            FireHomingFireball();
            _isTelegraphing = false;
            _attackTimer = attackCooldown;
        }
        else
        {
            _isTelegraphing = true;
            _attackTimer = attackTelegraphDelay;
        }
    }

    void FireHomingFireball()
    {
        if (dragonFireballData == null || dragonFireballData.projectilePrefab == null)
        {
            Debug.LogError($"{name}: DragonMove has no dragonFireballData (or its projectilePrefab) assigned — cannot fire.", this);
            return;
        }

        Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
        Vector3 direction = (_playerTransform.position - spawnPos).normalized;

        GameObject proj = Instantiate(dragonFireballData.projectilePrefab, spawnPos, Quaternion.LookRotation(direction));

        if (proj.TryGetComponent(out FireballProjectile fp))
        {
            fp.Initialize(dragonFireballData);
            fp.target = _playerTransform;
        }

        if (proj.TryGetComponent(out Rigidbody rb))
            rb.AddForce(direction * dragonFireballData.projectileSpeed, ForceMode.VelocityChange);

        Debug.Log($"{name}: fired homing fireball at player.");
    }

    void ResetAllAnimatorTriggers()
    {
        _animator.ResetTrigger("TriggerLoiter");
        _animator.ResetTrigger("TriggerChase");
        _animator.ResetTrigger("TriggerAttack");
        _animator.ResetTrigger("TriggerDead");
    }

    void UpdateLoiter()
    {
        if (loiterVolume == null) return;

        _loiterHoldTimer -= Time.deltaTime;
        Vector3 toTarget = _currentLoiterTarget - transform.position;
        if (toTarget.magnitude < loiterPointReachedThreshold && _loiterHoldTimer <= 0f)
        {
            PickNewLoiterTarget();
            return;
        }
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _currentLoiterTarget, loiterSpeed * Time.deltaTime);
            FaceDirection(toTarget);
        }
    }

    void PickNewLoiterTarget()
    {
        if (loiterVolume == null) return;

        Bounds b = loiterVolume.bounds;
        _currentLoiterTarget = new Vector3(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y),
            Random.Range(b.min.z, b.max.z));
        _loiterHoldTimer = loiterPointHoldTime;
    }

    void UpdateChase()
    {
        float targetY = Mathf.Clamp(
            transform.position.y,
            _playerTransform.position.y + minAltitudeAbovePlayer,
            _playerTransform.position.y + maxAltitudeAbovePlayer);
        Vector3 targetPos = new Vector3(_playerTransform.position.x, targetY, _playerTransform.position.z);

        transform.position = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
        FaceDirection(targetPos - transform.position);
    }

    void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    float HorizontalDistanceToPlayer()
    {
        Vector3 delta = transform.position - _playerTransform.position;
        delta.y = 0f;
        return delta.magnitude;
    }

    // Attack must not trigger while the dragon is still mid-descent — horizontal
    // proximity alone isn't enough (a large vertical gap can otherwise be masked
    // by 3D distance, or ignored entirely by horizontal-only distance). Require
    // the dragon to have actually settled into its altitude band first.
    bool HasReachedAltitudeBand()
    {
        float heightAbovePlayer = transform.position.y - _playerTransform.position.y;
        return heightAbovePlayer >= minAltitudeAbovePlayer - altitudeArrivalTolerance
            && heightAbovePlayer <= maxAltitudeAbovePlayer + altitudeArrivalTolerance;
    }

    void OnDeath()
    {
        _currentState = DragonState.Dead;
        ResetAllAnimatorTriggers();
        _animator.SetTrigger("TriggerDead");
        OnDragonDefeated?.Invoke();
    }
}
