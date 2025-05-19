
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UI;
using ChristinaCreatesGames.Animations;
using System;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    Vector2 moveInput;
    [SerializeField] float moveSpeed;
    public float GetMoveSpeed() { return moveSpeed; }

    [Header("Jump Fields")]
    [SerializeField] float jumpCooldown;
    [SerializeField] float fastJumpCooldown;

    [Header("Manual Rotation Fields")]
    [SerializeField] float pitchAngle;      //x
    [SerializeField] float yawAngle;        //y
    [SerializeField] float rollAngle;       //z
    [SerializeField] float rotationSpeed;

    [Header("Movement Range Within Camera Fields")]
    [Range(0, 1)]
    [SerializeField] float minXPos;
    [Range(0, 1)]
    [SerializeField] float maxXPos;
    [Range(0, 1)]
    [SerializeField] float minYPos;
    [Range(0, 1)]
    [SerializeField] float maxYPos;

    [Header("Cameras")]
    [SerializeField] CinemachineVirtualCamera defaultCamera;
    [SerializeField] CinemachineVirtualCamera hamsterCamera;
    [SerializeField] CinemachineVirtualCamera goalCamera;
    [SerializeField] CinemachineVirtualCamera bigIceCreamCamera;
    [SerializeField] CinemachineVirtualCamera overheadCamera;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] GameObject endingPanel;
    [SerializeField] TextMeshProUGUI finalScoreText;
    int scoreCount = 0;
    public int GetScore() { return scoreCount; }
    public void SetScore(int score) { scoreCount = score; }

    AudioManager audioM;
    Animator animator;
    SkinnedMeshRenderer smr;
    CinemachineDollyCart playerSpline;
    SquashAndStretch squash;
    PlayerKnockback knockback;
    PlayerAccelerate accelerate;

    bool isGrounded = true;
    public void SetIsGrounded(bool grounded) { isGrounded = grounded; }

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
        animator = GetComponent<Animator>();
        smr = GetComponentInChildren<SkinnedMeshRenderer>();
        playerSpline = GetComponentInParent<CinemachineDollyCart>();
        knockback = GetComponent<PlayerKnockback>();
        accelerate = GetComponent<PlayerAccelerate>();

        SetCameraPriority(defaultCamera);
    }

    void Update()
    {
        MovePlayer();
        ProcessRotation();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Player has to move LOCAL relative to the parented GO moving on the spline
    void MovePlayer()
    {
        CinemachineVirtualCamera activeCamera = GetActiveCamera();

        if (moveInput != Vector2.zero)
        {
            transform.localPosition += Time.deltaTime * moveSpeed * new Vector3(moveInput.x, 0, 0);

            ClampPosition(transform);
        }

        // If Player is at goal, stop moving on spline  
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
        if (scoreCount > 30)
        {
            animator.SetBool("isAtGoal", true);
        }
        else if (scoreCount > 20)
        {
            animator.SetBool("didOK", true);
        }
        else
        {
            animator.SetBool("didFail", true);
        }
        string blendShapeName = "eyes.squint";
        int index = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
        smr.SetBlendShapeWeight(index, 100);

        GetComponent<PlayerInput>().enabled = false;

        // Set final score text
        finalScoreText.text = scoreCount.ToString();

        // Trigger event for ending UI manager script
        EndGame();
    }

    public static event Action OnEndGame;
    public static void EndGame()
    {
        OnEndGame?.Invoke();
    }

    // Set player to running and reenable controls for bonus stage
    public void BonusStageEffects()
    {
        animator.SetBool("isAtGoal", false);
        animator.SetBool("isRunning", true);

        GetComponent<PlayerInput>().enabled = true;
    }

    // TODO: left off writing food effects 
    public void CollectFood(Collider other)
    {
        DOTween.Kill(other.transform);
        Destroy(other.gameObject);
        //GameObject food = Instantiate(other.gameObject, transform.position, Quaternion.identity);
        //StartCoroutine(FoodPopEffect(food));

        audioM.PlayCollectSFX();
        scoreCount++;
    }


    IEnumerator FoodPopEffect(GameObject food)
    {
        yield return null;
    }

    // NOTE OnTrigger with obstacles and enemies is handled in Accelerate
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<GoalCameraTrigger>())
        {
            SetCameraPriority(goalCamera);
        }
    }

    public void SetCameraPriority(CinemachineVirtualCamera newCamera)
    {
        defaultCamera.Priority = 10;
        hamsterCamera.Priority = 10;
        goalCamera.Priority = 10;
        bigIceCreamCamera.Priority = 10;
        overheadCamera.Priority = 10;

        newCamera.Priority = 20;
    }

    public void SetHamsterCamera()
    {
        defaultCamera.Priority = 10;
        hamsterCamera.Priority = 20;
        goalCamera.Priority = 10;
    }

    public void SetDefaultCamera()
    {
        defaultCamera.Priority = 20;

        hamsterCamera.Priority = 10;
        goalCamera.Priority = 10;
        bigIceCreamCamera.Priority = 10;
        overheadCamera.Priority = 10;
    }

    // Returns the camera with highest priority
    // Might want to refactor Inspector to an array of VCs
    CinemachineVirtualCamera GetActiveCamera()
    {
        CinemachineVirtualCamera[] cameras = new[] {
        defaultCamera, hamsterCamera, goalCamera, bigIceCreamCamera, overheadCamera
    };

        CinemachineVirtualCamera highest = cameras[0];
        foreach (var cam in cameras)
        {
            if (cam.Priority > highest.Priority)
            {
                highest = cam;
            }
        }

        return highest;
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

    //EVENT set up for HAMSTER MOVEMENT
    public static event Action OnPlayerJump;

    public static void PlayerJump()
    {
        OnPlayerJump?.Invoke();
    }

    void OnBounce(InputValue input)
    {
        if (input.isPressed)
        {
            MetricsTracker.IncrementMetric("JumpAttempt");
        }
        if (accelerate.GetCurrentSpeed() == 0)
        {
            jumpCooldown = fastJumpCooldown;
        }
        else
        {
            jumpCooldown = 1.0f;
        }

        // Preventing double jump with timer
        if (input.isPressed && isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            isGrounded = false;
            Jump();
        }
    }

    void Jump()
    {
        squash.PlaySquashAndStretch();
        audioM.PlayBounceSFX();
        PlayerJump();       // JUMP EVENT 

        lastJumpTime = Time.time;
    }
}
