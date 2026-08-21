using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    private bool isFading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // Start completely transparent
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    public void FadeToScene(string sceneName)
    {
        if (isFading) return;

        StartCoroutine(FadeOutThenLoad(sceneName));
    }

    private void LateUpdate()
    {
        if (Camera.main == null)
            return;

        // For your VR world-space fade
        transform.position =
            Camera.main.transform.position +
            Camera.main.transform.forward * 0.3f;

        transform.rotation = Camera.main.transform.rotation;
    }

    private IEnumerator FadeOutThenLoad(string sceneName)
    {
        isFading = true;

        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f));

        // Load scene
        SceneManager.LoadScene(sceneName);

        // Wait until the new scene has rendered
        yield return null;
        yield return new WaitForEndOfFrame();

        // Fade back in
        yield return StartCoroutine(Fade(1f, 0f));

        isFading = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null)
        {
            Debug.LogError("SceneFader: Fade Image is not assigned!");
            yield break;
        }

        float elapsed = 0f;

        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / fadeDuration);

            color.a = Mathf.Lerp(from, to, t);
            fadeImage.color = color;

            yield return null;
        }

        color.a = to;
        fadeImage.color = color;
    }
}