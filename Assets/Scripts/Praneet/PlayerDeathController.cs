using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathController : MonoBehaviour
{
    [SerializeField] private float restartDelay = 2f;

    // Hook this to GameManager -> OnDefeat in the Inspector
    public void OnPlayerDefeat()
    {
        Invoke(nameof(RestartScene), restartDelay);
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}