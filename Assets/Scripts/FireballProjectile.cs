using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private AudioClip hitExplosionClip;

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

        if (_data.effectOnHit != null)
            _data.effectOnHit.Apply(collision.gameObject);

        if (_data.ImpactPrefabVFX != null)
            Instantiate(_data.ImpactPrefabVFX, collision.contacts[0].point, Quaternion.identity);

        if (hitExplosionClip != null)
            AudioSource.PlayClipAtPoint(hitExplosionClip, collision.contacts[0].point);

        Destroy(gameObject);
    }
}
