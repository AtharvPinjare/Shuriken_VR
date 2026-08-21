using UnityEngine;

[System.Serializable]
public class EnemyEntry
{
    public GameObject enemyPrefab;
    public int count;
}

[CreateAssetMenu(fileName = "NewWaveData", menuName = "Shuriken VR/WaveData")]
public class WaveData : ScriptableObject
{
    [Tooltip("One entry = single enemy type. Multiple entries = alternates between types.")]
    public EnemyEntry[] enemies;

    [Tooltip("Seconds between each enemy spawn.")]
    public float spawnInterval = 1.5f;
}