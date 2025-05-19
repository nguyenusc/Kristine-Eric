using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    TextMeshProUGUI timerText;
    [SerializeField] float timerValue = 90.0f;
    [SerializeField] float msFontSize = 25.0f;
    [SerializeField] TextMeshProUGUI finalTimeText;

    bool isLevelOver;
    float startTimerValue;

    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        timerText.text = timerValue.ToString("#.00");

        startTimerValue = timerValue;
    }

    void Update()
    {
        if (!isLevelOver)
        {
            timerValue -= Time.deltaTime;

            int seconds = (int)timerValue;
            int milliseconds = (int)((timerValue - seconds) * 100);
            timerText.text = $"{seconds}.<size={msFontSize}>{milliseconds:D2}</size>";
        }
    }

    private void OnEnable()
    {
        PlayerMovement.OnPlayerWins += ShowFinalTime;
    }

    private void OnDisable()
    {
        PlayerMovement.OnPlayerWins -= ShowFinalTime;
    }

    // Clear timer from corner and show final time
    void ShowFinalTime()
    {
        isLevelOver = true;
        timerText.text = "";
        finalTimeText.text = (startTimerValue - timerValue).ToString("#.00");
        // Log the final completion time to metrics
        MetricsTracker.SetFinalTime(finalTimeText.text);
    }

    //Add some effects if timer is close to 0 like change red... countdown on screen, etc
}
