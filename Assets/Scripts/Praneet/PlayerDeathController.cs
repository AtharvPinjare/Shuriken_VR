using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathController : MonoBehaviour
{
    [SerializeField] private float restartDelay = 2f;
    [SerializeField] private AudioSource voiceSource;   // dedicated AudioSource for the death line
    [SerializeField] private AudioClip deathLineClip;    // your "ah shit, here we go again" clip

    // Hook this to GameManager -> OnDefeat in the Inspector
    public void OnPlayerDefeat()
    {
        if (voiceSource != null && deathLineClip != null)
        {
            voiceSource.clip = deathLineClip;
            voiceSource.volume = 3.0f;
            voiceSource.Play();
        }

        Invoke(nameof(RestartScene), restartDelay);
    }

    private void RestartScene()
    {
        SFader.Instance.FadeToScene(SceneManager.GetActiveScene().name);
    }
}