using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Linework.FastOutline;
using TMPro;
using System;
using ChristinaCreatesGames.Animations;

public class HamsterMovement : MonoBehaviour
{
    Vector2 moveInput;
    Vector3 direction = Vector3.down;

    [SerializeField] PlayerInput playerInput;
    [SerializeField] SquashAndStretch squash;
    [SerializeField] Rigidbody rb;

    Vector3 localPosOnBall;

    PlayerInput hamsterInput;
    PlayerMovement playerMovement;
    Animator animator;
    PlayerAccelerate accelerate;
    SkinnedMeshRenderer skm;
    PlayerKnockback knockback;
    AudioManager audioManager;

    [Header("Jump Fields")]
    [SerializeField] AnimationCurve jumpTest;
    [SerializeField] float jumpHeight;
    [SerializeField] float jumpDuration;
    [SerializeField] float fallSpeed;       //applied gravity
    [SerializeField] FastOutlineSettings fastOutlineSettings;   //IGNORE THE ERROR WARNINGS ASSOCIATED WITH OUTLINE - IT WORKS FINE!!
    [SerializeField] float sidewaysMoveSpeed = 9.0f;
    [SerializeField] ParticleSystem landingEffect;

    [Header("In Air Fields (currently unused)")]
    [SerializeField] float maxMoveSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float deceleration;
    float currentSpeed;

    [Header("Raycast Fields")]
    [SerializeField] float range;

    // True when event from Jump from PlayerMovement triggers
    // False - ReconnectWithBall
    bool isSeparated;
    public bool GetIsSeparated() { return isSeparated; }

    // True when hamster intersects 0 y 
    bool didHitGround;

    // Flag to prevent Jump coroutine from triggering multiple times
    // Set to true in SeparateHamster and reset at the end of Jump coroutine
    bool isJumping;

    // This bool controls when the player starts to detect the ball beneath them via raycast
    // And when they are able to fast fall
    bool isInAir;

    // Tracks the current number of jumps the player has done
    // Reset to 0 once player lands
    int currentJumps = 0;

    // Spawn food ONCE per fall
    bool hasSpawnedFood;

    // Tracks how many jumps player can perform
    // Dependant on PlayerAccelerate.OnSpeedChanged
    [SerializeField] int maxJumps = 1;
    public int GetMaxJumps() { return maxJumps; }

    // Stores reference to active jump
    // Set initially in SeparateHamster and subsequently in OnDoubleJump
    Coroutine jumpCoroutine;
    bool isJumpInterrupted = false;
    //for counting jumps failed
    private bool missedLandingCounted = false;

    void Awake()
    {
        // Cache a reference to hamster's local position on ball
        localPosOnBall = transform.localPosition;
    }

    void Start()
    {
        hamsterInput = GetComponent<PlayerInput>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        animator = GetComponentInParent<Animator>();
        accelerate = GetComponentInParent<PlayerAccelerate>();
        skm = GetComponentInChildren<SkinnedMeshRenderer>();
        knockback = GetComponentInParent<PlayerKnockback>();
        audioManager = FindObjectOfType<AudioManager>();

        fastOutlineSettings.Outlines[0].color = Color.black;
    }


    void Update()
    {
        // Ray for general ball detection
        Ray ray = new Ray(transform.position, transform.TransformDirection(direction));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction) * range, Color.red);

        // Ray for outline color
        Ray rayOutline = new Ray(transform.position, transform.TransformDirection(direction));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction) * 100, Color.yellow);

        if (isSeparated)
        {
            MoveHorizontal();

            // Raycast to detect ball collision
            // Calls Reconnect passing in TRUE because player landed on ball
            if ((Physics.Raycast(ray, out RaycastHit hit, range * 2)) && isInAir)
            {
                if (hit.collider.GetComponent<Ball>())
                {
                    ReconnectWithBall(true);
                }
            }

            // Raycast to change hamster outline color
            if ((Physics.Raycast(rayOutline, out RaycastHit hit2, 100)) && isInAir)
            {
                if (hit2.collider.GetComponent<Ball>() || hit2.collider.GetComponent<PlayerMovement>())
                {
                    fastOutlineSettings.Outlines[0].color = Color.green;
                }
                else
                {
                    fastOutlineSettings.Outlines[0].color = Color.red;
                }
            }
        }

        // After Jump coroutine finishes, apply gravity
        // Stop applying gravity when player hits the ground        
        if (isSeparated && !isJumping && !didHitGround)
        {
            AddGravity(ray);
        }

        // Hamster is not in air if its y pos passes the ball
        if (transform.localPosition.y <= localPosOnBall.y)
        {
            isInAir = false;
            fastOutlineSettings.Outlines[0].color = Color.black;
        }

        // Player hits the ground if its y pos intercepts world 0
        if (transform.localPosition.y <= -0.5f)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, -0.5f, transform.localPosition.z);
            didHitGround = true;
        }

        if (isSeparated && didHitGround)
        {
            didHitGround = false;

            StartCoroutine(BlinkOutAndRespawn());
        }
    }

    // Hamster flails in the water, flashes red, and respawns on ball
    IEnumerator BlinkOutAndRespawn()
    {
        int blinkCount = 1;  // Number of blinks
        float blinkInterval = 0.5f;  // Time between blinks

        animator.speed = 1.5f;
        animator.SetTrigger("didMissBall");

        // Player SFX and spawn food ONCE
        if (!hasSpawnedFood)
        {
            hasSpawnedFood = true;
            audioManager.PlayHamsterSplashSFX();
            knockback.Knockback(false, gameObject);

        }

        for (int i = 0; i < blinkCount; i++)
        {
            skm.material.SetColor("_BaseColor", Color.white);
            yield return new WaitForSeconds(blinkInterval);

            skm.material.SetColor("_BaseColor", Color.red);
            yield return new WaitForSeconds(blinkInterval);
        }

        // Reset color to default after blinking
        skm.material.SetColor("_BaseColor", Color.white);

        // Reset animator speed (?)
        animator.speed = 1f;

        // Reset back to ball
        // Passing in FALSE because player failed to land on ball     
        ReconnectWithBall(false);
    }


    // If Hamster misses the ball, gravity is applied until it reaches the Ground
    void AddGravity(Ray ray)
    {
        Vector3 pos = transform.localPosition;
        pos.y -= fallSpeed * Time.deltaTime;
        transform.localPosition = pos;
    }

    void OnMove(InputValue input)
    {
        moveInput = input.Get<Vector2>();
    }

    void MoveHorizontal()
    {
        Vector3 defaultVector = new Vector3(moveInput.x, 0f, 0f);

        switch (accelerate.GetCurrentSpeed())
        {
            case 0:
                transform.localPosition += defaultVector * Time.deltaTime * (sidewaysMoveSpeed / 2.5f);
                break;
            case 1:
                transform.localPosition += defaultVector * Time.deltaTime * (sidewaysMoveSpeed / 2);
                break;
            case 2:
                transform.localPosition += defaultVector * Time.deltaTime * (sidewaysMoveSpeed / 1.5f);
                break;
            case 3:
                transform.localPosition += defaultVector * Time.deltaTime * (sidewaysMoveSpeed * (2.0f / 3));
                break;
        }
    }

    // Jump sampled from animation curve 
    // Hamster is able to jump again once jump coroutine finishes by resetting flag isJumping 
    IEnumerator Jump()
    {
        MetricsTracker.IncrementMetric("JumpExecuted");
        // Check if double jump is called to immediately break from coroutine
        if (isJumpInterrupted)
        {
            isJumpInterrupted = false;
            yield break;
        }

        currentJumps++;

        float elapsedTime = 0f;
        float startY;

        if (currentJumps == 1)
        {
            startY = localPosOnBall.y;
        }
        else
        {
            startY = transform.localPosition.y;
        }

        animator.SetBool("isRolling", true);
        float originalSpeed = animator.speed;
        animator.speed = 1;

        // Set values of jump depending on which jump it is   
        switch (currentJumps)
        {
            case 1:
                jumpDuration = 1.5f;
                jumpHeight = 3.0f;
                audioManager.PlaySingleJump();
                break;
            case 2:
                jumpDuration = 1.5f;
                jumpHeight = 2.0f;
                audioManager.PlayDoubleJump();
                MetricsTracker.IncrementMetric("DoubleJump");
                break;
            case 3:
                jumpDuration = 1.5f;
                jumpHeight = 2.0f;
                audioManager.PlayTripleJump();
                MetricsTracker.IncrementMetric("TripleJump");
                break;
        }

        // Fast fall can activate if player presses 'S' after certain point in their jump
        bool fastFallTriggered = false;
        bool canFastFall = false;

        // Apply jump values to animation curve evaluation
        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float curveTime = elapsedTime / jumpDuration;

            float heightOffset = jumpTest.Evaluate(curveTime) * jumpHeight; // Sample from curve


            // Player can fast fall 1/4 of the way into their jump
            if (elapsedTime >= jumpDuration / 4)
            {
                animator.SetBool("isRolling", false);
                animator.speed = originalSpeed;
                canFastFall = true;
            }

            // Player starts raycast detection for outline color halfway into jump
            if (elapsedTime >= jumpDuration / 2 && !isInAir)
            {
                isInAir = true;
            }

            // NOTE: edited out fast fall since it's currently buggy
            //if (canFastFall && moveInput.y < 0.0f && !fastFallTriggered)
            //{
            //    FastFall();
            //    jumpDuration = elapsedTime + 0.3f; // Shorten the remaining jump time
            //    fastFallTriggered = true;
            //}

            // Evaluate curve faster for fast fall
            //if (fastFallTriggered)
            //{
            //    heightOffset -= fallSpeed * Time.deltaTime * 3.0f; // Accelerate downward
            //}

            transform.localPosition = new Vector3(transform.localPosition.x, startY + heightOffset, transform.localPosition.z);
            yield return null;
        }

        // Snap hamster back to startY after jump finishes
        transform.localPosition = new Vector3(transform.localPosition.x, startY, transform.localPosition.z);

        // Once jump is done, set isJumping to false
        isJumping = false;
    }


    void OnDoubleJump()
    {
        //Debug.Log($"OnDoubleJump called! currentJumps: {currentJumps}, maxJumps: {maxJumps}");

        if (currentJumps < maxJumps)
        {
            if (jumpCoroutine == null)
            {
                Debug.LogWarning("Jump coroutine is NULL!!");
            }
            if (jumpCoroutine != null)
            {
                isJumpInterrupted = true;
                StopCoroutine(jumpCoroutine);
            }

            isJumpInterrupted = false;

            jumpCoroutine = StartCoroutine(Jump());
        }
    }

    public static event Action OnFastFall;

    public static void FastFall()
    {
        OnFastFall?.Invoke();
    }

    void OnTriggerEnter(Collider other)
    {
        // Increment Score counter
        if (other.GetComponent<Food>())
        {
            playerMovement.CollectFood(other);
        }

        // Special Obstacles are split jump obstacles, meaning the hamster must avoid these 
        if (other.GetComponent<SpecialObstacle>())
        {
            Debug.Log("hamster detecting special obstacle");
            MetricsTracker.IncrementMetric("Knockback");
            knockback.Knockback(false, gameObject);
        }

        // Hamster detects collision with obstacle if separated with obstacles
        // only accessible to airborne hamster like flying orcas
        if (other.GetComponent<Obstacle>() && isSeparated)
        {
            Debug.Log("hamster detecting obstacle");
            MetricsTracker.IncrementMetric("Knockback");
            knockback.Knockback(false, gameObject);
        }
    }

    private void OnEnable()
    {
        PlayerMovement.OnPlayerJump += SeparateHamster;
        PlayerAccelerate.OnSpeedChanged += ChangeMaxJumps;
    }

    private void OnDisable()
    {
        PlayerMovement.OnPlayerJump -= SeparateHamster;
        PlayerAccelerate.OnSpeedChanged -= ChangeMaxJumps;
    }


    void ChangeMaxJumps(int cutoff)
    {
        //Debug.Log($"ChangeMaxJumps called with cutoff: {cutoff}");
        switch (cutoff)
        {
            case 0:
                maxJumps = 1;
                playerMovement.SetDefaultCamera();
                break;
            case 1:
                maxJumps = 2;
                playerMovement.SetHamsterCamera();
                break;
            case 2:
            case 3:
                maxJumps = 3;
                break;
        }

        // Call EVENT for JumpsUI
        JumpsChanged(maxJumps);
    }

    // On jump detected by parent player GO, controls switch from playerInput to hamsterInput
    // Jump coroutine is called - isJumping flag is set to true
    void SeparateHamster()
    {
        missedLandingCounted = false;  // Reset missed landing flag at the start of a new jump sequence.
        isSeparated = true;

        // Disable parent group input controls
        playerInput.enabled = false;

        // Enable hamster input controls
        hamsterInput.enabled = true;

        // Disable gravity on hamster RB to prevent jitter/conflict with animation curve
        rb.useGravity = false;

        hasSpawnedFood = false;

        // Regardless of speed, allow the hamster to start jumping
        if (!isJumping)
        {
            isJumping = true;
            jumpCoroutine = StartCoroutine(Jump());
        }
    }

    // If Melon lands on ball, disable hamsterInput controls and reenable default playerInput
    // Snap hamster back to position on ball
    void ReconnectWithBall(bool didLand)
    {
        hamsterInput.enabled = false;
        playerInput.enabled = true;
        isSeparated = false;
        transform.localPosition = localPosOnBall;
        currentJumps = 0;

        isJumping = false;
        isInAir = false;
        didHitGround = false;

        rb.useGravity = true;

        // isGrounded from Player is reset when Hamster connects with ball - enabling a fresh jump
        playerMovement.SetIsGrounded(true);

        // Fire event for Ball
        OnLanding();

        if (didLand)
        {
            MetricsTracker.IncrementMetric("SuccessfulLand");
            squash.PlaySquashAndStretch();
            landingEffect.Play();
        }
        else
        {
            // Only count a missed landing once per fall
            if (!missedLandingCounted)
            {
                MetricsTracker.IncrementMetric("MissedLanding");
                missedLandingCounted = true;
            }
        }
    }

    // EVENT for BALL to enable Food collision
    public static event Action OnLanding;

    public static void Landing()
    {
        OnLanding?.Invoke();
    }

    // EVENT for JumpsUI to change num jumps available to player
    public static event Action<int> OnJumpsChanged;
    public static void JumpsChanged(int jumpCount)
    {
        OnJumpsChanged?.Invoke(jumpCount);
    }

}
