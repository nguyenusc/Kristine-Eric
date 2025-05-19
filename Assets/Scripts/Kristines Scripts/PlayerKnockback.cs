using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class PlayerKnockback : MonoBehaviour
{
    PlayerMovement playerMovement;
    Animator animator;
    AudioManager audioManager;
    Scorekeeper scorekeeper;
    CameraShakeController cameraShake;

    [SerializeField] Rigidbody rb;
    [SerializeField] float strength;
    [SerializeField] float delay;
    [SerializeField] float cooldownDuration;
    [SerializeField] float adjustmentSpeed;

    [Header("Damage Flash")]
    [SerializeField] SkinnedMeshRenderer hamsterSKM;
    [SerializeField] MeshRenderer ballMesh;
    [SerializeField] float blinkIntensity;
    [SerializeField] float blinkDuration;

    [Header("Camera Shake")]
    [SerializeField] float shakeIntensity;
    [SerializeField] float shakeTime;

    [Header("Food Knockback")]
    [SerializeField] GameObject foodPrefab;
    [SerializeField] int foodCount;
    [SerializeField] float arcHeight;
    [SerializeField] float arcDuration;
    [SerializeField] float arcSpread;

    [Header("Arc Curve")]
    [SerializeField] AnimationCurve yArcCurve;

    bool isOnCooldown = false;
    float initialCameraDist;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        audioManager = FindObjectOfType<AudioManager>();
        scorekeeper = FindObjectOfType<Scorekeeper>();
        cameraShake = FindObjectOfType<CameraShakeController>();

        initialCameraDist = transform.position.z - Camera.main.transform.position.z;
        AdjustPlayerZPosition();
    }

    void Update()
    {
        // Keep player on proper Z plane
        if (Mathf.Abs(GetCurrentCameraDistance() - initialCameraDist) > 0.1f)
        {
            AdjustPlayerZPosition();
        }
    }

    // !! Knockback handles score decrementation !!
    // bool handles if object should be physically knocked back or not
    // obj determines which obj is knocked/spun
    public void Knockback(bool isPhysicallyKnocked, GameObject obj, bool useBallFlash = false)
    {
        // Do not execute function if still on cooldown
        if (isOnCooldown) return;

        scorekeeper.DecreaseScoreOnHit(); // Call Scorekeeper to update score -- do not alter score directly

        isOnCooldown = true;
        playerMovement.enabled = false;

        // Only physically knockback player if...
        if (isPhysicallyKnocked)
        {
            Vector3 pos = Camera.main.WorldToViewportPoint(obj.transform.position);
            float middleOfScreen = (playerMovement.GetMinX() + playerMovement.GetMaxX()) / 2;
            Vector3 direction = (pos.x < middleOfScreen) ? transform.right : -transform.right;

            Vector3 knockbackTarget = obj.transform.position + direction * strength;
            obj.transform.DOMove(knockbackTarget, delay).SetEase(Ease.OutCubic);
        }

        // Select the renderer to flash given the bool useBallFlash
        Renderer mesh = null;
        if (useBallFlash)
        {
            mesh = ballMesh;
            audioManager.PlayBallImpactSFX();
        }
        else
        {
            // Set hamster animation to isHit
            animator.SetBool("isHit", true);
            mesh = hamsterSKM;
            audioManager.PlayDeathSFX();
        }

        StartCoroutine(CooldownTimer());
        StartCoroutine(ResetAnimation());
        StartCoroutine(FlashRed(blinkIntensity, blinkDuration, mesh));

        // Intensity and frequency
        cameraShake.ShakeCamera(shakeIntensity, shakeTime);

        // Food knocked out 
        SpawnFoodArc(obj);

        // Spin on Y axis - Obj returns to default rotation after spinning using a lambda / Action
        // Euler works more consistently to reset hamster rotation than Quaternion.identity
        obj.transform.DORotate(new Vector3(0, 720, 0), 1f, RotateMode.FastBeyond360)
         .SetEase(Ease.OutQuad)
         .SetRelative().OnComplete(() =>
         {
             obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
         });

        StartCoroutine(Reset());
    }

    public void SpawnFoodArc(GameObject obj)
    {
        Vector3 origin = obj.transform.position + Vector3.up * 0.5f;

        for (int i = 0; i < foodCount; i++)
        {
            GameObject food = Instantiate(foodPrefab, origin, Quaternion.identity);

            // Launch direction of food should be randomized in a circle
            float angle = Random.Range(0f, 360f);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)).normalized;

            float distance = Random.Range(arcSpread * 0.7f, arcSpread * 1.4f);
            Vector3 endPos = origin + direction * distance;

            float height = Random.Range(arcHeight * 0.8f, arcHeight * 1.5f);

            // Get actual length of the animation curve
            float duration = yArcCurve[yArcCurve.length - 1].time;

            StartCoroutine(AnimateArcWithCurve(food, origin, endPos, height, duration));
        }
    }

    IEnumerator AnimateArcWithCurve(GameObject food, Vector3 startPos, Vector3 endPos, float height, float duration)
    {
        float timeElapsed = 0f;

        Vector3 randomSpin = new Vector3(
            Random.Range(360f, 1440f),
            Random.Range(360f, 1440f),
            Random.Range(360f, 1440f)
        );

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;

            Vector3 flatPos = Vector3.Lerp(startPos, endPos, t);
            float yOffset = yArcCurve.Evaluate(timeElapsed) * height;

            food.transform.position = new Vector3(flatPos.x, startPos.y + yOffset, flatPos.z);
            food.transform.Rotate(randomSpin * Time.deltaTime);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(food, 1f);
    }

    IEnumerator FlashRed(float intensity, float duration, Renderer mesh)
    {
        // Get correct property depending on mesh renderer or skm
        string color;
        if (mesh.material.HasProperty("_BaseColor"))
        {
            color = "_BaseColor";
        }
        else
        {
            color = "_Color";
        }

        Color startColor = mesh.material.GetColor(color);
        Color flashColor = Color.red;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float lerp = Mathf.Clamp01(timer / duration);
            Color currentColor = Color.Lerp(flashColor, startColor, lerp);
            mesh.material.SetColor(color, currentColor);
            yield return null;
        }
    }

    IEnumerator Reset()
    {
        yield return new WaitForSeconds(delay);
        playerMovement.enabled = true;
    }

    IEnumerator ResetAnimation()
    {
        yield return new WaitForSeconds(0.3f);
        animator.SetBool("isHit", false);
    }

    IEnumerator CooldownTimer()
    {
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }

    float GetCurrentCameraDistance()
    {
        return transform.position.z - Camera.main.transform.position.z;
    }

    void AdjustPlayerZPosition()
    {
        float targetZ = Camera.main.transform.position.z + initialCameraDist;
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, targetZ);
        transform.position = Vector3.Lerp(transform.position, targetPosition, adjustmentSpeed * Time.deltaTime);
    }
}
