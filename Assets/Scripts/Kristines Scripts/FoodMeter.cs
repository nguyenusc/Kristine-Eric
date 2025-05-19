using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FoodMeter : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float lerpSpeed = 2.0f; // Speed of depletion when cutoff changes

    private PlayerAccelerate accelerate;
    private PlayerMovement player;

    private int cutoff1;
    private int cutoff2;
    private int cutoff3;
    private int previousScore = 0;
    private int previousTargetCutoff;
    private int previousCutoff; // Stores the last cutoff before change

    [SerializeField] float MAX_FILL_AMOUNT = 0.55f;
    private bool isResetting = false;
    private bool reachedFull = false;

    void Start()
    {
        accelerate = FindObjectOfType<PlayerAccelerate>();
        player = FindObjectOfType<PlayerMovement>();

        // Get cutoff values from PlayerAccelerate
        cutoff1 = accelerate.GetCutoff1();
        cutoff2 = accelerate.GetCutoff2();
        cutoff3 = accelerate.GetCutoff3();

        previousTargetCutoff = DetermineTargetCutoff(previousScore);
        previousCutoff = 0; // Start at 0 so first range calculation is valid

        UpdateFillAmount();
    }

    void Update()
    {
        int currentScore = player.GetScore();
        int targetCutoff = DetermineTargetCutoff(currentScore);

        if (targetCutoff != previousTargetCutoff)
        {
            if (!reachedFull)
            {
                // Ensure bar reaches full before resetting
                StartCoroutine(FillToMaxThenReset());
            }
        }
        else if (!isResetting && currentScore != previousScore)
        {
            UpdateFillAmount();
        }

        previousScore = currentScore;
    }

    void UpdateFillAmount()
    {
        MetricsTracker.IncrementMetric("UpdateFill");
        if (reachedFull) return; // Stop updating when at full before reset

        int currentScore = player.GetScore();
        int targetCutoff = previousTargetCutoff;

        // Prevent division by zero
        int rangeStart = previousCutoff;
        int rangeEnd = targetCutoff;
        int range = rangeEnd - rangeStart;

        if (range <= 0) return;

        // Normalize score within the current range
        int scoreInCurrentRange = currentScore;
        if (previousCutoff != targetCutoff)
        {
            scoreInCurrentRange -= previousCutoff;
        }

        float fillAmount = (float)scoreInCurrentRange / range * MAX_FILL_AMOUNT;
        fillImage.fillAmount = Mathf.Clamp(fillAmount, 0f, MAX_FILL_AMOUNT);
    }

    IEnumerator FillToMaxThenReset()
    {
        reachedFull = true;
        float elapsedTime = 0f;
        float startFill = fillImage.fillAmount;

        // Fill to max before resetting
        while (elapsedTime < 0.5f) // Quick fill to max before depleting
        {
            fillImage.fillAmount = Mathf.Lerp(startFill, MAX_FILL_AMOUNT, elapsedTime * 3);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fillImage.fillAmount = MAX_FILL_AMOUNT;

        yield return new WaitForSeconds(0.5f); // Hold max fill for a moment

        // Start depletion to zero
        StartCoroutine(ResetFillAmount());
    }

    IEnumerator ResetFillAmount()
    {
        isResetting = true;
        float elapsedTime = 0f;
        float startFill = fillImage.fillAmount;

        while (elapsedTime < 1.0f / lerpSpeed)
        {
            fillImage.fillAmount = Mathf.Lerp(startFill, 0f, elapsedTime * lerpSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fillImage.fillAmount = 0f;
        isResetting = false;
        reachedFull = false;

        // Update the cutoff values for the next range
        previousCutoff = previousTargetCutoff;
        previousTargetCutoff = DetermineTargetCutoff(player.GetScore());
    }

    int DetermineTargetCutoff(int score)
    {
        if (score < cutoff1) return cutoff1;
        if (score < cutoff2) return cutoff2;
        return cutoff3;
    }
}
