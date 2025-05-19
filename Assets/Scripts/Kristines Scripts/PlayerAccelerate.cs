using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.Collections;
using ChristinaCreatesGames.Animations;
using System;

public class PlayerAccelerate : MonoBehaviour
{
    [Header("Points to Accelerate")]
    [SerializeField] int CUTOFF_1;
    [SerializeField] int CUTOFF_2;
    [SerializeField] int CUTOFF_3;
    [SerializeField] int currentCutoff = 0;

    public int GetCutoff1() { return CUTOFF_1; }
    public int GetCutoff2() { return CUTOFF_2; }
    public int GetCutoff3() { return CUTOFF_3; }
    public int GetCutoff() { return currentCutoff; }

    [Header("Speed Boosts")]
    [SerializeField] float SPEED_1;
    [SerializeField] float SPEED_2;
    [SerializeField] float SPEED_3;
    [SerializeField] int currentSpeed = 0;
    [SerializeField] float baseSpeed = 10;
    // baseSpeed was previously cached in Awake, but since the start script overwrites the dolly cart speed to 0
    // baseSpeed needs to be set manually 
    public int GetCurrentSpeed() { return currentSpeed; }

    [SerializeField] float MAX_SLIDER_VALUE = 0.66f;
    const float MAX_SPEED = 100.0f;

    float animSpeed;
    float ANIM_SPEED_0 = 1f;
    float ANIM_SPEED_1 = 2f;
    float ANIM_SPEED_2 = 2.5f;
    float ANIM_SPEED_3 = 3f;
    public float GetAnimSpeed() { return animSpeed; }

    PlayerMovement player;
    CinemachineDollyCart dollyCart;
    AudioManager audioManager;
    Animator hamsterAnim;

    [Header("UI Elements")]
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] Animator hamsterSpriteAnim;
    [SerializeField] GameObject hamsterSprite;
    [SerializeField] GameObject speechBubble;
    [SerializeField] ScriptableRendererFeature speedLines;
    [SerializeField] float speedLinesTime = 1.5f;

    [Header("Reset Hit Timer")]
    [SerializeField] float resetHitTimer = 1.5f;
    bool wasHitRecently;    // prevents acceleration effects from triggering after being hit

    // Show the hamster dialogue only once upon first acceleration
    bool hasAcceleratedOnce;

    void Start()
    {
        player = GetComponent<PlayerMovement>();
        dollyCart = GetComponentInParent<CinemachineDollyCart>();
        hamsterAnim = GetComponent<Animator>();
        audioManager = FindObjectOfType<AudioManager>();

        speedText.text = ((int)(MAX_SPEED * (1.0f / 8))).ToString();
        slider.value = MAX_SLIDER_VALUE * 1 / 6;
    }

    void Update()
    {
        AccelerateCheck();
    }

    // Compare score and current speed to determine if player can accelerate
    void AccelerateCheck()
    {
        int score = player.GetScore();

        if (score >= CUTOFF_3 && currentSpeed < 3)
        {
            currentSpeed = 3;
            SetSpeed3();
        }
        else if (score >= CUTOFF_2 && currentSpeed < 2)
        {
            currentSpeed = 2;
            SetSpeed2();
        }
        else if (score >= CUTOFF_1 && currentSpeed < 1)
        {
            currentSpeed = 1;
            SetSpeed1();
        }
    }

    //if player is hit, player gets knocked back to previous speed 
    // Score decrements to 1/4 of previous threshold 
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Enemy>() || other.GetComponent<Obstacle>())
        {
            wasHitRecently = true;
            StartCoroutine(ResetHitFlag());

            // Reset speed appropriately
            switch (currentSpeed)
            {
                case 1:
                    SetDefault();
                    currentSpeed = 0;
                    break;
                case 2:
                    SetSpeed1();
                    currentSpeed = 1;
                    break;
                case 3:
                    SetSpeed2();
                    currentSpeed = 2;
                    break;
            }
        }
    }

    IEnumerator ResetHitFlag()
    {
        yield return new WaitForSeconds(resetHitTimer);
        wasHitRecently = false;
    }

    void PlayAccelerationEffects()
    {
        audioManager.PlayAccelerateSFX();

        // Show hamster only on first acceleration
        if (!hasAcceleratedOnce)
        {
            hasAcceleratedOnce = true;
            StartCoroutine(ShowHamsterSpriteandDialogue());
        }
        StartCoroutine(ShowSpeedLines());
    }

    // Hamster sprite and dialogue shows on screen
    IEnumerator ShowHamsterSpriteandDialogue()
    {
        //hamsterSprite.SetActive(true);
        hamsterSpriteAnim.SetBool("canAccel", true);
        speechBubble.SetActive(true);

        yield return new WaitForSeconds(5f);

        //hamsterSprite.SetActive(false);
        hamsterSpriteAnim.SetBool("canAccel", false);
        speechBubble.SetActive(false);
    }

    // Show speed lines effect briefly during accel
    IEnumerator ShowSpeedLines()
    {
        speedLines.SetActive(true);
        yield return new WaitForSeconds(speedLinesTime);
        speedLines.SetActive(false);
    }

    void SetDefault()
    {
        currentCutoff = 0;
        dollyCart.m_Speed = baseSpeed;
        player.SetXSpeed(9.0f);
        speedText.text = ((int)(MAX_SPEED * (1.0f / 8))).ToString();
        slider.value = MAX_SLIDER_VALUE * 1 / 6;
        animSpeed = ANIM_SPEED_0;
        hamsterAnim.speed = animSpeed;

        OnSpeedChanged?.Invoke(currentCutoff);

        if (!wasHitRecently)
        {
            PlayAccelerationEffects();
        }
    }

    void SetSpeed1()
    {
        currentCutoff = 1;

        dollyCart.m_Speed = SPEED_1;
        player.SetXSpeed(10.0f);
        slider.value = MAX_SLIDER_VALUE * 1 / 3;
        speedText.text = ((int)(MAX_SPEED * (1.0f / 4))).ToString();
        animSpeed = ANIM_SPEED_1;
        hamsterAnim.speed = animSpeed;

        OnSpeedChanged?.Invoke(currentCutoff);

        if (!wasHitRecently)
        {
            PlayAccelerationEffects();
        }
    }

    void SetSpeed2()
    {
        currentCutoff = 2;

        dollyCart.m_Speed = SPEED_2;
        player.SetXSpeed(11.0f);
        slider.value = MAX_SLIDER_VALUE * 2 / 3;
        speedText.text = ((int)(MAX_SPEED * (2.0f / 4))).ToString();
        animSpeed = ANIM_SPEED_2;
        hamsterAnim.speed = animSpeed;

        OnSpeedChanged?.Invoke(currentCutoff);

        if (!wasHitRecently)
        {
            PlayAccelerationEffects();
        }
    }

    void SetSpeed3()
    {
        currentCutoff = 3;
        dollyCart.m_Speed = SPEED_3;
        player.SetXSpeed(12.0f);
        slider.value = MAX_SLIDER_VALUE * 3 / 3;
        speedText.text = ((int)(MAX_SPEED * (3.0f / 4))).ToString();
        animSpeed = ANIM_SPEED_3;
        hamsterAnim.speed = animSpeed;

        OnSpeedChanged?.Invoke(currentCutoff);

        if (!wasHitRecently)
        {
            PlayAccelerationEffects();
        }
    }

    private void OnEnable()
    {
        PlayerMovement.OnPlayerWins += ResetAnimSpeed;
    }

    private void OnDisable()
    {
        PlayerMovement.OnPlayerWins -= ResetAnimSpeed;
    }


    // When the player reaches the end of the level, reset player's animation speed
    void ResetAnimSpeed()
    {
        hamsterAnim.speed = 1;
    }

    public static event Action<int> OnSpeedChanged;
}
