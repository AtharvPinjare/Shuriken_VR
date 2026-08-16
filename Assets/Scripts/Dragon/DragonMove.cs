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

    [Header("Attack — beam (Stage 3, stretch goal; fireball above is the fallback)")]
    [SerializeField] bool useBeamAttack = false;
    [SerializeField] GameObject beamVfxPrefab;
    [SerializeField] float beamDuration = 1.8f; // matches FF_Laser01_Settings.laser_duration default
    [SerializeField] float beamDamagePerTick = 8f;
    [SerializeField] float beamTickInterval = 0.3f;
    [SerializeField] float beamHitRadius = 1.5f;
    // Empirically measured for VFX Laser Fire.prefab: mesh X-extent(6.98)*2 * startSizeX(8) *
    // the "Scale" parent node's own localScale(0.2) = world units of beam length per 1.0 of
    // FF_Laser01_Settings.length_multiplier. Re-measure if the prefab/VFX pack changes.
    [SerializeField] float beamWorldLengthPerMultiplierUnit = 22.336f;

    bool _beamActive;
    float _beamRemainingDuration;
    float _beamTickTimer;
    Vector3 _beamOrigin;
    Vector3 _beamDirection;
    float _beamLength;

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

        // Runs independently of the FSM switch below — a beam's damage window
        // outlives a single Update tick and shouldn't stop ticking just because
        // the state happens to change while it's still active.
        UpdateBeamDamage();

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
            if (useBeamAttack) FireBeam();
            else FireHomingFireball();
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

    // Beam VFX (Flashy Feather "VFX Laser Fire") is a fire-and-forget particle
    // burst with no continuous tracking and no hit-detection of its own — aim
    // and length are set once at cast time, and damage is scripted separately
    // via IsPlayerInBeamPath(), never physics (OVRCameraRig has no Collider —
    // see the Stage 2 fireball fix and the note in CLAUDE.md).
    void FireBeam()
    {
        if (beamVfxPrefab == null)
        {
            Debug.LogError($"{name}: DragonMove has no beamVfxPrefab assigned — cannot fire beam.", this);
            return;
        }

        Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
        Vector3 direction = (_playerTransform.position - spawnPos).normalized;
        float distance = Vector3.Distance(spawnPos, _playerTransform.position);

        GameObject beam = Instantiate(beamVfxPrefab, spawnPos, Quaternion.LookRotation(direction));
        if (beam.TryGetComponent(out ff_laser_animations_01.FF_Laser01_Settings settings) && settings.t_main_laser != null)
        {
            float lengthMultiplier = Mathf.Max(0.1f, distance / beamWorldLengthPerMultiplierUnit);
            Vector3 s = settings.t_main_laser.localScale;
            settings.t_main_laser.localScale = new Vector3(lengthMultiplier, s.y, s.z);
        }
        Destroy(beam, beamDuration + 0.2f); // DESTROY_ON_END defaults to false on this prefab

        _beamActive = true;
        _beamRemainingDuration = beamDuration;
        _beamTickTimer = 0f;
        _beamOrigin = spawnPos;
        _beamDirection = direction;
        _beamLength = distance;

        Debug.Log($"{name}: fired beam at player.");
    }

    void UpdateBeamDamage()
    {
        if (!_beamActive) return;

        _beamRemainingDuration -= Time.deltaTime;
        if (_beamRemainingDuration <= 0f)
        {
            _beamActive = false;
            return;
        }

        _beamTickTimer -= Time.deltaTime;
        if (_beamTickTimer > 0f) return;
        _beamTickTimer = beamTickInterval;

        if (IsPlayerInBeamPath())
        {
            _playerHealth?.TakeDamage(beamDamagePerTick);
            Debug.Log($"{name}: beam tick hit player for {beamDamagePerTick}.");
        }
    }

    bool IsPlayerInBeamPath()
    {
        Vector3 toPlayer = _playerTransform.position - _beamOrigin;
        float t = Mathf.Clamp(Vector3.Dot(toPlayer, _beamDirection), 0f, _beamLength);
        Vector3 closestPoint = _beamOrigin + _beamDirection * t;
        return Vector3.Distance(_playerTransform.position, closestPoint) <= beamHitRadius;
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
