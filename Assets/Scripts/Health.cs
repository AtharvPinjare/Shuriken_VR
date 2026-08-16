using UnityEngine;
using UnityEngine.Events;


public class Health : MonoBehaviour
{
    // Generic faction guard for projectile friendly-fire checks (Dragon homing
    // fireball, and reused by the ranged enemy). Defaults to Enemy since most
    // Health components in the project are enemies — the player's Health is the
    // one instance that must be explicitly set to Player in the Inspector.
    public enum Faction { Player, Enemy }

    [SerializeField] public float maxHealth = 100f;
    public float _currentHealth;
    public bool IsDead { get; private set; }
    public UnityEvent OnDeath;
    public UnityEvent OnDamaged;
    [SerializeField] public Faction faction = Faction.Enemy;

    void Awake() => _currentHealth = maxHealth;

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        _currentHealth -= amount;
        Debug.Log($"{gameObject.GetInstanceID()} HP: {_currentHealth}");
        OnDamaged.Invoke();

        if (_currentHealth <= 0f)
        {
            IsDead = true;
            OnDeath.Invoke();
        }
    }

    public void OnEntityDeath(string Entity)
    {
        Debug.Log($"{Entity} died!");
    }
}
