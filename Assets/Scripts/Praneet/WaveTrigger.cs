using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaveTrigger : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private bool disableAfterTrigger = true;

    private bool _hasTriggered;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[WaveTrigger] OnTriggerEnter fired by: {other.gameObject.name}");
        Debug.Log($"[WaveTrigger] other.attachedRigidbody: {(other.attachedRigidbody != null ? other.attachedRigidbody.name : "NULL")}");
        Debug.Log($"[WaveTrigger] playerRigidbody assigned: {(playerRigidbody != null ? playerRigidbody.name : "NULL — NOT ASSIGNED IN INSPECTOR")}");
        Debug.Log($"[WaveTrigger] _hasTriggered: {_hasTriggered}");

        if (_hasTriggered)
        {
            Debug.Log("[WaveTrigger] Blocked — already triggered.");
            return;
        }

        if (other.attachedRigidbody != playerRigidbody)
        {
            Debug.Log("[WaveTrigger] Blocked — rigidbody mismatch.");
            return;
        }

        Debug.Log("[WaveTrigger] PASSED all checks — calling TriggerNextWave()");
        _hasTriggered = true;
        waveManager.TriggerNextWave();

        if (disableAfterTrigger)
            gameObject.SetActive(false);
    }
}