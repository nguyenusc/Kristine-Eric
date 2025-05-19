using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using System;
using EasyTextEffects;

public class EndSceneManager : MonoBehaviour
{
    PlayerMovement playerMovement;
    FogLerpController fogController;
    CinemachineDollyCart playerSpline;
    AudioManager audioManager;

    [Header("UI Fields")]
    [SerializeField] GameObject winPanel;
    [SerializeField] TMP_FontAsset perfectText;
    [SerializeField] TMP_FontAsset okText;
    [SerializeField] TMP_FontAsset badText;
    [SerializeField] TextMeshProUGUI foodScoreText;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI ratingText;

    [SerializeField] GameObject slider;
    [SerializeField] TextMeshProUGUI bonusStageText;
    [SerializeField] TextMeshProUGUI niceScoopText;
    [SerializeField] TextMeshProUGUI tryAgainText;



    [Header("Camera Fields")]
    [SerializeField] CinemachineVirtualCamera overheadCamera;
    [SerializeField] CinemachineVirtualCamera bigIceCreamCamera;
    [SerializeField] CinemachineVirtualCamera defaultCamera;


    [Header("Ice Cream Fields")]
    [SerializeField] GameObject iceCreamPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform iceCreamTargetPosition;

    [Header("Countup Text Fields")]
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] float durationToTotal;

    bool hasSpawnedIceCream;

    bool hasWaterLowered;

    bool hasResetScene;

    bool soundHasPlayed;

    bool hasBonusStage;

    const float FOG_START = 418f;
    const float FOG_END = 1000f;

    string perfectColor = "#FFE600"; // Yellow Text for OK score
    string okColor = "#09FF00"; // Green Text for OK score
    string badColor = "#FF0003"; // Red Text for OK score


    private void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        fogController = FindObjectOfType<FogLerpController>();
        playerSpline = FindObjectOfType<CinemachineDollyCart>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    void ShowEndPanel()
    {
        StartCoroutine(EndCutsceneSequence());
    }
    void SetFontAndColor(TMP_FontAsset font, string hexColor)
    {
        foodScoreText.font = font;
        timeText.font = font;
        ratingText.font = font;

        if (ColorUtility.TryParseHtmlString(hexColor, out Color parsedColor))
        {
            foodScoreText.color = parsedColor;
            timeText.color = parsedColor;
            ratingText.color = parsedColor;
        }
    }
    IEnumerator EndCutsceneSequence()
    {
        // Hide slider, show win panel
        slider.SetActive(false);
        winPanel.gameObject.SetActive(true);

        if (playerMovement.GetScore() > 30)
        {
            SetFontAndColor(perfectText, perfectColor);
        }
        else if (playerMovement.GetScore() > 20)
        {
            SetFontAndColor(okText, okColor);
        }
        else
        {
            SetFontAndColor(badText, badColor);
        }


        yield return new WaitForSeconds(3f);

        // Spawn ice cream and switch camera
        if (!hasSpawnedIceCream)
        {
            playerMovement.SetCameraPriority(bigIceCreamCamera);

            GameObject iceCream = Instantiate(iceCreamPrefab, spawnPoint.position, Quaternion.identity);
            iceCream.transform.localScale = Vector3.one * 0.1f;

            GrowAndMove growScript = iceCream.GetComponent<GrowAndMove>();
            Vector3 finalScale;
            if (playerMovement.GetScore() > 30)
            {
                finalScale = new Vector3(25f, 17.6f, 23.7f);
            }
            else if (playerMovement.GetScore() > 20)
            {
                finalScale = new Vector3(25f / 2, 17.6f / 2, 23.7f / 2);
            }
            else
            {
                finalScale = new Vector3(25f / 3, 17.6f / 3, 23.7f / 3);
            }
            growScript.StartGrow(spawnPoint.position, iceCreamTargetPosition.position, finalScale, 2f);
            hasSpawnedIceCream = true;
        }

        yield return new WaitForSeconds(3f);
        if (playerMovement.GetScore() > 30)
        {
            bonusStageText.gameObject.SetActive(true);
        }
        else if (playerMovement.GetScore() > 20)
        {
            niceScoopText.gameObject.SetActive(true);
        }
        else
        {
            tryAgainText.gameObject.SetActive(true);
        }


        //    yield return new WaitForSeconds(3f);

        //    // Switch to overhead cam
        //    playerMovement.SetCameraPriority(overheadCamera);
        //    winPanel.gameObject.SetActive(false);

        //    // Wait before lowering water
        //    yield return new WaitForSeconds(2.85f);

        //    // Lower water and lessen fog while doing so
        //    if (!hasWaterLowered)
        //    {
        //        LowerWater();
        //        fogController.LerpFogEnd(FOG_END);
        //        hasWaterLowered = true;
        //    }

        //    // Wait and play "puzzle solved jingle" before transitioning back to player
        //    yield return new WaitForSeconds(3f);
        //    if (!soundHasPlayed)
        //    {
        //        audioManager.PlayPuzzleSoundSFX();
        //        soundHasPlayed = true;
        //    }

        //    yield return new WaitForSeconds(1f);


        //    // Reset fog to its original value, set player to position beyond ice cream,
        //    // and camera to default player camera
        //    if (!hasResetScene)
        //    {
        //        playerSpline.enabled = true;

        //        fogController.LerpFogEnd(FOG_START);
        //        playerMovement.SetDefaultCamera();

        //        hasResetScene = true;
        //    }

        //    yield return new WaitForSeconds(2.3f);

        //    if (!hasBonusStage)
        //    {
        //        bonusStageText.gameObject.SetActive(true);
        //        playerMovement.BonusStageEffects();
        //        slider.SetActive(true);

        //        yield return new WaitForSeconds(2.5f);

        //        bonusStageText.gameObject.SetActive(false);

        //        hasBonusStage = true;
        //    }
    }



    public static event Action OnLowerWater;
    public static void LowerWater()
    {
        OnLowerWater?.Invoke();
    }

    public void CountUpTo(int targetNumber)
    {
        StartCoroutine(CountUpCoroutine(targetNumber, durationToTotal));
    }

    IEnumerator CountUpCoroutine(int target, float duration)
    {
        float elapsed = 0f;
        int current = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            current = Mathf.RoundToInt(Mathf.Lerp(0, target, t));
            finalScoreText.text = current.ToString();
            yield return null;
        }

        // To make sure final number is hit
        finalScoreText.text = target.ToString();
    }

    private void OnEnable()
    {
        PlayerMovement.OnEndGame += ShowEndPanel;
    }
    private void OnDisable()
    {
        PlayerMovement.OnEndGame -= ShowEndPanel;
    }

}
