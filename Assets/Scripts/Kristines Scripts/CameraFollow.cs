// ** THE FOLLOWING CODE IS FROM MIX AND JAM'S STAR FOX RAIL MOVEMENT TUTORIAL:
// https://www.youtube.com/watch?v=JVbr7osMYTo&t=123s

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Space]

    [Header("Offset")]
    public Vector3 offset = Vector3.zero;

    [Space]

    [Header("Limits")]
    public Vector2 limits = new Vector2(5, 3);

    [Space]

    [Header("Smooth Damp Time")]
    [Range(0, 1)]
    public float smoothTime;

    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        if (!Application.isPlaying)
        {
            transform.localPosition = offset;
        }

        FollowTarget(target);
    }

    void LateUpdate()
    {
        Vector3 localPos = transform.localPosition;

        transform.localPosition = new Vector3(Mathf.Clamp(localPos.x, -limits.x, limits.x), Mathf.Clamp(localPos.y, -limits.y, limits.y), localPos.z);
    }

    public void FollowTarget(Transform t)
    {
        Vector3 localPos = transform.localPosition;

        // Convert target's world position to local position relative to CameraFollow's parent
        Vector3 targetLocalPos = transform.parent.InverseTransformPoint(t.position);

        // If in Play Mode, use SmoothDamp; otherwise, set position directly
        if (Application.isPlaying)
        {
            transform.localPosition = Vector3.SmoothDamp(
                localPos,
                new Vector3(targetLocalPos.x + offset.x, targetLocalPos.y + offset.y, localPos.z),
                ref velocity,
                smoothTime
            );
        }
        else
        {
            transform.localPosition = new Vector3(targetLocalPos.x + offset.x, targetLocalPos.y + offset.y, localPos.z);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-limits.x, -limits.y, transform.position.z), new Vector3(limits.x, -limits.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(-limits.x, limits.y, transform.position.z), new Vector3(limits.x, limits.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(-limits.x, -limits.y, transform.position.z), new Vector3(-limits.x, limits.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(limits.x, -limits.y, transform.position.z), new Vector3(limits.x, limits.y, transform.position.z));
    }
}