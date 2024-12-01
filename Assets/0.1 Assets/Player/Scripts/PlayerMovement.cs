using UnityEngine;
using UnityEngine.InputSystem;
using Alteruna;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    public float runSpeed = 6.0f;
    public float crouchSpeed = 1.0f;
    public float proneSpeed = 0.5f;
    public float walkSpeed = 3.0f;
    public float rotationSpeed = 10f;

    [Header("Player Input Actions")]
    public InputActionReference move;
    public InputActionReference prone;
    public InputActionReference crawl;
    public InputActionReference run;

    [Header("Camera and Animator")]
    public Animator anim;
    public Transform followCamera;

    private Rigidbody rb;
    private CapsuleCollider capCol;
    private Vector3 moveDirection;

    private bool isRunning;
    public bool isCrouching;
    public bool isProning;
    private float currentSpeed;

    private int speedHash;
    private int crouchStateHash;
    private int proneStateHash;
    private int groundedBool;

    private Alteruna.Avatar avatar;
    private Transform cameraMainTransform;

    void Awake()
    {
        speedHash = Animator.StringToHash("Speed");
        crouchStateHash = Animator.StringToHash("CrouchState");
        proneStateHash = Animator.StringToHash("ProneState");
        groundedBool = Animator.StringToHash("Grounded");

        rb = GetComponent<Rigidbody>();
        capCol = GetComponent<CapsuleCollider>();
    }

    void Start()
    {
        avatar = GetComponent<Alteruna.Avatar>();

        if (avatar.IsMe)
        {
            // Assign the camera to follow the player
            if (followCamera != null)
            {
                followCamera.parent = null;
                followCamera.position = transform.position - transform.forward * 5 + Vector3.up * 2;
                followCamera.LookAt(transform);
            }
        }

        {
            // Get the main camera's transform and make it follow the player
            cameraMainTransform = Camera.main?.transform;

            if (cameraMainTransform != null)
            {
                // Optional: Assign follow and look-at logic for the camera
                CinemachineCamera virtualCamera = GetComponent<CinemachineCamera>();
                if (virtualCamera != null)
                {
                    virtualCamera.LookAt = transform;
                    virtualCamera.Follow = transform;
                }
            }
        }
    }

    void OnEnable()
    {
        crawl.action.started += ToggleCrouch;
        crawl.action.canceled += ToggleCrouch;
        run.action.started += ToggleRun;
        run.action.canceled += ToggleRun;
        prone.action.started += ToggleProne;
        prone.action.canceled += ToggleProne;
    }

    void OnDisable()
    {
        crawl.action.started -= ToggleCrouch;
        crawl.action.canceled -= ToggleCrouch;
        run.action.started -= ToggleRun;
        run.action.canceled -= ToggleRun;
        prone.action.started -= ToggleProne;
        prone.action.canceled -= ToggleProne;
    }

    void Update()
    {
        if (!avatar.IsMe) return;

        moveDirection = move.action.ReadValue<Vector2>();

        // Update grounded animation
        anim.SetBool(groundedBool, IsGrounded());
    }

    void FixedUpdate()
    {
        if (!avatar.IsMe) return;

        Vector3 forward = followCamera.forward;
        Vector3 right = followCamera.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDirection = forward * moveDirection.y + right * moveDirection.x;
        Vector3 movement = desiredMoveDirection.normalized;

        DetermineMovementSpeed();
        ApplyMovement(movement);
        RotateTowardsMovement(movement);
    }

    private void DetermineMovementSpeed()
    {
        if (isProning)
        {
            currentSpeed = proneSpeed;
            anim.SetFloat(proneStateHash, 1f);
            anim.SetFloat(crouchStateHash, 0f);
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
            anim.SetFloat(crouchStateHash, 1f);
            anim.SetFloat(proneStateHash, 0f);
        }
        else
        {
            currentSpeed = isRunning ? runSpeed : walkSpeed;
            anim.SetFloat(proneStateHash, 0f);
            anim.SetFloat(crouchStateHash, 0f);
        }

        anim.SetFloat(speedHash, currentSpeed);
    }

    private void ApplyMovement(Vector3 movement)
    {
        Vector3 velocity = movement * currentSpeed;
        velocity.y = rb.linearVelocity.y; // Preserve vertical velocity
        rb.linearVelocity = velocity;
    }

    private void RotateTowardsMovement(Vector3 movement)
    {
        if (movement == Vector3.zero) return;

        float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void ToggleCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            isCrouching = !isCrouching;
            if (isCrouching)
                isProning = false; // Cannot crouch and prone at the same time
        }
    }

    private void ToggleProne(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            isProning = !isProning;
            if (isProning)
                isCrouching = false; // Cannot crouch and prone at the same time
        }
    }

    private void ToggleRun(InputAction.CallbackContext ctx)
    {
        isRunning = ctx.ReadValueAsButton();
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, capCol.bounds.extents.y + 0.1f);
    }
}
