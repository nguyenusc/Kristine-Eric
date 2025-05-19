using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using DG.Tweening.Core.Easing;

public class StartGame : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI readyText;
    [SerializeField] TextMeshProUGUI goText;
    [SerializeField] CinemachineDollyCart dollyCart;
    [SerializeField] float playerStartSpeed;

    AnimationCurve zoomCurve;
    AnimationCurve fadeCurve;

    void Awake()
    {
        // Cache reference to initial player speed before setting to 0
        playerStartSpeed = dollyCart.m_Speed;

        // Init curves with ease in/out
        zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        readyText.gameObject.SetActive(true);
        goText.gameObject.SetActive(true);
    }

    // Stop player from moving
    void Start()
    {
        // Set alpha to 0 before fading in
        Color color = readyText.color;
        color.a = 0f;
        readyText.color = color;
        goText.color = color;

        dollyCart.m_Speed = 0f;
        StartCoroutine(ShowStartSequence());
    }

    IEnumerator ZoomInText(TextMeshProUGUI text, float duration)
    {
        float startSize = 98.0f;

        float endSize = 88.0f;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);    //ensure time is normalized
            text.fontSize = Mathf.Lerp(startSize, endSize, t);
            yield return null;
        }

        // Ensures text is completely visible
        text.fontSize = endSize;
    }

    IEnumerator ShowStartSequence()
    {
        yield return StartCoroutine(ZoomAndFadeText(readyText, 98f, 88f, 0f, 1f, 0.4f));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeText(readyText, 1f, 0f, 0.3f));

        yield return StartCoroutine(ZoomAndFadeText(goText, 98f, 88f, 0f, 1f, 0.4f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeText(goText, 1f, 0f, 0.3f));

        dollyCart.m_Speed = playerStartSpeed;                               // Start player movement
    }

    // Lerps the text between the sizes passed in
    IEnumerator AnimateText(TextMeshProUGUI text, float startSize, float endSize, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            text.fontSize = Mathf.Lerp(startSize, endSize, t);
            yield return null;
        }
        text.fontSize = endSize;
    }

    // Fades the text between the alphas passed in
    IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float endAlpha, float duration)
    {
        Color originalColor = text.color;
        Color fadeColor = originalColor;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            fadeColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
            text.color = fadeColor;
            yield return null;
        }

        // Final alpha
        fadeColor.a = endAlpha;
        text.color = fadeColor;
    }



    IEnumerator ZoomAndFadeText(TextMeshProUGUI text, float startSize, float endSize, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = text.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Evaluate via curve
            float zoomT = zoomCurve.Evaluate(t);
            float fadeT = fadeCurve.Evaluate(t);

            text.fontSize = Mathf.Lerp(startSize, endSize, zoomT);
            color.a = Mathf.Lerp(startAlpha, endAlpha, fadeT);
            text.color = color;

            yield return null;
        }

        // Final values
        text.fontSize = endSize;
        color.a = endAlpha;
        text.color = color;
    }


}



