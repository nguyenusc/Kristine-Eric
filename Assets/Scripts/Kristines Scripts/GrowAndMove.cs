using UnityEngine;
using System.Collections;

public class GrowAndMove : MonoBehaviour
{
    [SerializeField] float duration = 1f;

    public void StartGrow(Vector3 startPosition, Vector3 endPosition, Vector3 targetScale, float arcHeight = 2f)
    {
        transform.position = startPosition;
        StartCoroutine(GrowAndArcMove(startPosition, endPosition, transform.localScale, targetScale, arcHeight, duration));
    }

    IEnumerator GrowAndArcMove(Vector3 startPos, Vector3 endPos, Vector3 startScale, Vector3 endScale, float height, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Arc position
            Vector3 linearPos = Vector3.Lerp(startPos, endPos, t);

            // Parabola equation multiplied by scalar and interpolated by t
            float arc = 4 * height * t * (1 - t);
            linearPos.y += arc;

            // Apply transformations
            transform.position = linearPos;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        transform.localScale = endScale;
    }
}
