using UnityEngine;

public class MagicRuneEffect : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 90f;

    [Header("Pulse (scale in/out)")]
    public bool pulse = true;
    public float pulseAmount = 0.1f;
    public float pulseSpeed = 2f;

    [Header("Auto-Destroy")]
    public float lifetime = 0f;

    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);

        if (pulse)
        {
            float t = Mathf.Sin(Time.time * pulseSpeed);
            float scaleMul = 1f + (t * pulseAmount);
            transform.localScale = startScale * scaleMul;
        }
    }
}