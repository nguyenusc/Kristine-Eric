using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RollBall : MonoBehaviour
{
    [SerializeField] float cycleLength = 2.0f;
    void Start()
    {
        // Rotate Ball using DOTween
        // Pass in a Vector3 of how much you want to rotate by, in conjuction with FastBeyond360
        // To avoid ball snapping back to default position after full rotation, use LoopType.Incremental instead of LoopType.Restart
        transform.DORotate(new Vector3(360, 0, 0), cycleLength, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
    }
}
