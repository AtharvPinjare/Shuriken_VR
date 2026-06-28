using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    private SpellData _data;

    public void Initialize(SpellData data)
    {
        _data = data;
    }

    private void Start()
    {
        Destroy(gameObject, 20f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Fireball] Hit: {collision.gameObject.name}" +
                  $" | Damage: {_data?.damage}");

        if (collision.gameObject.TryGetComponent(out Health health))
            health.TakeDamage(_data.damage);

        if (_data.ImpactPrefabVFX != null)
            Instantiate(_data.ImpactPrefabVFX, collision.contacts[0].point, Quaternion.identity);

        Destroy(gameObject);
    }
}
