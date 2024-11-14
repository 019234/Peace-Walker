using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public CinemachineCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    void Start()
    {
        if (virtualCamera != null)
        {

            noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void TriggerShake()
    {
        if (noise != null)
        {

            noise.AmplitudeGain = 1f;
            Invoke("StopShake", 0.5f);
        }
    }

    private void StopShake()
    {
        if (noise != null)
        {
            noise.AmplitudeGain = 0f;
        }
    }
}
