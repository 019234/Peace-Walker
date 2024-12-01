using UnityEngine;
using UnityEngine.InputSystem; // Import the New Input System namespace

public class PlayerMovementAi : MonoBehaviour
{
    [Header("Movement")]
    public CharacterController controller;
    public float playerSpeed = 7.0f;
    public float jumpForce = 10.0f;
    public float stepDown = 30.0f;
    [Tooltip("Fall velocity when the player is on a slope")]
    public float fallSlopeVelocity = 8f;
    [Tooltip("Force in the Y-axis when the player is on a slope")]
    public float fallSlopeForce = 10.0f;

    [HideInInspector]
    public bool safeZone;
    [HideInInspector]
    public bool IsRunning;
    [HideInInspector]
    public bool IsWalking;

    Vector3 m_PlayerMovement;
    Vector3 moveDirection; // Use the input directly as Vector3
    float m_Speed;
    float m_Gravity = 20.0f;
    float m_FallVelocity;
    bool m_IsJump;

    bool m_OnSlope = false;
    Vector3 m_HitNormal;

    Animator m_Animator;

    [Header("Input References")]
    public InputActionReference move; // Reference for Move action
    public InputActionReference sprint; // Reference for Sprint action
    public InputActionReference jump; // Reference for Jump action

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Speed = playerSpeed;

        // Enable input actions
        move.action.Enable();
        sprint.action.Enable();
        jump.action.Enable();
    }

    void Update()
    {
        // Read movement input as Vector3
        moveDirection = move.action.ReadValue<Vector3>();
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        bool isMove = moveDirection.magnitude > 0.1f;

        m_PlayerMovement = moveDirection * m_Speed;

        // Rotate the player to face the movement direction
        if (isMove)
        {
            controller.transform.forward = new Vector3(moveDirection.x, 0f, moveDirection.z);
        }

        Gravity();
        Jump();

        if (controller.isGrounded && !m_IsJump)
        {
            m_PlayerMovement += Vector3.down * stepDown;
        }

        controller.Move(m_PlayerMovement * Time.deltaTime);
        m_IsJump = !controller.isGrounded;

        if (isMove)
        {
            Action();
        }
        else
        {
            m_Animator.SetFloat("Speed", 0);
        }
    }

    private void OnAnimatorMove() { }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        m_HitNormal = hit.normal;
    }

    void Action()
    {
        if (sprint.action.ReadValue<float>() > 0.5f) // Check if Sprint is held
        {
            m_Speed = playerSpeed * 1.5f;
            IsWalking = false;
            IsRunning = true;
            m_Animator.SetFloat("Speed", controller.velocity.magnitude);
        }
        else
        {
            m_Speed = playerSpeed;
            IsRunning = false;
            IsWalking = true;
            m_Animator.SetFloat("Speed", controller.velocity.magnitude);
        }
    }

    void Gravity()
    {
        if (controller.isGrounded)
        {
            m_FallVelocity = -m_Gravity * Time.deltaTime;
            m_PlayerMovement.y = m_FallVelocity;
        }
        else
        {
            m_FallVelocity -= m_Gravity * Time.deltaTime;
            m_PlayerMovement.y = m_FallVelocity;
            m_Animator.SetFloat("AirSpeed", controller.velocity.y);

            if (controller.velocity.y <= -25 || controller.velocity.y >= 20)
            {
                m_Animator.SetTrigger("Jump");
            }
        }
        m_Animator.SetBool("IsGrounded", controller.isGrounded);
        FallSlope();
    }

    void Jump()
    {
        if (controller.isGrounded && jump.action.triggered) // Check if Jump was triggered
        {
            m_IsJump = true;
            m_FallVelocity = jumpForce;
            m_PlayerMovement.y = m_FallVelocity;
            m_Animator.SetTrigger("Jump");
        }
    }

    void FallSlope()
    {
        m_OnSlope = Vector3.Angle(m_HitNormal, Vector3.up) > controller.slopeLimit && Vector3.Angle(m_HitNormal, Vector3.up) < 89;

        if (m_OnSlope)
        {
            m_PlayerMovement.x += ((1f - m_HitNormal.y) * m_HitNormal.x) * fallSlopeVelocity;
            m_PlayerMovement.z += ((1f - m_HitNormal.y) * m_HitNormal.z) * fallSlopeVelocity;
            m_PlayerMovement.y -= fallSlopeForce;
        }
    }
}
