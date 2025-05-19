using UnityEngine;
using UnityEngine.UI;

public class BalanceSliderController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player Transform to follow")]
    [SerializeField] private GameObject player;
    [Tooltip("The RectTransform of the slider UI element")]
    [SerializeField] private RectTransform sliderRect;
    [Tooltip("The Slider component used as a balance indicator")]
    [SerializeField] private Slider slider;

    [Header("UI Offset")]
    [Tooltip("Offset in screen space (pixels) relative to the player's screen position")]
    [SerializeField] private Vector2 screenOffset;

    [Header("Balance Settings")]
    [Tooltip("Amplitude of the oscillatory drift (0-1 range)")]
    [SerializeField] private float oscillationAmplitude = 0.1f;
    [SerializeField] private float jumpOscillationAmplitude = 0.1f;
    [Tooltip("Frequency of oscillation (radians per second)")]
    [SerializeField] private float oscillationFrequency = 1f;
    [Tooltip("Rate at which keyboard input shifts the balance center (per second)")]
    [SerializeField] private float controlSpeed = 0.2f;
    [Tooltip("Fixed amount added/subtracted when using mouse buttons")]
    [SerializeField] private float mouseControlAmount = 0.05f;

    [Header("Loss Conditions (Optional)")]
    [Tooltip("If slider value goes below this, balance is lost")]
    [SerializeField] private float loseThresholdLow = 0.2f;
    [Tooltip("If slider value goes above this, balance is lost")]
    [SerializeField] private float loseThresholdHigh = 0.8f;

    // Internal state
    private float centerValue = 0.5f; // The oscillation center (0.5 is balanced)
    private float timeOffset;         // Optional phase offset for oscillation
    private TestPlayerMovement playerMove;
    void Start()
    {
        // Optionally randomize the phase so the oscillation doesn't always start at 0.
        timeOffset = Random.Range(0f, Mathf.PI * 2f);

        // Initialize slider value.
        if (slider != null)
            slider.value = centerValue;
        playerMove = player.GetComponent<TestPlayerMovement>();
    }

    void Update()
    {
        // Update the UI position so that it follows the player's screen position.
        if (player != null && sliderRect != null)
        {
            // Convert player's world position to screen point.
            Vector3 screenPos = Camera.main.WorldToScreenPoint(player.transform.position);
            sliderRect.position = screenPos + (Vector3)screenOffset;
        }
        float oscillation = 0;
        if (playerMove.isJumping)
        {
            oscillation = jumpOscillationAmplitude * Mathf.Sin(Time.time * oscillationFrequency + timeOffset);

        }
        else
        {
            oscillation = oscillationAmplitude * Mathf.Sin(Time.time * oscillationFrequency + timeOffset);
        }

        // Calculate the oscillatory component.

        float newValue = centerValue + oscillation;
        newValue = Mathf.Clamp01(newValue);
        if (slider != null)
            slider.value = newValue;

        // Process keyboard input to shift the balance center.
        // A key moves the center left (decrease value), D key moves it right (increase value).
        if (Input.GetKey(KeyCode.A))
        {
            centerValue -= controlSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            centerValue += controlSpeed * Time.deltaTime;
        }

        // Process mouse input: left button decreases the center, right button increases it.
        if (Input.GetMouseButton(0))
        {
            centerValue -= mouseControlAmount;
        }
        if (Input.GetMouseButton(1))
        {
            centerValue += mouseControlAmount;
        }
        centerValue = Mathf.Clamp01(centerValue);

        // Check for loss conditions (optional).
        if (newValue <= loseThresholdLow || newValue >= loseThresholdHigh)
        {
            Debug.Log("Balance lost!");
            player.GetComponent<RestartScene>().Restart();
            // Here you could call a restart function or trigger a loss event.
        }
    }
}
