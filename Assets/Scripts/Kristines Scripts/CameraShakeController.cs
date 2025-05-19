using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class CameraShakeController : MonoBehaviour
{
    CinemachineBasicMultiChannelPerlin perlinNoise;
    CinemachineVirtualCamera currentVirtualCamera;

    public void ShakeCamera(float intensity, float shakeTime)
    {
        UpdateCamera();

        perlinNoise.m_AmplitudeGain = intensity;
        StartCoroutine(WaitTime(shakeTime));
    }

    void UpdateCamera()
    {
        // Get the ICinemachineCamera from the cinemachine brain
        var activeCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if (activeCamera is CinemachineVirtualCamera)
        {
            // Find and cast icinemachine to vcam and assign vcam
            CinemachineVirtualCamera vcam = (CinemachineVirtualCamera)activeCamera;
            currentVirtualCamera = vcam;
            perlinNoise = currentVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    IEnumerator WaitTime(float shakeTime)
    {
        yield return new WaitForSeconds(shakeTime);
        ResetIntensity();
    }

    void ResetIntensity()
    {
        perlinNoise.m_AmplitudeGain = 0f;
    }
}
