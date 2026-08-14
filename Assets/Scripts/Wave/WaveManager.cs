// WaveManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntUnityEvent : UnityEvent<int>
{
}


public class WaveManager : MonoBehaviour
{
    [SerializeField] private List<WaveData> _waves;
    [SerializeField] private WaveSpawner _spawner;
    [SerializeField] private float _delayBetweenWaves = 3f;
    public UnityEvent OnAllWavesCleared;
    public IntUnityEvent OnWaveStarted;
    public int CurrentWave => _currentWaveIndex + 1;
    public int TotalWaves => _waves.Count;

    private int _currentWaveIndex = -1;
    private int _aliveCount;
    private bool _spawningComplete;
    private bool _waveCleared;

    private void Start()
    {
        StartNextWave();
    }

    private void StartNextWave()
    {
        _currentWaveIndex++;

        if (_currentWaveIndex >= _waves.Count)
        {
            Debug.Log("All waves cleared — game won.");
            OnAllWavesCleared.Invoke();
            return;
        }

        _aliveCount = 0;
        _spawningComplete = false;
        _waveCleared = false;

        Debug.Log($"Starting wave {_currentWaveIndex + 1}");
        OnWaveStarted?.Invoke(_currentWaveIndex + 1);
        _spawner.SpawnWave(_waves[_currentWaveIndex], this);

    }

    // Called by WaveSpawner immediately after Instantiate for each enemy.
    public void RegisterEnemy(Health health)
    {
        _aliveCount++;
        health.OnDeath.AddListener(OnEnemyDied);
        Debug.Log($"Registered enemy, alive: {_aliveCount}");
    }

    // Called by WaveSpawner once it has finished spawning every enemy in the wave.
    public void NotifySpawningComplete()
    {
        _spawningComplete = true;
        CheckWaveClear();
    }

    private void OnEnemyDied()
    {
        _aliveCount--;
        CheckWaveClear();
    }

    private void CheckWaveClear()
    {
        // Both conditions matter: without _spawningComplete, killing the first
        // enemy the instant it spawns drops _aliveCount to 0 while the coroutine
        // still has enemies queued to spawn.
        if (_waveCleared || !_spawningComplete || _aliveCount > 0)
            return;

        _waveCleared = true;
        Debug.Log($"Wave {_currentWaveIndex + 1} cleared");
        StartCoroutine(NextWaveAfterDelay());
    }

    private IEnumerator NextWaveAfterDelay()
    {
        yield return new WaitForSeconds(_delayBetweenWaves);
        StartNextWave();
    }
}
