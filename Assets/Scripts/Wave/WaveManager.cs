using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntUnityEvent : UnityEvent<int> { }

[System.Serializable]
public class WaveEntry
{
    public WaveData waveData;
    public Transform[] spawnPoints;
}

public class WaveManager : MonoBehaviour
{
    [SerializeField] private List<WaveEntry> _waves;
    [SerializeField] private WaveSpawner _spawner;

    public UnityEvent OnAllWavesCleared;
    public IntUnityEvent OnWaveStarted;

    public int CurrentWave => _currentWaveIndex + 1;
    public int TotalWaves => _waves.Count;

    private int _currentWaveIndex = -1;
    private int _aliveCount;
    private bool _spawningComplete;
    private bool _waveCleared;
    private bool _waveInProgress;

    private void Start()
    {
        // If there's only one wave total, no trigger is needed at all — just begin.
        if (_waves.Count <= 1)
        {
            StartNextWave();
        }
    }

    public void TriggerNextWave()
    {
        Debug.Log($"[WaveManager] TriggerNextWave called. _waveInProgress: {_waveInProgress}, _currentWaveIndex: {_currentWaveIndex}, _waves.Count: {_waves.Count}");

        if (_waveInProgress)
        {
            Debug.Log("[WaveManager] Blocked — wave already in progress.");
            return;
        }
        if (_currentWaveIndex + 1 >= _waves.Count)
        {
            Debug.Log("[WaveManager] Blocked — no more waves left.");
            return;
        }

        StartNextWave();
    }

    private void StartNextWave()
    {
        _currentWaveIndex++;
        _waveInProgress = true;

        if (_currentWaveIndex >= _waves.Count)
        {
            Debug.Log("All waves cleared — game won.");
            OnAllWavesCleared.Invoke();
            return;
        }

        _aliveCount = 0;
        _spawningComplete = false;
        _waveCleared = false;

        WaveEntry entry = _waves[_currentWaveIndex];
        Debug.Log($"Starting wave {_currentWaveIndex + 1}");
        OnWaveStarted?.Invoke(_currentWaveIndex + 1);
        _spawner.SpawnWave(entry.waveData, entry.spawnPoints, this);
    }

    public void RegisterEnemy(Health health)
    {
        _aliveCount++;
        health.OnDeath.AddListener(OnEnemyDied);
        Debug.Log($"Registered enemy, alive: {_aliveCount}");
    }

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
        if (_waveCleared || !_spawningComplete || _aliveCount > 0)
            return;

        _waveCleared = true;
        _waveInProgress = false;
        Debug.Log($"Wave {_currentWaveIndex + 1} cleared");

        // If the wave that just cleared is the SECOND-TO-LAST wave, auto-start
        // the final wave immediately — no trigger required for the last one.
        bool justClearedSecondToLast = (_currentWaveIndex == _waves.Count - 2);

        if (_currentWaveIndex + 1 >= _waves.Count)
        {
            Debug.Log("All waves cleared — game won.");
            OnAllWavesCleared.Invoke();
        }
        else if (justClearedSecondToLast)
        {
            StartNextWave();
        }
        // else: idles, waits for the next WaveTrigger
    }
}