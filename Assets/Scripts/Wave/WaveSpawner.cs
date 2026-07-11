// WaveSpawner.cs
using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Health _playerHealth;

    public void SpawnWave(WaveData waveData, WaveManager manager)
    {
        StartCoroutine(SpawnRoutine(waveData, manager));
    }

    private IEnumerator SpawnRoutine(WaveData waveData, WaveManager manager)
    {
        if (_spawnPoints.Length == 0)
        {
            Debug.LogError("WaveSpawner has no spawn points assigned.");
            yield break;
        }

        for (int i = 0; i < waveData.enemyCount; i++) 
        {   
            Transform spawnPoint = _spawnPoints[i % _spawnPoints.Length];
            GameObject enemy = Instantiate(waveData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth == null)
            {
                Debug.LogError($"Spawned enemy '{enemy.name}' has no Health component — WaveManager cannot track it.");
            }
            else
            {
                manager.RegisterEnemy(enemyHealth);
            }

            EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
            if (enemyMove == null)
            {
                Debug.LogError($"Spawned enemy '{enemy.name}' has no EnemyMove component — it will spawn static.");
            }
            else
            {
                enemyMove.InjectPlayerReferences(_playerTransform, _playerHealth);
            }

            yield return new WaitForSeconds(waveData.spawnInterval);
        }

        manager.NotifySpawningComplete();
    }
}
