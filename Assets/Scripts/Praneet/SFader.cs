using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SFader : MonoBehaviour
{
    public static SFader Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("World Space Settings")]
    [SerializeField] private float distanceFromCamera = 0.5f;

    [Header("Scene Start")]
    [SerializeField] private bool fadeInOnStart = true;

    private bool isTransitioning = false;
    private Canvas fadeCanvas;

    private void Awake()
    {
        // Keep only one SFader in the entire game.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Make SFader survive scene changes.
        DontDestroyOnLoad(gameObject);

        if (fadeImage != null)
        {
            fadeCanvas = fadeImage.GetComponentInParent<Canvas>();
        }
    }

    private void Start()
    {
        if (fadeImage == null)
        {
            Debug.LogError(
                "[SFader] Fade Image is not assigned!",
                this
            );

            return;
        }

        if (fadeInOnStart)
        {
            // Start black.
            SetAlpha(1f);

            // Fade into the scene.
            StartCoroutine(FadeRoutine(1f, 0f));
        }
        else
        {
            SetAlpha(0f);
        }
    }

    private void LateUpdate()
    {
        FollowVRCamera();
    }

    // =========================================================
    // FOLLOW VR CAMERA
    // =========================================================

    private void FollowVRCamera()
    {
        if (fadeCanvas == null)
            return;

        Camera vrCamera = Camera.main;

        if (vrCamera == null)
            return;

        Transform cam = vrCamera.transform;

        // Put the canvas directly in front of the player's eyes.
        fadeCanvas.transform.position =
            cam.position + cam.forward * distanceFromCamera;

        // Make the canvas face exactly the same direction as the camera.
        fadeCanvas.transform.rotation =
            cam.rotation;
    }

    // =========================================================
    // PUBLIC FADE FUNCTIONS
    // =========================================================

    public void FadeOut()
    {
        if (fadeImage == null)
            return;

        StartCoroutine(
            FadeRoutine(0f, 1f)
        );
    }

    public void FadeIn()
    {
        if (fadeImage == null)
            return;

        StartCoroutine(
            FadeRoutine(1f, 0f)
        );
    }

    public void FadeToScene(string sceneName)
    {
        if (isTransitioning)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError(
                "[SFader] Scene name is empty."
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "[SFader] Scene '" +
                sceneName +
                "' cannot be loaded. " +
                "Make sure it is added to Build Settings."
            );

            return;
        }

        StartCoroutine(
            FadeToSceneRoutine(sceneName)
        );
    }

    // =========================================================
    // SCENE TRANSITION
    // =========================================================

    private IEnumerator FadeToSceneRoutine(string sceneName)
    {
        isTransitioning = true;

        Debug.Log(
            "[SFader] Fading to scene: " +
            sceneName
        );

        // -----------------------------
        // FADE OUT
        // -----------------------------

        yield return StartCoroutine(
            FadeRoutine(0f, 1f)
        );

        // -----------------------------
        // LOAD SCENE
        // -----------------------------

        SceneManager.LoadScene(sceneName);

        // Wait one frame so the new scene
        // and its camera can initialize.
        yield return null;

        // -----------------------------
        // FIND NEW CAMERA
        // -----------------------------

        // Wait until Camera.main exists.
        while (Camera.main == null)
            yield return null;

        // -----------------------------
        // FADE IN
        // -----------------------------

        yield return StartCoroutine(
            FadeRoutine(1f, 0f)
        );

        isTransitioning = false;
    }

    // =========================================================
    // FADE ROUTINE
    // =========================================================

    private IEnumerator FadeRoutine(float from, float to)
    {
        if (fadeImage == null)
            yield break;

        float timer = 0f;

        Color color = fadeImage.color;

        // Block interaction while screen is black/fading.
        fadeImage.raycastTarget = true;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            color.a =
                Mathf.Lerp(
                    from,
                    to,
                    progress
                );

            fadeImage.color = color;

            yield return null;
        }

        color.a = to;
        fadeImage.color = color;

        // Allow interaction once screen is visible.
        if (to <= 0f)
        {
            fadeImage.raycastTarget = false;
        }
    }

    // =========================================================
    // SET ALPHA
    // =========================================================

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;

        color.a = alpha;

        fadeImage.color = color;

        fadeImage.raycastTarget = alpha > 0f;
    }
}