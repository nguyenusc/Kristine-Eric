using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Jump Fields")]
    [SerializeField] AnimationCurve jumpCurve;
    [SerializeField] float jumpHeight = 3f;
    [SerializeField] float jumpDuration = 1.0f;
    //[SerializeField] float shortenedJumpFactor = 2f;
    [SerializeField] GameObject parentGO;

    bool isSeparated;
    bool isJumping;
    bool fastFallTriggered;

    Vector3 originalPosition;
    PlayerAccelerate accelerate;
    PlayerMovement playerMovement;
    PlayerKnockback knockback;

    void Awake()
    {
        originalPosition = transform.localPosition;
    }

    void Start()
    {
        accelerate = GetComponentInParent<PlayerAccelerate>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        knockback = GetComponentInParent<PlayerKnockback>();
    }

    private void OnEnable()
    {
        PlayerMovement.OnPlayerJump += SeparateBall;
        HamsterMovement.OnFastFall += FastFall;
        HamsterMovement.OnLanding += OnHamsterLanded;
    }

    private void OnDisable()
    {
        PlayerMovement.OnPlayerJump -= SeparateBall;
        HamsterMovement.OnFastFall -= FastFall;
        HamsterMovement.OnLanding -= OnHamsterLanded;
    }

    // If Player GO jumps, isSeparated bool turns true to start the Jump coroutine
    void SeparateBall()
    {
        if (!isJumping)
        {
            isSeparated = true;
            isJumping = true;
            StartCoroutine(Jump());
        }
    }

    // If Hamster lands, reset isSeparated
    void OnHamsterLanded()
    {
        isSeparated = false;
    }

    IEnumerator Jump()
    {
        float elapsedTime = 0f;
        float startY = originalPosition.y;
        fastFallTriggered = false;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float curveTime = elapsedTime / jumpDuration;

            float heightOffset = jumpCurve.Evaluate(curveTime) * jumpHeight;

            // NOTE: Edited out fast fall since it's currently buggy for hamster       
            //if (fastFallTriggered)
            //{
            //    elapsedTime += Time.deltaTime * shortenedJumpFactor;
            //}

            // Snap back ball to starting y position after jump in case of displacement
            transform.localPosition = new Vector3(originalPosition.x, startY + heightOffset, originalPosition.z);
            yield return null;
        }

        isJumping = false;
        transform.localPosition = originalPosition;
    }

    // Fast fall method to be called when the Hamster triggers fast fall
    public void FastFall()
    {
        if (!fastFallTriggered)
        {
            fastFallTriggered = true;
        }
    }

    // BY DEFAULT Hamster handles food triggers on the ground
    // Ball will only detect triggers if separated
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Food>() && isSeparated)
        {
            //Debug.Log("collection from ball");
            playerMovement.CollectFood(other);
        }

        // Ball will PHYSICALLY knockback entire player GO if it detects an obstacle while attached to hamster
        if (other.GetComponent<Obstacle>() && !isSeparated)
        {
            Debug.Log("ball detecting obstacle");
            knockback.Knockback(true, parentGO);
        }

        // When ball detects collision apart from the hamster 
        // Ball has no physical knockback, flashes red, and plays unique SFX 
        if (other.GetComponent<Obstacle>() && isSeparated)
        {
            Debug.Log("ball detecting obstacle apart from hamster");
            knockback.Knockback(false, gameObject, true);
        }
    }
}
