using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using Mirror;
using System;
using Unity.Cinemachine;
using Unity.VisualScripting;

public class PlayerMovement : NetworkBehaviour
{
    #region Publics
    [Header("Player Settings")]
    public float runSpeed = 6.0f;
    public float crouchSpeed = 1.0f;
    public float proneSpeed = 0.5f;
    public float walkSpeed = 3.0f;
    public float rotationSpeed;
    public float transitionSpeed = 5.0f;
    public float proneTransitionSpeed = 10.0f;

    [Header("Player Input Actions")]
    public InputActionReference move;
    public InputActionReference prone;
    public InputActionReference crawl;
    public InputActionReference run;
    public InputActionReference jump;


    [Header("Camera and Animator")]
    public Animator anim;
    #endregion

    #region Privates
    private Transform cameraMainTransform;
    private CapsuleCollider capCol;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private bool isRunning;
    public bool isCrouching;
    public bool isProning;
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

    public bool isMoving;
    private CinemachineCamera followCameraPlayer;
    #endregion

    void OnEnable()
    {
        crawl.action.started += ToggleCrouch;
        crawl.action.canceled += ToggleCrouch;
        run.action.started += Run;
        run.action.canceled += Run;
        prone.action.started += ToggleProne;
        prone.action.canceled += ToggleProne;

    }

    #region OnDisable,OnStart,Awake,Start etc etc
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

        // Get the transform of the main camera
        cameraMainTransform = Camera.main?.transform;
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            followCameraPlayer = CinemachineCamera.FindAnyObjectByType<CinemachineCamera>();
            followCameraPlayer.LookAt = this.gameObject.transform;
            followCameraPlayer.Follow = this.gameObject.transform;

        }
    }

    [ClientCallback]

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    #endregion

    private void Update()
    {
        if (!isLocalPlayer) { return; }
        moveDirection = move.action.ReadValue<Vector3>();
        anim.SetBool(groundedBool, IsGrounded());

        isMoving = moveDirection.magnitude > 0f;
        anim.SetBool("IsMoving", isMoving);
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) { return; }
        if (cameraMainTransform == null) return;

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
            Prone();

            if (movement.magnitude > 0)
            {
                currentProneSpeed = Mathf.Lerp(currentProneSpeed, 2f, proneTransitionSpeed * Time.deltaTime);
                currentCrouchSpeed = 0f;
            }
            else
            {
                currentProneSpeed = Mathf.Lerp(currentProneSpeed, 0f, proneTransitionSpeed * Time.deltaTime);
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
            currentProneSpeed = Mathf.Lerp(currentProneSpeed, 0.0f, proneTransitionSpeed * Time.deltaTime);

            anim.SetFloat(crouchStateHash, currentCrouchSpeed);
            anim.SetFloat(proneStateHash, currentProneSpeed);
            UnCrouched();
        }

        anim.SetFloat(speedHash, currentSpeed);

        // Set velocity based on movement direction
        Vector3 velocity = movement * (currentSpeed + currentCrouchSpeed + currentProneSpeed);
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

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
        if (!isLocalPlayer) { return; }

        if (ctx.started)
        {
            if (isProning)
            {
                anim.SetFloat(proneStateHash, 0f);
                anim.SetBool("isProning", false);
                isProning = false;
            }

            isCrouching = !isCrouching;
            anim.SetFloat(crouchStateHash, isCrouching ? 1f : 0f);
        }
    }

    private void ToggleProne(InputAction.CallbackContext ctx)
    {
        if (isForcedProning) return;
        if (!isLocalPlayer) { return; }

        if (ctx.started)
        {
            if (isCrouching)
            {
                anim.SetFloat(crouchStateHash, 0f);
                isCrouching = false;
            }

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
        if (!isLocalPlayer) { return; }
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
        if (!isLocalPlayer) { return; }
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
        capCol.radius = 1;
        capCol.center = new Vector3(capCol.center.x, 1.27f, 0.44f);
        capCol.height = 2.5f;
        capCol.direction = 1;
    }
    private void Prone()
    {
        capCol.radius = 0.7f;
        capCol.center = new Vector3(capCol.center.x, 0.51f, 0.01f);
        capCol.height = 3.97f;
        capCol.direction = 2;
    }

    private void UnCrouched()
    {
        capCol.radius = 1;
        capCol.center = new Vector3(capCol.center.x, 1.95f, capCol.center.z);
        capCol.height = 4.20f;
        capCol.direction = 1;
    }
}
