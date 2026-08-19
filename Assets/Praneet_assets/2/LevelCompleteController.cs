using UnityEngine;

public class LevelCompleteController : MonoBehaviour
{
    [Header("References")]
    public GameObject characterModel;
    public AudioSource voiceAudioSource;
    public AudioClip voiceLineClip;

    public GameObject subtitleCanvas;
    public TMPro.TMP_Text subtitleText;

    [Header("Content")]
    [TextArea(2, 5)]
    public string subtitleMessage =
        "You have successfully completed the level. You can now proceed.";

    [Header("Scene Transition")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Delay after the voice line finishes before returning to Main Menu.")]
    public float delayBeforeSceneLoad = 1f;

    [Header("Debug / State")]
    [SerializeField] private bool playOnSceneStart = true;
    [SerializeField] private bool debugTriggerNow = false;

    private bool hasTriggered = false;

    private void Start()
    {
        // Hide the ending elements initially.
        if (characterModel != null)
            characterModel.SetActive(false);

        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(false);

        // Automatically start when FinalScene loads.
        if (playOnSceneStart)
        {
            TriggerLevelComplete();
        }
    }

    private void Update()
    {
        // Optional manual testing.
        if (debugTriggerNow && !hasTriggered)
        {
            TriggerLevelComplete();
        }
    }

    public void TriggerLevelComplete()
    {
        if (hasTriggered)
            return;

        hasTriggered = true;

        Debug.Log(
            "[LevelCompleteController] Level complete sequence started."
        );

        // ---------------------------------
        // SHOW CHARACTER
        // ---------------------------------

        if (characterModel != null)
            characterModel.SetActive(true);

        // ---------------------------------
        // SHOW SUBTITLES
        // ---------------------------------

        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(true);

        if (subtitleText != null)
            subtitleText.text = subtitleMessage;

        // ---------------------------------
        // PLAY VOICE
        // ---------------------------------

        if (voiceAudioSource != null &&
            voiceLineClip != null)
        {
            voiceAudioSource.clip = voiceLineClip;

            voiceAudioSource.Play();

            float totalDelay =
                voiceLineClip.length +
                delayBeforeSceneLoad;

            Invoke(
                nameof(ReturnToMainMenu),
                totalDelay
            );
        }
        else
        {
            Debug.LogWarning(
                "[LevelCompleteController] " +
                "Voice AudioSource or Voice Clip is missing."
            );

            Invoke(
                nameof(ReturnToMainMenu),
                delayBeforeSceneLoad
            );
        }
    }

    // ---------------------------------
    // RETURN TO MAIN MENU
    // ---------------------------------

    private void ReturnToMainMenu()
    {
        Debug.Log(
            "[LevelCompleteController] Returning to Main Menu."
        );

        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(false);

        if (characterModel != null)
            characterModel.SetActive(false);

        if (SFader.Instance != null)
        {
            SFader.Instance.FadeToScene(
                mainMenuSceneName
            );
        }
        else
        {
            Debug.LogWarning(
                "[LevelCompleteController] " +
                "SFader not found. Loading Main Menu directly."
            );

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                mainMenuSceneName
            );
        }
    }

    // ---------------------------------
    // RESET
    // ---------------------------------

    public void ResetSequence()
    {
        CancelInvoke(
            nameof(ReturnToMainMenu)
        );

        hasTriggered = false;
        debugTriggerNow = false;
    }
}   