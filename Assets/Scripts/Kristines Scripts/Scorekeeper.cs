using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using EasyTextEffects;
public class Scorekeeper : MonoBehaviour
{
    PlayerAccelerate accelerate;
    PlayerMovement player;
    TextMeshProUGUI scoreText;
    TextEffect textEffect;

    int cutoff1;
    int cutoff2;
    int cutoff3;
    int previousScore = 0;
    int previousTargetCutoff;

    bool hasStarted = false; // Flag to prevent bouncing on start


    void Start()
    {
        accelerate = FindObjectOfType<PlayerAccelerate>();
        player = FindObjectOfType<PlayerMovement>();
        scoreText = GetComponent<TextMeshProUGUI>();
        textEffect = GetComponent<TextEffect>();

        cutoff1 = accelerate.GetCutoff1();
        cutoff2 = accelerate.GetCutoff2();
        cutoff3 = accelerate.GetCutoff3();

        previousTargetCutoff = DetermineTargetCutoff(previousScore);

        // Init text without bouncing
        UpdateScoreText(false);
        hasStarted = true;
    }

    void Update()
    {
        int currentScore = player.GetScore();
        int targetCutoff = DetermineTargetCutoff(currentScore);

        if (currentScore != previousScore)
        {
            UpdateScoreText(true);
            previousScore = currentScore;
            previousTargetCutoff = targetCutoff;
        }
    }


    void UpdateScoreText(bool allowBounce)
    {
        int currentScore = player.GetScore();
        int targetCutoff = DetermineTargetCutoff(currentScore);
        string newText;

        if (currentScore < previousScore)
        {
            // Apply shake / "damage" effect when score decreases
            newText = $"<link=shake>{currentScore} / {targetCutoff}</link>";
            StartCoroutine(StopEffectsAfterDelay(0.5f));
        }
        else if (allowBounce && hasStarted && targetCutoff != previousTargetCutoff)
        {
            // Bounce ENTIRE text when target cutoff changes
            newText = $"<link=bounce_once>{currentScore} / {targetCutoff}</link>";
        }
        else if (allowBounce && hasStarted)
        {
            newText = $"<link=bounce_once>{currentScore}</link> / {targetCutoff}";
        }
        else
        {
            // Stop bounce from happening at the start
            newText = $"{currentScore} / {targetCutoff}";
        }

        scoreText.text = newText;

        scoreText.ForceMeshUpdate();

        if (hasStarted)
        {
            textEffect.Refresh();
            textEffect.StartManualTagEffects();
        }
    }


    int DetermineTargetCutoff(int score)
    {
        if (score < cutoff1) return cutoff1;
        if (score < cutoff2) return cutoff2;
        return cutoff3;
    }

    // Score decrements to 1/4 their previous cutoff
    public void DecreaseScoreOnHit()
    {
        int score = player.GetScore();
        int decrementAmount = 0;

        if (score >= cutoff2)
        {
            decrementAmount = (cutoff3 - cutoff2) / 4;
        }
        else if (score >= cutoff1)
        {
            decrementAmount = (cutoff2 - cutoff1) / 4;
        }
        else
        {
            decrementAmount = cutoff1 / 4;
        }

        int newScore = Mathf.Max(0, score - decrementAmount);
        player.SetScore(newScore); // Update player's score

        // Update UI
        UpdateScoreText(true);
    }

    IEnumerator StopEffectsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        textEffect.StopManualTagEffects();
    }
}