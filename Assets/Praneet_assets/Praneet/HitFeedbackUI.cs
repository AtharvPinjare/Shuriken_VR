using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HitFeedbackUI : MonoBehaviour
{
    [SerializeField] private Image hitOverlay;      // drag "Image" child here
    [SerializeField] private float flashInAlpha = 0.6f;   // peak visibility
    [SerializeField] private float holdTime = 0.05f;      // brief hold at peak
    [SerializeField] private float fadeOutTime = 0.4f;    // fade back to 0

    private Coroutine activeFlash;

    void Awake()
    {
        if (hitOverlay == null)
            hitOverlay = GetComponentInChildren<Image>();

        SetAlpha(0f);
    }

    // Hook this to Health -> OnDamaged in the Inspector
    public void ShowHit()
    {
        if (activeFlash != null)
            StopCoroutine(activeFlash);

        activeFlash = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetAlpha(flashInAlpha);

        yield return new WaitForSeconds(holdTime);

        float t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(flashInAlpha, 0f, t / fadeOutTime);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(0f);
        activeFlash = null;
    }

    private void SetAlpha(float a)
    {
        if (hitOverlay == null) return;
        Color c = hitOverlay.color;
        c.a = a;
        hitOverlay.color = c;
    }
}