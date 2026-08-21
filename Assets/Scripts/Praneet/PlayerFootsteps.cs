using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepInterval = 0.4f;
    [SerializeField] private float moveThreshold = 0.03f;       // raised — ignores small VR head jitter
    [SerializeField] private float requiredMovingTime = 0.15f;  // must move steadily for this long before first step plays

    private Vector3 _lastPosition;
    private float _stepTimer;
    private float _movingDuration;

    void Start()
    {
        _lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;
        float horizontalDelta = Vector3.Distance(
            new Vector3(currentPosition.x, 0, currentPosition.z),
            new Vector3(_lastPosition.x, 0, _lastPosition.z)
        );

        // Normalize against deltaTime so it reflects actual speed, not just frame-to-frame noise
        float speed = horizontalDelta / Time.deltaTime;
        bool isMoving = speed > moveThreshold;

        if (isMoving)
        {
            _movingDuration += Time.deltaTime;

            if (_movingDuration >= requiredMovingTime)
            {
                _stepTimer -= Time.deltaTime;
                if (_stepTimer <= 0f)
                {
                    PlayRandomStep();
                    _stepTimer = stepInterval;
                }
            }
        }
        else
        {
            _movingDuration = 0f;
            _stepTimer = 0f;
        }

        _lastPosition = currentPosition;
    }

    private void PlayRandomStep()
    {
        if (footstepClips == null || footstepClips.Length == 0 || footstepSource == null) return;
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        footstepSource.PlayOneShot(clip);
    }
}