using UnityEngine;
using UnityEngine.InputSystem;

public class ShakeInput : MonoBehaviour
{
    public CameraShake cameraShake; 
    public InputActionReference testButton;



    void Update()
    {
        if (testButton.action.IsPressed())
        {
            cameraShake.TriggerShake();
        }
    }
}
