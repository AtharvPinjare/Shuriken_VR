using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 2f;

    public static SceneFader Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Only protect THIS object, not its current parent chain
            transform.SetParent(null); // detach from camera rig first
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutThenLoad(sceneName));
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
            transform.rotation = Camera.main.transform.rotation;
        }
    }

    private System.Collections.IEnumerator FadeOutThenLoad(string sceneName)
    {
        yield return StartCoroutine(Fade(0f, 1f)); // fade to black
        SceneManager.LoadScene(sceneName);
        yield return null; // wait a frame for new scene to load
        yield return StartCoroutine(Fade(1f, 0f)); // fade back in
    }

    private System.Collections.IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(c.r, c.g, c.b, to);
    }
}