using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class RedFlashPostProcess : MonoBehaviour
{
    [SerializeField] private Volume hitVolume;       // drag your dedicated hit-effect Volume GameObject here
    [SerializeField] private float triggerDelay = 0.15f;
    [SerializeField] private float holdTime = 0.05f;
    [SerializeField] private float fadeOutTime = 0.4f;

    private Coroutine activeFlash;

    void Awake()
    {
        if (hitVolume == null)
            hitVolume = GetComponent<Volume>();

        hitVolume.weight = 0f; // off by default
    }

    // Hook this to Health -> OnDamaged in the Inspector
    public void ShowHit()
    {
        if (activeFlash != null)
            StopCoroutine(activeFlash);

        activeFlash = StartCoroutine(DelayedFlash());
    }

    private IEnumerator DelayedFlash()
    {
        yield return new WaitForSeconds(triggerDelay);

        hitVolume.weight = 1f;

        yield return new WaitForSeconds(holdTime);

        float t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            hitVolume.weight = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            yield return null;
        }

        hitVolume.weight = 0f;
        activeFlash = null;
    }
}