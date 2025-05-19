using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EasyTextEffects;

public class JumpsUI : MonoBehaviour
{
    TextMeshProUGUI numJumpsText;
    TextEffect textEffect;

    // The first time player accelerates, text bounces for longer 
    bool hasAccelerated;

    void Start()
    {
        numJumpsText = GetComponent<TextMeshProUGUI>();
        textEffect = GetComponent<TextEffect>();
    }

    public void OnEnable()
    {
        HamsterMovement.OnJumpsChanged += ChangeJumpUI;
    }

    public void OnDisable()
    {
        HamsterMovement.OnJumpsChanged -= ChangeJumpUI;
    }

    void ChangeJumpUI(int jumpCount)
    {
        numJumpsText.text = $"<link=bounce_high+title>{jumpCount}x</link>";
        numJumpsText.ForceMeshUpdate();
        textEffect.Refresh();
        textEffect.StartManualTagEffects();

        if (!hasAccelerated)
        {
            StartCoroutine(BounceText(5.0f));
        }
        else
        {
            StartCoroutine(BounceText(3.0f));
        }
    }

    IEnumerator BounceText(float delay)
    {
        yield return new WaitForSeconds(delay);
        textEffect.StopManualTagEffects();
    }
}
