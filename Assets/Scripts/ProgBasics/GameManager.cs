using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int _score = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void AddScore(int amount)
    {
        _score += amount;
        Debug.Log("Score: " + _score);
    }
}