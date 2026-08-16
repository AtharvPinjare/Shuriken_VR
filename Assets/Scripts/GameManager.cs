using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Victory, Defeat }

    public static GameManager Instance { get; private set; }

    [SerializeField] private Health playerHealth;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private DragonMove[] dragons;

    public GameState CurrentState { get; private set; } = GameState.Playing;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerHealth.OnDeath.AddListener(HandlePlayerDeath);
        waveManager.OnAllWavesCleared.AddListener(HandleVictory);

        foreach (var dragon in dragons)
        {
            if (dragon != null)
                dragon.InjectPlayerReferences(playerTransform, playerHealth);
        }
    }

    private void HandlePlayerDeath()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Defeat;
        waveSpawner.gameObject.SetActive(false); // kills any running spawn coroutine
        Debug.Log("DEFEAT");
    }

    private void HandleVictory()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Victory;
        waveSpawner.gameObject.SetActive(false);
        Debug.Log("VICTORY");
    }
}