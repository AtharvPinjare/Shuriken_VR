using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Health _playerHealth;

    public void SpawnWave(WaveData waveData, Transform[] spawnPoints, WaveManager manager)
    {
        StartCoroutine(SpawnRoutine(waveData, spawnPoints, manager));
    }

    private IEnumerator SpawnRoutine(WaveData waveData, Transform[] spawnPoints, WaveManager manager)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError($"Wave using '{waveData.name}' has no spawn points assigned in WaveManager.");
            yield break;
        }

        if (waveData.enemies == null || waveData.enemies.Length == 0)
        {
            Debug.LogError($"WaveData '{waveData.name}' has no enemy entries assigned.");
            yield break;
        }

        int totalToSpawn = 0;
        foreach (var e in waveData.enemies) totalToSpawn += e.count;

        int[] remaining = new int[waveData.enemies.Length];
        for (int i = 0; i < waveData.enemies.Length; i++)
            remaining[i] = waveData.enemies[i].count;

        int spawnPointIndex = 0;
        int entryCursor = 0;

        for (int spawned = 0; spawned < totalToSpawn; spawned++)
        {
            int attempts = 0;
            while (remaining[entryCursor] <= 0 && attempts < waveData.enemies.Length)
            {
                entryCursor = (entryCursor + 1) % waveData.enemies.Length;
                attempts++;
            }

            EnemyEntry entry = waveData.enemies[entryCursor];
            remaining[entryCursor]--;

            Transform spawnPoint = spawnPoints[spawnPointIndex % spawnPoints.Length];
            spawnPointIndex++;

            GameObject enemy = Instantiate(entry.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            BillboardUI healthBarBillboard = enemy.GetComponentInChildren<BillboardUI>();
            if (healthBarBillboard == null)
                Debug.LogError($"Spawned enemy '{enemy.name}' has no BillboardUI on its Canvas — health bar will not face the player.");
            else
                healthBarBillboard.SetTarget(_playerTransform);

            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth == null)
                Debug.LogError($"Spawned enemy '{enemy.name}' has no Health component — WaveManager cannot track it.");
            else
                manager.RegisterEnemy(enemyHealth);

            EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
            FlyingMageEnemy flyingEnemy = enemy.GetComponent<FlyingMageEnemy>();
            if (enemyMove != null)
                enemyMove.InjectPlayerReferences(_playerTransform, _playerHealth);
            else if (flyingEnemy != null)
                flyingEnemy.InjectPlayerReferences(_playerTransform, _playerHealth);
            else
                Debug.LogError($"Spawned enemy '{enemy.name}' has no EnemyMove or FlyingMageEnemy component — it will spawn static.");

            entryCursor = (entryCursor + 1) % waveData.enemies.Length;
            yield return new WaitForSeconds(waveData.spawnInterval);
        }

        manager.NotifySpawningComplete();
    }
}