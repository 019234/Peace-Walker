using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    public float runSpeed = 6.0f;
    public float crouchSpeed = 1.0f;
    public float proneSpeed = 1.0f;
    public float walkSpeed = 3.0f;
    public float rotationSpeed;
    public float transitionSpeed = 5.0f;
    public float pronetransitionSpeed = 10.0f;

    [Header("Player Input Actions")]
    public InputActionReference move;
    public InputActionReference prone;
    public InputActionReference crawl;
    public InputActionReference run;

    [Header("Camera and Animator")]
    public Transform cameraMainTransform;
    public Animator anim;

    private CapsuleCollider capCol;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private bool isRunning;
    private bool isCrouching;
    private bool isProning;
    private bool isForcedCrouching;
    private bool isForcedProning;
    private float targetSpeed;
    private float currentSpeed;
    private float targetCrouchSpeed;
    private float targetProneSpeed;
    private float currentCrouchSpeed;
    private float currentProneSpeed;
    private int speedHash;
    private int crouchStateHash;
    private int proneStateHash;
    private int groundedBool;
    private Vector3 colExtents;

    // New bool to track if the player is moving
    private bool isMoving;

    void OnEnable()
    {
        crawl.action.started += ToggleCrouch;
        crawl.action.canceled += ToggleCrouch;
        run.action.started += Run;
        run.action.canceled += Run;

        prone.action.started += ToggleProne;
        prone.action.canceled += ToggleProne;
    }

    void OnDisable()
    {
        crawl.action.started -= ToggleCrouch;
        crawl.action.canceled -= ToggleCrouch;
        run.action.started -= Run;
        run.action.canceled -= Run;

        prone.action.started -= ToggleProne;
        prone.action.canceled -= ToggleProne;
    }

    void Awake()
    {
        speedHash = Animator.StringToHash("Speed");
        crouchStateHash = Animator.StringToHash("CrouchState");
        proneStateHash = Animator.StringToHash("ProneState");
        groundedBool = Animator.StringToHash("Grounded");

        anim.SetBool(groundedBool, true);
        anim.SetBool("isProning", false);
        rb = GetComponent<Rigidbody>();
        capCol = GetComponent<CapsuleCollider>();
        colExtents = GetComponent<Collider>().bounds.extents;
    }

    void Update()
    {
        moveDirection = move.action.ReadValue<Vector3>();
        anim.SetBool(groundedBool, IsGrounded());

        // Set IsMoving to true if there is movement
        isMoving = moveDirection.magnitude > 0f;

        // Update the Animator with the IsMoving value
        anim.SetBool("IsMoving", isMoving);
    }

    void FixedUpdate()
    {
        Vector3 forward = cameraMainTransform.forward;
        Vector3 right = cameraMainTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDirection = forward * moveDirection.z + right * moveDirection.x;
        Vector3 movement = desiredMoveDirection.normalized;

        if (isCrouching || isForcedCrouching)
        {
            targetCrouchSpeed = crouchSpeed;
            Crouched();

            if (movement.magnitude > 0)
            {
                currentCrouchSpeed = Mathf.Lerp(currentCrouchSpeed, 2f, transitionSpeed * Time.deltaTime);
                currentProneSpeed = 0f;
            }
            else
            {
                currentCrouchSpeed = Mathf.Lerp(currentCrouchSpeed, 1f, transitionSpeed * Time.deltaTime);
            }
            anim.SetFloat(crouchStateHash, currentCrouchSpeed);

            targetSpeed = 0.0f;
            currentSpeed = targetSpeed;
        }
        else if (isProning || isForcedProning)
        {
            targetProneSpeed = proneSpeed;
            isCrouching = false;

            if (movement.magnitude > 0)
            {
                currentProneSpeed = Mathf.Lerp(currentProneSpeed, 2f, pronetransitionSpeed * Time.deltaTime);
                currentCrouchSpeed = 0f;
            }
            else
            {
                currentProneSpeed = Mathf.Lerp(currentProneSpeed, 0f, pronetransitionSpeed * Time.deltaTime);
            }
            anim.SetFloat(proneStateHash, currentProneSpeed);

            targetSpeed = 0.0f;
            currentSpeed = targetSpeed;
        }
        else
        {
            targetSpeed = movement.magnitude > 0 ? (isRunning ? runSpeed : walkSpeed) : 0.0f;
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, transitionSpeed * Time.deltaTime);

            currentCrouchSpeed = Mathf.Lerp(currentCrouchSpeed, 0.0f, transitionSpeed * Time.deltaTime);
            currentProneSpeed = Mathf.Lerp(currentProneSpeed, 0.0f, pronetransitionSpeed * Time.deltaTime);

            anim.SetFloat(crouchStateHash, currentCrouchSpeed);
            anim.SetFloat(proneStateHash, currentProneSpeed);
            UnCrouched();
        }

        anim.SetFloat(speedHash, currentSpeed);

        rb.MovePosition(rb.position + movement * (currentSpeed + currentCrouchSpeed + currentProneSpeed) * Time.fixedDeltaTime);

        if (movement != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void ToggleCrouch(InputAction.CallbackContext ctx)
    {
        if (isForcedCrouching) return;

        if (ctx.started)
        {
            // Reset prone state if currently proning
            if (isProning)
            {
                anim.SetFloat(proneStateHash, 0f);
                anim.SetBool("isProning", false);
                isProning = false;
            }

            // Toggle crouch state
            isCrouching = !isCrouching;
            anim.SetFloat(crouchStateHash, isCrouching ? 1f : 0f);
        }
    }

    private void ToggleProne(InputAction.CallbackContext ctx)
    {
        if (isForcedProning) return;

        if (ctx.started)
        {
            // Reset crouch state if currently crouching
            if (isCrouching)
            {
                anim.SetFloat(crouchStateHash, 0f);
                isCrouching = false;
            }

            // Toggle prone state
            isProning = !isProning;
            anim.SetBool("isProning", isProning);
            anim.SetFloat(proneStateHash, isProning ? 1f : 0f);
        }
    }


    private void Run(InputAction.CallbackContext ctx)
    {
        isRunning = ctx.ReadValueAsButton();
        Debug.Log("Run triggered");
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "ForceCrouch")
        {
            Crouched();
            isForcedCrouching = true;
            isCrouching = true;
        }
        if (collision.gameObject.tag == "ForceProne")
        {
            isForcedProning = true;
            isProning = true;
            anim.SetBool("isProning", true);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "ForceCrouch")
        {
            UnCrouched();
            isForcedCrouching = false;
            isCrouching = false;
        }
        if (collision.gameObject.tag == "ForceProne")
        {
            isForcedProning = false;
            isProning = false;
            anim.SetBool("isProning", false);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, colExtents.y + 0.1f);
    }

    private void Crouched()
    {
        // Adjust the radius to a smaller value for crouching
        capCol.radius = 1.0f;

        // Adjust the Y position of the center for a lower crouch position
        capCol.center = new Vector3(capCol.center.x, 0.5f, capCol.center.z); // Change 0.5f to the desired crouch center height
    }

    private void UnCrouched()
    {
        // Reset the radius to the original size for standing position
        capCol.radius = 2.0f;

        // Set the center back to a higher position for standing
        capCol.center = new Vector3(capCol.center.x, 1.0f, capCol.center.z); // Change 1.0f to the desired standing center height
    }


}

