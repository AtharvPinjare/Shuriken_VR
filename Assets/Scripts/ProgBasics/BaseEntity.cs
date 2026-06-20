using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    [SerializeField] protected string entityName = "Unnamed";
    protected HealthComponent health;

    protected virtual void Start()
    {
        health = GetComponent<HealthComponent>();

        if (health == null)
            Debug.LogWarning(entityName + " is missing a HealthComponent!");
    }

    public virtual void OnDeath()
    {
        Debug.Log(entityName + " base OnDeath called.");
    }
}