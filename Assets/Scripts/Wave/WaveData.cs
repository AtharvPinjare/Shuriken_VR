// WaveData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewWaveData", menuName = "Shuriken VR/WaveData")]
public class WaveData : ScriptableObject
{
    [Tooltip("Enemy prefab to spawn for this wave.")]
    public GameObject enemyPrefab;

    [Tooltip("How many enemies spawn in this wave.")]
    public int enemyCount = 5;

    [Tooltip("Seconds between each enemy spawn.")]
    public float spawnInterval = 1.5f;
}
