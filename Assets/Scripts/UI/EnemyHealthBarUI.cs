using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    private Slider _slider;
    private Health _health;

    private void Awake()
    {
        _slider = GetComponentInChildren<Slider>();
        _health = GetComponentInParent<Health>();

        if (_slider == null) Debug.LogError($"{name}: EnemyHealthBarUI found no Slider in children.");
        if (_health == null) Debug.LogError($"{name}: EnemyHealthBarUI found no Health in parents.");
    }

    private void OnEnable()
    {
        if (_health == null) return;
        _health.OnDamaged.AddListener(UpdateBar);
        UpdateBar(); // sync to full health immediately on spawn, don't wait for first hit
    }

    private void OnDisable()
    {
        if (_health == null) return;
        _health.OnDamaged.RemoveListener(UpdateBar);
    }

    private void UpdateBar()
    {
        if (_slider == null) return;
        _slider.value = _health._currentHealth / _health.maxHealth;
    }
}