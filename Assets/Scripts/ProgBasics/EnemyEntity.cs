using UnityEngine;

public class EnemyEntity : BaseEntity
{
    [SerializeField] private int scoreValue = 10;

    private void OnMouseDown()
    {
        health.TakeDamage(34f);
    }

    public override void OnDeath()
    {
        Debug.Log(entityName + " died! +" + scoreValue + " points");
        GameManager.Instance.AddScore(scoreValue);
        gameObject.SetActive(false);
    }
}