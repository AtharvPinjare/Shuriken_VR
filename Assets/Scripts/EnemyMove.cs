using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Chase, Attack, Dead }

public class EnemyMove : MonoBehaviour
{
    [Header("Ranges")]
    [SerializeField] float detectionRange = 8f;
    [SerializeField] float loseRange = 10f;
    [SerializeField] float attackRange = 2f;

    [Header("Attack")]
    [SerializeField] float attackDamage = 10f;
    [SerializeField] float attackCooldown = 2f;

    [Header("Death")]
    [SerializeField] float deathDestroyDelay = 2f; // match this to "Mutant Dying" clip length

    NavMeshAgent _agent;
    Animator _animator;
    Renderer _renderer;
    Color _originalColor;
    float _attackTimer;

    // Injected by WaveSpawner right after Instantiate — do NOT expose these
    // in the Inspector. Prefab assets cannot hold scene references, which is
    // exactly why these were coming back None on the Mutant prefab.
    Transform _playerTransform;
    Health _playerHealth;

    [SerializeField] EnemyState _currentState = EnemyState.Idle;

    public void InjectPlayerReferences(Transform playerTransform, Health playerHealth)
    {
        _playerTransform = playerTransform;
        _playerHealth = playerHealth;
    }

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

        _animator = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
            _originalColor = _renderer.material.color;

        var health = GetComponent<Health>();
        health.OnDeath.AddListener(OnDeath);
        health.OnDamaged.AddListener(FlashDamage);
    }

    void Update()
    {
        // Guards against a frame-ordering edge case: if Start() runs before
        // WaveSpawner's InjectPlayerReferences() call lands, this stays Idle
        // and non-crashing instead of throwing on a null _playerTransform.
        if (_playerTransform == null)
            return;

        switch (_currentState)
        {
            case EnemyState.Idle:
                _agent.ResetPath();
                _animator.SetFloat("MoveSpeed", 0f);
                if (DistanceToPlayer() < detectionRange)
                    _currentState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                _agent.SetDestination(_playerTransform.position);
                _animator.SetFloat("MoveSpeed", _agent.velocity.magnitude);
                if (DistanceToPlayer() < attackRange)
                    _currentState = EnemyState.Attack;
                else if (DistanceToPlayer() > loseRange)
                    _currentState = EnemyState.Idle;
                break;

            case EnemyState.Attack:
                _agent.ResetPath();
                _animator.SetFloat("MoveSpeed", 0f);
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    _attackTimer = attackCooldown;
                    _animator.SetTrigger("Attack");
                    _playerHealth?.TakeDamage(attackDamage);
                }
                if (DistanceToPlayer() > attackRange)
                    _currentState = EnemyState.Chase;
                break;

            case EnemyState.Dead:
                break;
        }
    }

    void OnDeath()
    {
        _currentState = EnemyState.Dead;
        _agent.enabled = false;
        _animator.SetBool("IsDead", true);
        _animator.SetTrigger("Die");
        Destroy(gameObject, deathDestroyDelay);
    }

    void FlashDamage()
    {
        if (_renderer != null)
            StartCoroutine(DamageFlashCoroutine());
    }

    System.Collections.IEnumerator DamageFlashCoroutine()
    {
        _renderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _renderer.material.color = _originalColor;
    }

    float DistanceToPlayer() =>
        Vector3.Distance(transform.position, _playerTransform.position);
}
