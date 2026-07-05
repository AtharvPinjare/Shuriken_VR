using UnityEngine;
using UnityEngine.AI;


public enum EnemyState { Idle, Chase, Attack, Dead }


public class EnemyMove : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform playerTransform;


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
    Health _playerHealth;
    Animator _animator;
    Renderer _renderer;
    Color _originalColor;
    float _attackTimer;

    [SerializeField] EnemyState _currentState = EnemyState.Idle;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _playerHealth = playerTransform.GetComponent<Health>();


        // GetComponentInChildren checks self first, then children — safe
        // regardless of whether Animator sits on this GameObject or on
        // the nested Mutant model.
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
        switch (_currentState)
        {
            case EnemyState.Idle:
                _agent.ResetPath();
                _animator.SetFloat("MoveSpeed", 0f);
                if (DistanceToPlayer() < detectionRange)
                    _currentState = EnemyState.Chase;
                break;


            case EnemyState.Chase:
                _agent.SetDestination(playerTransform.position);
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
        Vector3.Distance(transform.position, playerTransform.position);
}
