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
    NavMeshAgent _agent;
    Health _playerHealth;
    float _attackTimer;
    [SerializeField]EnemyState _currentState = EnemyState.Idle;
    
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _playerHealth = playerTransform.GetComponent<Health>();
        GetComponent<Health>().OnDeath.AddListener(OnDeath);
    }


    void Update()
    {
        Debug.Log(_currentState);
        switch (_currentState)
        {
            case EnemyState.Idle:
                _agent.ResetPath();
                if (DistanceToPlayer() < detectionRange)
                    _currentState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                _agent.SetDestination(playerTransform.position);
                if (DistanceToPlayer() < attackRange)
                    _currentState = EnemyState.Attack;
                else if (DistanceToPlayer() > loseRange)
                    _currentState = EnemyState.Idle;
                break;

            case EnemyState.Attack:
                _agent.ResetPath();
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    _attackTimer = attackCooldown;
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
        Destroy(gameObject, 2f);
    }


    float DistanceToPlayer() =>
        Vector3.Distance(transform.position, playerTransform.position);
}
