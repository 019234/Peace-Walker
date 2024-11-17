using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraAssigner : MonoBehaviour
{
    [Header("Cinemachine Camera Settings")]
    public GameObject targetObject;  // The GameObject to be assigned to Follow and LookAt

    void Start()
    {
        // Find the Cinemachine Virtual Camera in the scene
        CinemachineCamera cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

        if (cinemachineCamera != null)
        {
            if (targetObject != null)
            {
                // Assign the targetObject to Follow and LookAt of the Cinemachine Virtual Camera
                cinemachineCamera.Follow = targetObject.transform;
                cinemachineCamera.LookAt = targetObject.transform;

                Debug.Log("Cinemachine camera assigned to target object.");
            }
            else
            {
                Debug.LogWarning("Target object is not assigned in the inspector.");
            }
        }
        else
        {
            Debug.LogWarning("No Cinemachine Virtual Camera found in the scene.");
        }
    }
}
