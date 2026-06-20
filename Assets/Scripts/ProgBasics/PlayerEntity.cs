using UnityEngine;

public class PlayerEntity : BaseEntity
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            health.TakeDamage(15f);

        if (Input.GetKeyDown(KeyCode.H))
            health.Heal(20f);
    }

    public override void OnDeath()
    {
        Debug.Log(entityName + " died! Respawning...");
    }
}