using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteController : MonoBehaviour
{
    [Header("References")]
    public GameObject characterModel;
    public AudioSource voiceAudioSource;
    public AudioClip voiceLineClip;
    public GameObject subtitleCanvas;
    public TMPro.TMP_Text subtitleText;

    [Header("Content")]
    [TextArea]
    public string subtitleMessage = "You have successfully completed the level. You can now proceed.";

    [Header("Scene Transition")]
    public string mainMenuSceneName = "MainMenu";
    [Tooltip("Delay in seconds after the voice line ends before loading the Main Menu.")]
    public float delayBeforeSceneLoad = 1f;

    [Header("Debug / State")]
    [Tooltip("For manual testing only: check this in Play mode to fire the " +
             "sequence without calling TriggerLevelComplete() from code.")]
    [SerializeField] private bool debugTriggerNow = false;

    [SerializeField] private bool hasTriggered = false;

    private void Update()
    {
        if (debugTriggerNow && !hasTriggered)
        {
            TriggerLevelComplete();
        }
    }

    public void TriggerLevelComplete()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        gameObject.SetActive(true);

        if (characterModel != null)
            characterModel.SetActive(true);

        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(true);

        if (subtitleText != null)
            subtitleText.text = subtitleMessage;

        if (voiceAudioSource != null && voiceLineClip != null)
        {
            voiceAudioSource.clip = voiceLineClip;
            voiceAudioSource.Play();

            float totalDelay = voiceLineClip.length + delayBeforeSceneLoad;
            Invoke(nameof(EndSequenceAndLoadMenu), totalDelay);
        }
        else
        {
            Debug.LogWarning("[LevelCompleteController] Voice AudioSource or Clip not assigned — no audio will play. " +
                              "Falling back to delayBeforeSceneLoad only.");
            Invoke(nameof(EndSequenceAndLoadMenu), delayBeforeSceneLoad);
        }
    }

    private void EndSequenceAndLoadMenu()
    {
        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(false);

        if (characterModel != null)
            characterModel.SetActive(false);

        gameObject.SetActive(false);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ResetSequence()
    {
        hasTriggered = false;
        debugTriggerNow = false;
    }
}