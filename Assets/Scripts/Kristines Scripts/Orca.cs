using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Orca : MonoBehaviour
{
    [SerializeField] float cycleLength;

    void Start()
    {
        transform.DOLocalRotate(new Vector3(0, 360, 0), cycleLength, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetRelative()
            .SetEase(Ease.Linear);
    }
}
