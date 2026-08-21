using UnityEngine;

public class TutorialSignActivator : MonoBehaviour
{
    [Header("Scene Loading")]
    [Tooltip("Exact scene name to load, must be added in Build Settings")]
    public string sceneToLoad = "Game_Scene";

    [Header("Optional Feedback Before Load")]
    public GameObject activateVFX;
    public AudioSource activateSound;

    private bool activated = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (activated) return;

        if (collision.gameObject.TryGetComponent(out FireballProjectile projectile))
        {
            activated = true;

            if (activateVFX != null)
                activateVFX.SetActive(true);

            if (activateSound != null)
                activateSound.Play();

            // USE THE FADER instead of directly loading the scene
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.FadeToScene(sceneToLoad);
            }
            else
            {
                Debug.LogError("SceneFader.Instance is NULL! Make sure a SceneFader exists in the scene.");
            }
        }
    }
}