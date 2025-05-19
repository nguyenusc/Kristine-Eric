using UnityEngine;
using System.Collections;

public class FogLerpController : MonoBehaviour
{
    [Header("Fog Settings")]
    public float transitionDuration;

    private Coroutine currentLerp;

    void Start()
    {
        // Make sure fog is enabled
        RenderSettings.fog = true;
    }

    public void LerpFogEnd(float targetEnd)
    {
        if (currentLerp != null)
        {
            StopCoroutine(currentLerp);
        }

        currentLerp = StartCoroutine(LerpFogEndCoroutine(targetEnd, transitionDuration));
    }

    private IEnumerator LerpFogEndCoroutine(float targetEnd, float duration)
    {
        float start = RenderSettings.fogEndDistance;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            RenderSettings.fogEndDistance = Mathf.Lerp(start, targetEnd, t);
            yield return null;
        }

        RenderSettings.fogEndDistance = targetEnd;
        currentLerp = null;
    }
}
