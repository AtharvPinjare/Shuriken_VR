using UnityEngine;

public class WaveStartAudio : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip[] waveStartClips;

    public void PlayWaveStartSound(int waveNumber)
    {
        if (sfxSource == null || waveStartClips == null || waveStartClips.Length == 0)
        {
            Debug.LogWarning("[WaveStartAudio] No clips assigned — skipping sound.");
            return;
        }

        int index = waveNumber - 1;
        AudioClip clip;

        if (index >= 0 && index < waveStartClips.Length && waveStartClips[index] != null)
            clip = waveStartClips[index];
        else
            clip = waveStartClips[Random.Range(0, waveStartClips.Length)];

        if (clip != null)
            sfxSource.PlayOneShot(clip, 10.0f);
    }
}