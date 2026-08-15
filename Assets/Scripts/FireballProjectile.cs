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
            PlayHitExplosion();

        Destroy(gameObject);
    }

    private void PlayHitExplosion()
    {
        GameObject audioObject = new GameObject("Fireball Hit Explosion SFX");
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.PlayOneShot(hitExplosionClip);

        Destroy(audioObject, hitExplosionClip.length);
    }
}
