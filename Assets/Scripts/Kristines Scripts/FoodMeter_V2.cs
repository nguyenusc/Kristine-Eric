using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FoodMeter_V2 : MonoBehaviour
{
    [SerializeField] float lerpSpeed = 2.0f;

    Slider foodSlider;
    PlayerAccelerate accelerate;
    PlayerMovement player;

    int cutoff3;
    float targetValue;
    int currentScore = 0;

    void Start()
    {
        accelerate = FindObjectOfType<PlayerAccelerate>();
        player = FindObjectOfType<PlayerMovement>();
        foodSlider = GetComponent<Slider>();

        // Use only the final cutoff now
        cutoff3 = accelerate.GetCutoff3();

        currentScore = player.GetScore();
        targetValue = Mathf.Clamp01((float)currentScore / cutoff3);

        // Prevents slider from lerping to 0 at beginning
        foodSlider.value = targetValue;
    }


    void Update()
    {
        currentScore = player.GetScore();
        targetValue = Mathf.Clamp01((float)currentScore / cutoff3);

        // Smoothly move slider towards target
        foodSlider.value = Mathf.Lerp(foodSlider.value, targetValue, Time.deltaTime * lerpSpeed);
    }
}
