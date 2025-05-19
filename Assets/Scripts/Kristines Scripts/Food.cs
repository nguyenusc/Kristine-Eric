using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] float cycleLength = 5;
    void Start()
    {
        transform.DORotate(new Vector3(0, 360, 0), cycleLength * 0.5f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetRelative()
            .SetEase(Ease.Linear);
    }

}
