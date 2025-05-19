using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


// This script is applied to the Player GO hence the need to use DOLocalMoveY as opposed to DoMoveY
// We need to manipulate the local position of the Player 

public class PlayerBob : MonoBehaviour
{
    [SerializeField] float bobEffect;
    [SerializeField] float cycleLength;

    PlayerMovement player;
    Tween bobTween;

    // Change to coroutine and bob only when touching collider?

    void Start()
    {
        player = GetComponent<PlayerMovement>();

        // Add an offset (bobEffect) to the Player's local position 
        bobTween = transform.DOLocalMoveY(transform.localPosition.y + bobEffect, cycleLength).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    //void Update()
    //{
    //    if (player.GetIsGrounded())
    //    {
    //        StartCoroutine(PlayBob());
    //    }

    //}

    //IEnumerator PlayBob()
    //{


    //}
}
