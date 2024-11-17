using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public CinemachineCamera virtualCamera; // Changed to CinemachineVirtualCamera
    private CinemachineBasicMultiChannelPerlin noise;

    void Start()
    {
        virtualCamera = CinemachineCamera.FindAnyObjectByType<CinemachineCamera>();

        if (virtualCamera != null)
        {

            noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void TriggerShake()
    {
        if (noise != null)
        {
            // Set the amplitude to trigger the shake
            noise.AmplitudeGain = 1.53f; // Change the shake intensity
            //Invoke("StopShake", 0.5f); // Stop the shake after 0.5 seconds
        }
    }

    public void StopShake()
    {
        if (noise != null)
        {
            // Reset the amplitude to 0 to stop the shake
            noise.AmplitudeGain = 0f;
        }
    }
}
