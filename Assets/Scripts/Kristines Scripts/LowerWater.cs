using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowerWater : MonoBehaviour
{
    [SerializeField] float targetY = -45.7f;
    [SerializeField] float duration = 3f;

    Coroutine currentMove;

    void Lower()
    {
        if (currentMove != null)
            StopCoroutine(currentMove);

        currentMove = StartCoroutine(LerpPositionY(targetY, duration));
    }

    private IEnumerator LerpPositionY(float targetY, float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
        currentMove = null;
    }

    void OnEnable()
    {
        EndSceneManager.OnLowerWater += Lower;
    }

    void OnDisable()
    {
        EndSceneManager.OnLowerWater -= Lower;
    }
}



