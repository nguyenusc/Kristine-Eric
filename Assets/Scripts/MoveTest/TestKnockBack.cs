using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TestPlayerKnockback : MonoBehaviour
{
    TestPlayerMovement playerMovement;
    Animator animator;

    [SerializeField] Rigidbody rb;
    [SerializeField] float strength = 5f;
    [SerializeField] float delay = 0.15f;
    [SerializeField] float cooldownDuration = 1f;

    [SerializeField] float adjustmentSpeed = 2f;

    public UnityEvent OnBegin, OnDone;

    private bool isOnCooldown = false;
    private float initialCameraDist;

    void Start()
    {
        playerMovement = GetComponent<TestPlayerMovement>();
        animator = GetComponent<Animator>();

        // Calculate the initial distance between the player and the camera
        initialCameraDist = transform.position.z - Camera.main.transform.position.z;

        AdjustPlayerZPosition(); // Set initial Z position based on the calculated distance
    }


    // This will have to be adjusted if we include different camera angles
    private void Update()
    {
        // Adjust player's Z position only if needed (after knockback)
        if (Mathf.Abs(GetCurrentCameraDistance() - initialCameraDist) > 0.1f)
        {
            AdjustPlayerZPosition();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isOnCooldown && other.GetComponent<Enemy>())
        {
            PlayFeedback(other.gameObject);
        }
    }

    public void PlayFeedback(GameObject sender)
    {
        StopAllCoroutines();
        OnBegin?.Invoke();

        isOnCooldown = true;
        StartCoroutine(CooldownTimer());
        StartCoroutine(ResetAnimation());

        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        float middleOfScreen = (playerMovement.GetMinX() + playerMovement.GetMaxX()) / 2;
        Vector3 direction = (pos.x < middleOfScreen) ? transform.right : -transform.right;

        rb.AddForce(direction * strength, ForceMode.Impulse);
        animator.SetBool("isHit", true);

        StartCoroutine(Reset());
    }

    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(delay);
        rb.velocity = Vector3.zero;
        OnDone?.Invoke();
    }

    IEnumerator ResetAnimation()
    {
        yield return new WaitForSeconds(0.3f);
        animator.SetBool("isHit", false);

    }

    private IEnumerator CooldownTimer()
    {
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }

    private float GetCurrentCameraDistance()
    {
        return transform.position.z - Camera.main.transform.position.z;
    }

    private void AdjustPlayerZPosition()
    {
        // Calculate target Z position based on the camera's position and initial distance
        float targetZ = Camera.main.transform.position.z + initialCameraDist;
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, targetZ);

        // Adjust the player's Z position
        transform.position = Vector3.Lerp(transform.position, targetPosition, adjustmentSpeed * Time.deltaTime);
    }
}