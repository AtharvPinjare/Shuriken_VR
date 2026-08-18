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

    [Header("Spawn Positioning")]
    public Transform playerTransform;         // drag CenterEyeAnchor or player root here
    public float spawnDistance = 20f;

    [Header("Content")]
    [TextArea]
    public string subtitleMessage = "You have successfully completed the level. You can now proceed.";

    [Header("Scene Transition")]
    public string mainMenuSceneName = "MainMenu";
    [Tooltip("Delay in seconds after the voice line ends before loading the Main Menu.")]
    public float delayBeforeSceneLoad = 1f;

    [Header("Debug / State")]
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

        PositionInFrontOfPlayer();   // NEW — happens before activating anything

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

    // NEW — spawns the character 20m ahead of wherever the player is facing at trigger time
    private void PositionInFrontOfPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("[LevelCompleteController] Player Transform not assigned — skipping reposition.");
            return;
        }

        Vector3 flatForward = playerTransform.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        transform.position = playerTransform.position + flatForward * spawnDistance;

        // Face the character back toward the player
        Vector3 lookDir = playerTransform.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);
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