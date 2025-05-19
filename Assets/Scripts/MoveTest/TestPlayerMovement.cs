using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UI;
using ChristinaCreatesGames.Animations;

public class TestPlayerMovement : MonoBehaviour
{
    // ----------------------------
    // INPUT & MOVEMENT VARIABLES
    // ----------------------------
    Vector2 moveInput;                                // Stores player input from the controller/keyboard

    [Header("Side Movement Fields")]
    [SerializeField] float maxMoveSpeed = 5f;          // Maximum horizontal movement speed
    [SerializeField] float acceleration = 10f;         // How fast the player accelerates
    [SerializeField] float deceleration = 15f;         // How fast the player decelerates
    private float currentSpeed = 0f;                   // Current speed used for smooth acceleration

    // ----------------------------
    // JUMP & GRAVITY VARIABLES
    // ----------------------------
    [Header("Variable Jump Settings")]
    [SerializeField] float fullJumpTime = 0.5f;        // The max time (seconds) for a full jump hold
    [SerializeField] float minJumpForce = 3f;          // Minimum jump force for a quick tap
    [SerializeField] float fullJumpForce = 6f;         // Maximum jump force when held for fullJumpTime
    private float jumpStartTime;                       // Records time when jump button is pressed

    [SerializeField] float gravityMultiplier = 2.5f;   // Multiplier for gravity when falling
    [SerializeField] float lowJumpMultiplier = 2f;     // Multiplier when jump button is released early

    [SerializeField] float coyoteTime = 0.1f;          // Allow jump for a short time after leaving ground
    [SerializeField] float jumpBufferTime = 0.2f;      // Allow jump input slightly before landing
    private float coyoteTimeCounter;                   // Countdown for coyote time
    private float jumpBufferCounter;                   // Countdown for jump buffering
    public bool isJumping = false;                    // Tracks if the player is mid-jump
    private bool jumpButtonReleased = true; // Ensure this is true at start

    // ----------------------------
    // MANUAL ROTATION VARIABLES
    // ----------------------------
    [Header("Manual Rotation Fields")]
    [SerializeField] float pitchAngle;                 // Pitch angle adjustment (x-axis)
    [SerializeField] float yawAngle;                   // Yaw angle adjustment (y-axis)
    [SerializeField] float rollAngle;                  // Roll angle adjustment (z-axis)
    [SerializeField] float rotationSpeed = 8.5f;       // Speed at which rotation is smoothed

    // ----------------------------
    // CAMERA & POSITION LIMITS
    // ----------------------------
    [Header("Movement Range Within Camera Fields")]
    [Range(0, 1)]
    [SerializeField] float minXPos;                    // Minimum X (viewport) position allowed
    [Range(0, 1)]
    [SerializeField] float maxXPos;                    // Maximum X (viewport) position allowed
    [Range(0, 1)]
    [SerializeField] float minYPos;                    // Minimum Y (viewport) position allowed
    [Range(0, 1)]
    [SerializeField] float maxYPos;                    // Maximum Y (viewport) position allowed

    // ----------------------------
    // EFFECTS & UI
    // ----------------------------
    [Header("Death Effect")]
    [SerializeField] ParticleSystem deathEffect;       // Particle effect for death/knockback
    [Header("UI")]
    [SerializeField] TextMeshProUGUI scoreText;        // Score text UI element
    [SerializeField] GameObject endingPanel;           // Panel displayed when level ends
    [SerializeField] TextMeshProUGUI finalScoreText;   // Final score display
    int scoreCount = 0;                                // Current score

    // ----------------------------
    // CAMERA REFERENCES
    // ----------------------------
    [Header("Cameras")]
    [SerializeField] CinemachineVirtualCamera defaultCamera;
    [SerializeField] CinemachineVirtualCamera sideCamera;
    [SerializeField] CinemachineVirtualCamera frontCamera;
    [SerializeField] CinemachineVirtualCamera goalCamera;

    // ----------------------------
    // COMPONENT REFERENCES
    // ----------------------------
    AudioManager audioM;                               // Manages audio playback
    Rigidbody rb;                                      // Player's Rigidbody for physics
    Animator animator;                                 // Animator for controlling animations
    SkinnedMeshRenderer smr;                           // For blend shapes (e.g., squint effect)
    CinemachineDollyCart playerSpline;                 // For following a path/dolly track
    SquashAndStretch squash;                           // For jump squash & stretch

    // ----------------------------
    // STATE VARIABLES
    // ----------------------------
    bool isGrounded = true;                            // Tracks if the player is on the ground
    bool isAtGoal = false;
    // ----------------------------
    // UNITY METHODS
    // ----------------------------
    void Start()
    {
        // Initialize references...
        squash = GetComponent<SquashAndStretch>();
        audioM = FindAnyObjectByType<AudioManager>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        smr = GetComponentInChildren<SkinnedMeshRenderer>();
        playerSpline = GetComponentInParent<CinemachineDollyCart>();

        // Set default camera priority
        SetCameraPriority(defaultCamera);

        // Explicitly allow the first jump.
        jumpButtonReleased = true;
    }

    void Update()
    {

        MovePlayer();          // Handle horizontal movement with acceleration/deceleration
        ProcessRotation();     // Apply manual rotation
        ApplyBetterGravity();  // Adjust gravity for better jump/fall feel
        HandleJumpBuffer();    // Manage coyote time & jump buffering (no old input calls)
    }

    // ----------------------------
    // INPUT METHODS (New Input System)
    // ----------------------------
    // Called when movement input is received
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Called by the Input System to handle jump input
    void OnBounce(InputValue input)
    {
        if (input.isPressed)
        {
            // Only trigger a new jump if the button was previously released and the alternative ground condition is met.
            if (jumpButtonReleased || (coyoteTimeCounter > 0 && Mathf.Abs(rb.velocity.y) < 0.1f))
            {
                jumpStartTime = Time.time;
                isJumping = true;
                jumpButtonReleased = false;

            }
        }
        else // When the jump button is released
        {
            if (isJumping && isGrounded)
            {
                // Calculate hold time (clamped to fullJumpTime)
                float holdTime = Mathf.Clamp(Time.time - jumpStartTime, 0, fullJumpTime);
                // Compute a logarithmic interpolation factor (0 to 1)
                float factor = Mathf.Log(1 + holdTime) / Mathf.Log(1 + fullJumpTime);
                // Interpolate between minJumpForce and fullJumpForce using the factor
                float effectiveForce = Mathf.Lerp(minJumpForce, fullJumpForce, factor);
                // Apply the calculated jump force.
                rb.velocity = new Vector3(rb.velocity.x, effectiveForce, rb.velocity.z);
                isJumping = false;
            }
            // Mark that the button has been released so a new jump can be triggered.
            jumpButtonReleased = true;
        }
    }


    // ----------------------------
    // MOVEMENT & ROTATION
    // ----------------------------
    void MovePlayer()
    {

        CinemachineVirtualCamera activeCamera = GetActiveCamera();


        // Accelerate/decelerate horizontally
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                moveInput.x * maxMoveSpeed,
                acceleration * Time.deltaTime
            );
        }
        else
        {
            // Exponential decay for quick slowdown if no input
            currentSpeed *= Mathf.Pow(0.9f, Time.deltaTime * deceleration);
        }

        // Apply movement based on active camera
        if (activeCamera == defaultCamera)
        {
            // Default camera: move along local X
            transform.localPosition += Time.deltaTime * new Vector3(currentSpeed, 0, 0);
        }
        else if (activeCamera == sideCamera)
        {
            // Side camera: clamp X around -1f, move along Z
            Vector3 newPosition = transform.localPosition;
            newPosition.x = Mathf.Lerp(transform.localPosition.x, -1f, 2f * Time.deltaTime);
            newPosition.z += Time.deltaTime * (-currentSpeed);
            transform.localPosition = newPosition;
        }
        else if (activeCamera == frontCamera)
        {
            // Front camera: invert X input
            transform.localPosition += Time.deltaTime * new Vector3(-currentSpeed, 0, 0);
        }

        // If not at goal camera, clamp to viewport
        if (activeCamera != goalCamera)
        {
            ClampPosition(transform);

        }
        else if (!isAtGoal)
        {
            isAtGoal = true;

            rb.velocity = Vector3.zero;
            // Show final score if at goal
            ShowScore();
        }
    }

    void ProcessRotation()
    {
        // Calculate roll, yaw, pitch based on input
        float roll = -rollAngle * moveInput.x;
        float yaw = -yawAngle * moveInput.x;
        float pitch = -pitchAngle * moveInput.y;

        // Lerp to target rotation
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, roll);
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // ----------------------------
    // JUMP & GRAVITY
    // ----------------------------
    void ApplyBetterGravity()
    {
        // If falling, apply extra downward force
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1) * Time.deltaTime;
        }
        // If jump is released early, apply low-jump multiplier
        else if (rb.velocity.y > 0 && !IsJumpButtonHeld())
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    // Check if jump button is currently held (using new input system)
    bool IsJumpButtonHeld()
    {
        // If you have an InputAction reference for jump, you can check its .ReadValue<float>()
        // For a quick fix, this returns true if isJumping hasn't ended.
        return isJumping;
    }

    // Coyote time + jump buffer logic (no old input calls!)
    void HandleJumpBuffer()
    {
        // Count down coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Count down jump buffer
        jumpBufferCounter -= Time.deltaTime;
    }

    // ----------------------------
    // COLLISION & TRIGGERS
    // ----------------------------


    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;

        }
    }
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            coyoteTimeCounter = coyoteTime;

        }
    }


    void OnTriggerEnter(Collider other)
    {
        // Score / Collectibles
        if (other.gameObject.GetComponent<Food>())
        {
            Destroy(other.gameObject);
            audioM.PlayCollectSFX();
            scoreCount++;
            scoreText.text = scoreCount + " / 30";
        }
        // Enemy knockback
        if (other.gameObject.GetComponent<Enemy>())
        {
            KnockBack();
        }
        // Camera triggers
        if (other.gameObject.GetComponent<SideCameraTrigger>())
        {
            SetCameraPriority(sideCamera);
        }
        if (other.gameObject.GetComponent<DefaultCameraTrigger>())
        {
            SetCameraPriority(defaultCamera);
        }
        if (other.gameObject.GetComponent<FrontCameraTrigger>())
        {
            SetCameraPriority(frontCamera);
        }
        if (other.gameObject.GetComponent<GoalCameraTrigger>())
        {
            SetCameraPriority(goalCamera);

        }
    }

    void KnockBack()
    {
        scoreCount -= 3;
        scoreText.text = scoreCount + " / 30";
        // Instantiate(deathEffect, transform.position, Quaternion.identity);
        audioM.PlayDeathSFX();
        //isGrounded = true;
    }

    // ----------------------------
    // UI & GAME STATE
    // ----------------------------
    void ShowScore()
    {

        playerSpline.enabled = false;
        Debug.Log(playerSpline.enabled);
        animator.SetBool("isAtGoal", true);

        string blendShapeName = "eyes.squint";
        int index = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
        smr.SetBlendShapeWeight(index, 100);

        GetComponent<PlayerInput>().enabled = false;
        scoreText.gameObject.SetActive(false);
        endingPanel.gameObject.SetActive(true);
        finalScoreText.text = scoreCount.ToString();

    }

    // ----------------------------
    // CAMERA & POSITION
    // ----------------------------
    void SetCameraPriority(CinemachineVirtualCamera newCamera)
    {
        defaultCamera.Priority = 10;
        sideCamera.Priority = 10;
        frontCamera.Priority = 10;
        newCamera.Priority = 20;
    }

    CinemachineVirtualCamera GetActiveCamera()
    {
        if (sideCamera.Priority > defaultCamera.Priority && sideCamera.Priority > frontCamera.Priority)
        {
            return sideCamera;
        }
        if (frontCamera.Priority > defaultCamera.Priority && frontCamera.Priority > sideCamera.Priority)
        {
            return frontCamera;
        }
        if (defaultCamera.Priority > frontCamera.Priority && defaultCamera.Priority > sideCamera.Priority)
        {
            return defaultCamera;
        }
        else
        {
            return goalCamera;
        }
    }

    void ClampPosition(Transform transform)
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp(pos.x, minXPos, maxXPos);
        pos.y = Mathf.Clamp(pos.y, minYPos, maxYPos);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }

    public float GetMinX() { return minXPos; }
    public float GetMaxX() { return maxXPos; }


}
