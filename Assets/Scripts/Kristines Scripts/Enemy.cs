using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float cycleLength = 2f;
    [SerializeField] float height = 2f;
    [SerializeField] float width = 5f;

    [SerializeField] bool isSwimming;
    [SerializeField] float angle = 45f;


    SwimmingTarget target;

    void Start()
    {
        target = FindObjectOfType<SwimmingTarget>();

        if (!isSwimming)
        {
            transform.DOMoveY(height, cycleLength).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }
        if (isSwimming)
        {

            // Convert the angle to radians (DOTween works in radians)
            float angleInRadians = Mathf.Deg2Rad * angle;

            // Calculate the X and Z components of the direction vector
            float xMovement = Mathf.Cos(angleInRadians);  // X component
            float zMovement = Mathf.Sin(angleInRadians);  // Z component

            // Create the direction vector based on the angle
            Vector3 movementDirection = new Vector3(xMovement, 0, zMovement);

            // Normalize the direction vector to ensure consistent movement speed
            movementDirection.Normalize();

            // Move the object at the defined angle
            transform.DOLocalMove(transform.position + movementDirection * width, cycleLength)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);  // Moves back and forth
        }

    }
}
