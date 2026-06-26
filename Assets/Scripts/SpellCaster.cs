using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private SpellData fireballData;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private AudioSource castAudioSource;
    [SerializeField] private float castCooldown = 1f;
    [SerializeField] private GestureManager _gestureManager;

    private float _lastCastTime; 

    private void OnEnable()
    {
        _gestureManager.OnFireballGesture += HandleFireballGesture;
    }

    private void OnDisable()
    {
        _gestureManager.OnFireballGesture -= HandleFireballGesture;
    }

    public void HandleFireballGesture()  
    {
        if (Time.time - _lastCastTime < castCooldown) return;
        _lastCastTime = Time.time;
        if (fireballData != null) CastSpell(fireballData);
    }

    private void CastSpell(SpellData data)
    {
        if (data.projectilePrefab == null) return;

        Vector3 direction = Camera.main.transform.forward;
        Vector3 spawnPos = spawnPoint.position + direction * 0.3f;

        GameObject proj = Instantiate(
            data.projectilePrefab,
            spawnPos,
            Quaternion.LookRotation(direction)
        );

        if (proj.TryGetComponent(out FireballProjectile fp))
            fp.Initialize(data);

        if (proj.TryGetComponent(out Rigidbody rb))
            rb.AddForce(direction * data.projectileSpeed,
                        ForceMode.VelocityChange);

        if (castAudioSource != null && data.castClip != null)
            castAudioSource.PlayOneShot(data.castClip);
    }
}


