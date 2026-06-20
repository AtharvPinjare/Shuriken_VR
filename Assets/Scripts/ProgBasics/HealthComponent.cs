using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    public float CurrentHealth => _currentHealth;
    public bool IsDead => _currentHealth <= 0f;

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //         TakeDamage(10f);
    // }

    private void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        _currentHealth -= amount;
        _currentHealth = Mathf.Max(_currentHealth, 0f);
        Debug.Log(gameObject.name + " took " + amount + " damage. HP: " + _currentHealth);

        if (IsDead)
            Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        _currentHealth += amount;
        _currentHealth = Mathf.Min(_currentHealth, maxHealth);
        Debug.Log(gameObject.name + " healed " + amount + ". HP: " + _currentHealth);
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");
    } 
}