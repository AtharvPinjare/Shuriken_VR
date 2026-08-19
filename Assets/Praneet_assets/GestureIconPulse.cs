using UnityEngine;

public class GestureIconPulse : MonoBehaviour
{
    public enum PulseMode { PositionZ, PositionY, Scale, Rotation }

    [Header("Mode")]
    public PulseMode mode = PulseMode.PositionZ;

    [Header("Position Pulse (Z = toward/away, Y = up/down)")]
    public float travelDistance = 30f; // canvas units if under a UI Canvas

    [Header("Scale Pulse (punch emphasis)")]
    public float scaleAmount = 0.15f;

    [Header("Rotation Pulse (e.g. shoot/twist gestures)")]
    [Tooltip("Local Euler axis to rotate around, e.g. (0,0,1) for a flat 2D " +
             "twist on a UI icon, or (1,0,0)/(0,1,0) for 3D hand models.")]
    public Vector3 rotationAxis = Vector3.forward;
    [Tooltip("Max rotation angle in degrees, swings from 0 -> angle -> 0.")]
    public float rotationAngle = 45f;

    [Header("Timing")]
    public float moveDuration = 0.6f;
    public float pauseDuration = 1f;

    [Header("Alternating Hands")]
    public float startDelay = 0f;

    private Vector3 startPos;
    private Vector3 startScale;
    private Quaternion startRotation;

    void Start()
    {
        startPos = transform.localPosition;
        startScale = transform.localScale;
        startRotation = transform.localRotation;
    }

    void Update()
    {
        float cycleLength = moveDuration + pauseDuration;
        float fullLoop = cycleLength * 2f;

        float localTime = Mathf.Repeat(Time.time - startDelay, fullLoop);

        float t = 0f;
        if (localTime >= 0f && localTime < moveDuration)
        {
            float progress = localTime / moveDuration;
            t = Mathf.Sin(progress * Mathf.PI);
        }

        switch (mode)
        {
            case PulseMode.PositionZ:
                {
                    Vector3 pos = startPos;
                    pos.z += t * travelDistance;
                    transform.localPosition = pos;
                    break;
                }
            case PulseMode.PositionY:
                {
                    Vector3 pos = startPos;
                    pos.y += t * travelDistance;
                    transform.localPosition = pos;
                    break;
                }
            case PulseMode.Scale:
                {
                    float scaleMul = 1f + (t * scaleAmount);
                    transform.localScale = startScale * scaleMul;
                    break;
                }
            case PulseMode.Rotation:
                {
                    float angle = t * rotationAngle;
                    transform.localRotation = startRotation * Quaternion.AngleAxis(angle, rotationAxis);
                    break;
                }
        }
    }
}