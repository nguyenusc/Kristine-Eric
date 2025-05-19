using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class MotorbikeLikeCameraPositionController : MonoBehaviour
{
    // Reference to the Cinemachine Virtual Camera.
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    // Reference to the player's transform.
    // (If this script is attached to the player, you can use "transform".)
    [SerializeField] private Transform CameraCenterTransform;

    // Multipliers to scale the effect based on displacement.
    [SerializeField] private float dutchMultiplier = 2.0f;
    [SerializeField] private float offsetMultiplier = 1.0f;
    // Smoothing time for SmoothDamp.
    [SerializeField] private float smoothTime = 0.3f;
    // Clamp limits.
    [SerializeField] private float maxDutch = 30f;       // Maximum tilt angle in degrees.
    [SerializeField] private float maxOffsetX = 10f;     // Maximum horizontal offset.

    // Cached reference to the Cinemachine Transposer component.
    //private CinemachineTransposer transposer;
    // Velocity variables for SmoothDamp.
    private float currentDutchVelocity = 0f;
    //private float currentOffsetXVelocity = 0f;
    // Baseline x position that represents the "neutral" state.
    private Vector3 baselineXZ;
    private Vector3 playerXZ;
    private float baselineDiff;
    private Vector3 crossProd;

    void Start()
    {


        // Set the baseline to the player's starting x-z position.
        baselineXZ = new Vector3(CameraCenterTransform.localPosition.x, 0, CameraCenterTransform.localPosition.z);
        playerXZ = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        baselineDiff = Vector3.Distance(playerXZ, baselineXZ);
        crossProd = Vector3.Cross(baselineXZ, playerXZ);

        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera is not assigned!");
        }
        /*
        else
        {
            //transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer == null)
            {
                Debug.LogError("Cinemachine Transposer component not found on the Virtual Camera!");
            }
            else
            {
                Debug.Log("Cinemachine Transposer component found.");
            }
        }
        */
    }

    void Update()
    {
        /*
        if (playerTransform == null || virtualCamera == null || transposer == null)
            return;
        */

        // Compute the lateral displacement from the baseline.
        playerXZ = new Vector2(transform.localPosition.x, transform.localPosition.z);
        float displacement = Vector2.Distance(playerXZ, baselineXZ);
        crossProd = Vector3.Cross(baselineXZ, playerXZ);
        if (crossProd.y < 0) displacement *= -1;
        Debug.Log("Pos: " + playerXZ + baselineXZ);
        Debug.Log("Player Displacement: " + displacement);

        // Calculate the target Dutch angle from displacement.
        float targetDutch = Mathf.Clamp(displacement * dutchMultiplier, -maxDutch, maxDutch);
        // Smoothly transition to the target Dutch angle.
        float newDutch = Mathf.SmoothDamp(virtualCamera.m_Lens.Dutch, targetDutch, ref currentDutchVelocity, smoothTime);
        virtualCamera.m_Lens.Dutch = newDutch;
        Debug.Log("Updated Dutch Angle: " + virtualCamera.m_Lens.Dutch);


        // Calculate the target x-offset for the camera's follow offset.
        // Assuming you have a reference to the DollyXOffset script:
        DollyXOffset offsetScript = virtualCamera.GetComponent<DollyXOffset>();
        if (offsetScript != null)
        {
            // Compute x-offset based on your logic. For example:
            float computedXOffset = Mathf.Clamp(displacement * offsetMultiplier, -maxOffsetX, maxOffsetX);
            offsetScript.xOffset = computedXOffset;
        }

        /*
        float targetOffsetX = Mathf.Clamp(displacement * offsetMultiplier, -maxOffsetX, maxOffsetX);
        Vector3 currentOffset = transposer.m_FollowOffset;
        float newOffsetX = Mathf.SmoothDamp(currentOffset.x, targetOffsetX, ref currentOffsetXVelocity, smoothTime);
        currentOffset.x = newOffsetX;
        transposer.m_FollowOffset = currentOffset;
        Debug.Log("Updated Follow Offset X: " + transposer.m_FollowOffset.x);
        */
    }
}
