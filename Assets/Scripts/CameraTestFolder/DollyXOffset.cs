using UnityEngine;

public class DollyXOffset : MonoBehaviour
{
    [SerializeField] private Transform target; // The character or object the camera should face.

    // The lateral offset value that you want to apply (can be updated dynamically from your controller)
    public float xOffset = 0f;

    // Smoothing settings for the offset
    public float smoothTime = 0.3f;
    private Vector3 offsetVelocity = Vector3.zero;
    private Vector3 lastAppliedOffset = Vector3.zero;

    // Smoothing for rotation
    public float rotationSpeed = 5f;

    void LateUpdate()
    {
        // Assume that at the beginning of LateUpdate, the dolly track has already set the camera's base position.
        // Remove the previously applied offset so we work with the true base position.
        Vector3 basePosition = transform.position - lastAppliedOffset;

        // Calculate the desired offset in world space.
        // Here, transform.rotation * Vector3.right gives the current right vector.
        Vector3 desiredOffset = transform.rotation * Vector3.right * xOffset;

        // Smoothly interpolate from the last applied offset to the desired offset.
        Vector3 newOffset = Vector3.SmoothDamp(lastAppliedOffset, desiredOffset, ref offsetVelocity, smoothTime);

        // Update the camera's position relative to the base dolly position.
        transform.position = basePosition + newOffset;

        // Update our record of the applied offset.
        lastAppliedOffset = newOffset;

        // Now, make the camera always face the target.
        if (target != null)
        {
            // Compute the rotation needed to look at the target.
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
            // Smoothly interpolate to that rotation.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
