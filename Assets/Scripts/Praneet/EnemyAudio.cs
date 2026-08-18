using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepInterval = 0.45f;
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("Attack")]
    [SerializeField] private AudioClip[] punchClips;

    [Header("Death")]
    [SerializeField] private AudioClip[] dieClips;

    [Header("Idle")]
    [SerializeField] private AudioClip[] idleClips;
    [SerializeField] private float idleMinInterval = 4f;   // random gap range between idle sounds
    [SerializeField] private float idleMaxInterval = 9f;

    private AudioSource _sfxSource;      // one-shots: footsteps, punch, die
    private AudioSource _idleSource;     // separate source so idle loop/one-shots don't cut off footsteps
    private NavMeshAgent _agent;
    private Health _health;

    private float _stepTimer;
    private float _idleTimer;
    private bool _isDead;

    void Awake()
    {
        var sources = GetComponents<AudioSource>();
        _sfxSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        _idleSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<Health>();

        ResetIdleTimer();
    }

    void OnEnable()
    {
        if (_health != null)
            _health.OnDeath.AddListener(PlayDieSound);
    }

    void OnDisable()
    {
        if (_health != null)
            _health.OnDeath.RemoveListener(PlayDieSound);
    }

    void Update()
    {
        if (_isDead || _agent == null || !_agent.enabled) return;

        bool isMoving = _agent.velocity.magnitude > moveThreshold;

        if (isMoving)
        {
            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                PlayRandomClip(_sfxSource, footstepClips);
                _stepTimer = stepInterval;
            }
            _idleTimer = 0f; // don't play idle sounds while walking
        }
        else
        {
            _stepTimer = 0f;
            _idleTimer -= Time.deltaTime;
            if (_idleTimer <= 0f)
            {
                PlayRandomClip(_idleSource, idleClips);
                ResetIdleTimer();
            }
        }
    }

    // Call this from an Animation Event on the Attack/Punch animation clip
    public void PlayPunchSound()
    {
        PlayRandomClip(_sfxSource, punchClips);
    }

    private void PlayDieSound()
    {
        _isDead = true;
        PlayRandomClip(_sfxSource, dieClips);
    }

    private void ResetIdleTimer()
    {
        _idleTimer = Random.Range(idleMinInterval, idleMaxInterval);
    }

    private void PlayRandomClip(AudioSource source, AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || source == null) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        source.PlayOneShot(clip);
    }
}