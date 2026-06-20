using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    private SpellData _data;
    // [SerializeField] private GameObject particles;

    public void Initialize(SpellData data)
    {
        _data = data;
    }

    // private void SpawnPaticleSystem(GameObject ParticleSystem)
    // {
    //     Instantiate(particles, gameObject.transform.position, gameObject.transform.rotation);
    // }

    private void Start()
    {
        Destroy(gameObject, 20f); // failsafe if it never hits
    }

    private void OnCollisionEnter(Collision collision)
    {
        // SpawnPaticleSystem(particles);
        Instantiate(_data.ImpactPrefabVFX, collision.contacts[0].point, Quaternion.identity);
        Destroy(gameObject);
    }
}