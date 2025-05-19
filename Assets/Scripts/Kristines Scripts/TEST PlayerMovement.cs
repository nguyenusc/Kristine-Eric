
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UI;
using ChristinaCreatesGames.Animations;
using System;

public class TESTPlayerMovement : MonoBehaviour
{
    Vector2 moveInput;
    [SerializeField] float moveSpeed;

    [Header("Jump Fields")]
    [SerializeField] float jumpForce = 2f;
    [SerializeField] float jumpCooldown = 1f;

    [Header("Manual Rotation Fields")]
    [SerializeField] float pitchAngle;      //x
    [SerializeField] float yawAngle;        //y
    [SerializeField] float rollAngle;       //z
    [SerializeField] float rotationSpeed = 8.5f;

    [Header("Movement Range Within Camera Fields")]
    [Range(0, 1)]
    [SerializeField] float minXPos;
    [Range(0, 1)]
    [SerializeField] float maxXPos;
    [Range(0, 1)]
    [SerializeField] float minYPos;
    [Range(0, 1)]
    [SerializeField] float maxYPos;

    [Header("Death Effect")]
    [SerializeField] ParticleSystem deathEffect;
    //[SerializeField] float cycleLength = 2f;

    [Header("Cameras")]
    [SerializeField] CinemachineVirtualCamera defaultCamera;
    [SerializeField] CinemachineVirtualCamera sideCamera;
    [SerializeField] CinemachineVirtualCamera frontCamera;
    [SerializeField] CinemachineVirtualCamera goalCamera;


    [Header("UI")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] GameObject endingPanel;
    [SerializeField] TextMeshProUGUI finalScoreText;
    int scoreCount = 0;
    public int GetScore() { return scoreCount; }

    AudioManager audioM;
    Rigidbody rb;
    Animator animator;
    SkinnedMeshRenderer smr;
    CinemachineDollyCart playerSpline;
    SquashAndStretch squash;
    PlayerKnockback knockback;

    bool isGrounded = true;
    bool canAccelerate;

    float lastJumpTime;

    public bool GetIsGrounded() { return isGrounded; }
    public float GetMinX() { return minXPos; }
    public float GetMaxX() { return maxXPos; }
    public float GetXSpeed() { return moveSpeed; }
    public void SetXSpeed(float speed) { moveSpeed = speed; }

    void Start()
    {
        squash = GetComponent<SquashAndStretch>();
        audioM = FindAnyObjectByType<AudioManager>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        smr = GetComponentInChildren<SkinnedMeshRenderer>();
        playerSpline = GetComponentInParent<CinemachineDollyCart>();
        knockback = GetComponent<PlayerKnockback>();

        SetCameraPriority(defaultCamera);
    }

    void Update()
    {
        MovePlayer();

        ProcessRotation();

        //CheckAccelerate();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    //player has to move LOCAL relative to the parented GO moving on the spline
    void MovePlayer()
    {
        CinemachineVirtualCamera activeCamera = GetActiveCamera();

        if (moveInput != Vector2.zero)
        {

            // If Camera is behind, translate left/right inputs to X axis
            // Disable up/down input
            if (activeCamera == defaultCamera)
            {
                transform.localPosition += Time.deltaTime * moveSpeed * new Vector3(moveInput.x, 0, 0);
            }

            // If Camera is to the side, translate left/right inputs to Z axis
            // Disable up/down input
            if (activeCamera == sideCamera)
            {
                Vector3 newPosition = transform.localPosition;
                newPosition.x = Mathf.Lerp(transform.localPosition.x, -1f, 2f);

                //newPosition.y += Time.deltaTime * moveSpeed * moveInput.y;
                newPosition.z += Time.deltaTime * moveSpeed * -moveInput.x;

                transform.localPosition = newPosition;
            }

            // If Camera is in front, flip left/right input on X axis
            // Disable up/down input
            if (activeCamera == frontCamera)
            {
                transform.localPosition += Time.deltaTime * moveSpeed * new Vector3(-moveInput.x, 0, 0);
            }

            ClampPosition(transform);
        }

        // If Player is at goal, stop moving on spline
        // Set Jump Animation, free player from Camera bounds
        if (activeCamera == goalCamera)
        {
            PlayerWins();
            knockback.enabled = false;
            ShowScore();
        }
    }

    //EVENT currently set up for TIMER
    public static event Action OnPlayerWins;

    public static void PlayerWins()
    {
        OnPlayerWins?.Invoke();
    }




    // Shown at the end of the level
    // Displays score and time
    void ShowScore()
    {
        playerSpline.enabled = false;
        animator.SetBool("isAtGoal", true);
        string blendShapeName = "eyes.squint";
        int index = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
        smr.SetBlendShapeWeight(index, 100);

        GetComponent<PlayerInput>().enabled = false;

        scoreText.gameObject.SetActive(false);
        endingPanel.gameObject.SetActive(true);
        finalScoreText.text = scoreCount.ToString();
    }



    void OnTriggerEnter(Collider other)
    {
        // Score counter
        if (other.gameObject.GetComponent<Food>())
        {
            Destroy(other.gameObject);
            audioM.PlayCollectSFX();
            scoreText.text = scoreCount + " / 30";
            scoreCount++;
        }
        if (other.gameObject.GetComponent<Enemy>())
        {
            KnockBack();
        }
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

        //Instantiate(deathEffect, transform.position, Quaternion.identity);
        audioM.PlayDeathSFX();
    }

    void SetCameraPriority(CinemachineVirtualCamera newCamera)
    {
        defaultCamera.Priority = 10;
        sideCamera.Priority = 10;
        frontCamera.Priority = 10;

        newCamera.Priority = 20;
    }

    // Returns the camera with highest priority
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
        if (defaultCamera.Priority > frontCamera.Priority && defaultCamera.Priority > frontCamera.Priority)
        {
            return defaultCamera;
        }
        else
        {
            return goalCamera;
        }
    }

    //clamps players position within camera view
    void ClampPosition(Transform transform)
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp(pos.x, minXPos, maxXPos);
        pos.y = Mathf.Clamp(pos.y, minYPos, maxYPos);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }

    // Apply artificial rotation to Player based on directional input
    void ProcessRotation()
    {
        //positive degree is left roll, negative degree is right roll, hence the negative rollAngle
        float roll = -rollAngle * moveInput.x;

        //positive yaw is right, negative yaw is left, negative angle
        float yaw = -yawAngle * moveInput.x;

        //positive pitch is down, negative pitch is up, hence negative pitchAngle
        float pitch = -pitchAngle * moveInput.y;

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, roll);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    //EVENT current set up for HAMSTER MOVEMENT
    public static event Action OnPlayerJump;

    public static void PlayerJump()
    {
        Debug.Log("player jump event triggered");
        OnPlayerJump?.Invoke();
    }

    void OnBounce(InputValue input)
    {
        // Preventing double jump with timer
        if (input.isPressed && isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            isGrounded = false;
            Jump();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Ground>())
        {
            isGrounded = true;
        }
    }

    void Jump()
    {
        Debug.Log("jumped");
        squash.PlaySquashAndStretch();
        audioM.PlayBounceSFX();

        PlayerJump();       // JUMP EVENT 

        rb.velocity += new Vector3(0, jumpForce, 0);
        lastJumpTime = Time.time;
    }

}
