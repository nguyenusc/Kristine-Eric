using UnityEngine;

public class BallBounce : MonoBehaviour
{
    [SerializeField] float bounceMultiplier = 0.5f; // Bounce damping factor
    private Rigidbody rb;

    void Start()
    {
        // Get the Rigidbody component attached to this GameObject
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("BallBounce: No Rigidbody component found on " + gameObject.name);
        }
    }

    // Called when a collision occurs
    void OnCollisionEnter(Collision collision)
    {
        // Log the collision for debugging purposes
        Debug.Log("BallBounce: Collided with " + collision.gameObject.name);

        // Check if the collided object is tagged as "Ground"
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("BallBounce: Hit Ground. Current velocity: " + rb.velocity);

            // Only bounce if the ball is moving downward
            if (rb.velocity.y < 0)
            {
                // Calculate new upward velocity using the bounce multiplier:
                // newYVelocity = -rb.velocity.y * bounceMultiplier
                float newYVelocity = -rb.velocity.y * bounceMultiplier;
                rb.velocity = new Vector3(rb.velocity.x, newYVelocity, rb.velocity.z);
                Debug.Log("BallBounce: Bouncing! New velocity: " + rb.velocity);
            }
        }
    }
}
