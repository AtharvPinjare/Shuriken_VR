using UnityEngine;

public class IntroNarration : MonoBehaviour
{
    [SerializeField] private AudioSource voiceSource;   // dedicated AudioSource for this clip
    [SerializeField] private AudioClip introClip;        // your generated welcome/tutorial voice line
    [SerializeField] private float startDelay = 3.5f;    // seconds after scene load before it plays

    void Start()
    {
        Invoke(nameof(PlayIntro), startDelay);
    }

    private void PlayIntro()
    {
        if (voiceSource == null || introClip == null)
        {
            Debug.LogWarning("[IntroNarration] Voice Source or Intro Clip not assigned.");
            return;
        }

        voiceSource.clip = introClip;
        voiceSource.Play();
    }
}