using UnityEngine;

public class FloatingTitle : MonoBehaviour
{
    [Header("Bob Settings")]
    public float bobHeight = 0.15f;
    public float bobSpeed = 1f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}