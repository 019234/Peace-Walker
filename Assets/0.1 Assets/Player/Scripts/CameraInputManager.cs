using UnityEngine;
using UnityEngine.InputSystem;

public class ShakeInput : MonoBehaviour
{
    private CameraShake cameraShake;
    private PlayerMovement playerMovement;

    private float speedHash;
    public InputActionReference runButton;
    public InputActionReference movebutton;
    private bool isMoving;


    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        cameraShake = GetComponent<CameraShake>();
        speedHash = Animator.StringToHash("Speed");
    }
    void Update()
    {

        Vector3 moveInput = movebutton.action.ReadValue<Vector3>();

        isMoving = moveInput.magnitude > 0.1f;
        //Debug.Log(isMoving);

        if (runButton.action.IsPressed() && isMoving)
        {
            cameraShake.TriggerShake();
        }

        else
        {
            cameraShake.StopShake();
        }
    }
}
