using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSignActivator : MonoBehaviour
{
    [Header("Scene Loading")]
    [Tooltip("Exact scene name to load, must be added in Build Settings")]
    public string sceneToLoad = "Game_Scene";

    [Header("Optional feedback before load")]
    public GameObject activateVFX;
    public AudioSource activateSound;
    public float loadDelay = 0f; // set >0 if you want VFX/sound to play before switching

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

            if (loadDelay > 0f)
                Invoke(nameof(LoadTargetScene), loadDelay);
            else
                LoadTargetScene();
        }
    }

    private void LoadTargetScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}