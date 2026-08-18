using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Health targetHealth;   // drag Player's Health component here
    [SerializeField] private Image fillImage;        // drag "Fill" image here (Image Type: Filled)

    void Start()
    {
        if (targetHealth == null)
        {
            Debug.LogWarning("HealthBarUI: targetHealth not assigned.");
            return;
        }

        UpdateBar();
    }

    // Hook this to Health -> OnDamaged in the Inspector
    public void UpdateBar()
    {
        if (targetHealth == null || fillImage == null) return;
        fillImage.fillAmount = targetHealth._currentHealth / targetHealth.maxHealth;
    }
}