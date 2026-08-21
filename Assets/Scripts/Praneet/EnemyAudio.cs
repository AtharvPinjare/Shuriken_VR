using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepInterval = 0.45f;
    [SerializeField] private float moveThreshold = 0.1f;
    [SerializeField] private float footstepVolume = 1.5f;

    [Header("Attack")]
    [SerializeField] private AudioClip[] punchClips;
    [SerializeField] private float punchVolume = 2f;

    [Header("Death")]
    [SerializeField] private AudioClip[] dieClips;
    [SerializeField] private float dieVolume = 1.8f;

    [Header("Idle")]
    [SerializeField] private AudioClip[] idleClips;
    [SerializeField] private float idleVolume = 1.5f;
    [SerializeField] private float idleMinInterval = 4f;
    [SerializeField] private float idleMaxInterval = 9f;

    private AudioSource _sfxSource;
    private AudioSource _idleSource;
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
                PlayRandomClip(_sfxSource, footstepClips, footstepVolume);
                _stepTimer = stepInterval;
            }
            _idleTimer = 0f;
        }
        else
        {
            _stepTimer = 0f;
            _idleTimer -= Time.deltaTime;
            if (_idleTimer <= 0f)
            {
                PlayRandomClip(_idleSource, idleClips, idleVolume);
                ResetIdleTimer();
            }
        }
    }

    public void PlayPunchSound()
    {
        PlayRandomClip(_sfxSource, punchClips, punchVolume);
    }

    private void PlayDieSound()
    {
        _isDead = true;
        PlayRandomClip(_sfxSource, dieClips, dieVolume);
    }

    private void ResetIdleTimer()
    {
        _idleTimer = Random.Range(idleMinInterval, idleMaxInterval);
    }

    private void PlayRandomClip(AudioSource source, AudioClip[] clips, float volumeScale)
    {
        if (clips == null || clips.Length == 0 || source == null) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        source.PlayOneShot(clip, volumeScale);
    }
}