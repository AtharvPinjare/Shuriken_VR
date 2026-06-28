using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;
    float _currentHealth;

    public bool IsDead { get; private set; }
    public UnityEvent OnDeath;

    void Awake() => _currentHealth = maxHealth;

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        _currentHealth -= amount;
        Debug.Log($"{gameObject.name} HP: {_currentHealth}");

        if (_currentHealth <= 0f)
        {
            IsDead = true;
            OnDeath.Invoke();
        }
    }
}
