using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerHitSound : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;   // dedicated SFX source, NOT the music one
    [SerializeField] private AudioClip[] hitSounds;    // drag one or more "uhh" clips here
    [SerializeField] private float triggerDelay = 0.15f; // match the red flash delay

    void Awake()
    {
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();
    }

    // Hook this to Health -> OnDamaged in the Inspector
    public void PlayHit()
    {
        if (hitSounds == null || hitSounds.Length == 0 || sfxSource == null) return;
        Invoke(nameof(PlayNow), triggerDelay);
    }

    private void PlayNow()
    {
        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
        sfxSource.PlayOneShot(clip,5.0f);
    }
}