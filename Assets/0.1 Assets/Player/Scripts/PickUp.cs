using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleObjectPicker : MonoBehaviour
{
    public InputActionReference pickUpLAction;
    public InputActionReference pickUpRAction;
    [Header("Hold Positions")]
    public Transform leftBriefHoldPosition;
    public Transform leftIsoHoldPosition;
    public Transform rightBriefHoldPosition;
    public Transform rightIsoHoldPosition;

    [Header("Rotation Speed")]
    public float rotationSpeed = 200f;

    private GameObject grabbedObjectL;
    private GameObject grabbedObjectR;
    private bool isHoldingL = false;
    private bool isHoldingR = false;

    private Animator anim;

    [Header("Detection Radii")]
    public float detectionRadius = 2f;
    public float leftDetectionOffset = -1f;
    public float rightDetectionOffset = 1f;

    private PlayerMovement playerMovement;

    void Start()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {



        if (pickUpLAction.action.WasPressedThisFrame() && !isHoldingL)
        {
            TryPickUp(ref grabbedObjectL, ref isHoldingL, "Brief", "Iso", leftBriefHoldPosition, leftIsoHoldPosition, leftDetectionOffset);
        }


        if (pickUpRAction.action.WasPressedThisFrame() && !isHoldingR)
        {
            TryPickUp(ref grabbedObjectR, ref isHoldingR, "Brief", "Iso", rightBriefHoldPosition, rightIsoHoldPosition, rightDetectionOffset);
        }


        if (pickUpLAction.action.WasReleasedThisFrame() && isHoldingL || playerMovement.isProning)
        {
            ReleaseObject(ref grabbedObjectL, ref isHoldingL);
        }


        if (pickUpRAction.action.WasReleasedThisFrame() && isHoldingR || playerMovement.isProning)
        {
            ReleaseObject(ref grabbedObjectR, ref isHoldingR);
        }


        if (isHoldingL && grabbedObjectL != null)
        {
            UpdateHeldObjectPosition(grabbedObjectL, "Brief", "Iso", leftBriefHoldPosition, leftIsoHoldPosition);
            RotateHeldObject(grabbedObjectL);
        }

        if (isHoldingR && grabbedObjectR != null)
        {
            UpdateHeldObjectPosition(grabbedObjectR, "Brief", "Iso", rightBriefHoldPosition, rightIsoHoldPosition);
            RotateHeldObject(grabbedObjectR);
        }

        anim.SetBool("PickUpR", isHoldingR);
        anim.SetBool("PickUpL", isHoldingL);


    }

    private void TryPickUp(ref GameObject grabbedObject, ref bool isHolding, string tagBrief, string tagIso, Transform briefHoldPosition, Transform isoHoldPosition, float sideOffset)
    {

        Vector3 detectionPosition = transform.position + transform.right * sideOffset;
        Collider[] colliders = Physics.OverlapSphere(detectionPosition, detectionRadius);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag(tagBrief) || collider.CompareTag(tagIso))
            {
                grabbedObject = collider.gameObject;
                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.detectCollisions = false;
                }
                isHolding = true;
                break;
            }
        }
    }

    private void ReleaseObject(ref GameObject grabbedObject, ref bool isHolding)
    {
        if (grabbedObject != null)
        {
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
                playerMovement.walkSpeed = 3.0f;
                playerMovement.crouchSpeed = 1.0f;
                playerMovement.runSpeed = 14.0f;
            }
            grabbedObject = null;
            isHolding = false;
        }
    }

    private void UpdateHeldObjectPosition(GameObject objectToMove, string tagBrief, string tagIso, Transform briefHoldPosition, Transform isoHoldPosition)
    {
        if (objectToMove.CompareTag(tagBrief))
        {
            objectToMove.transform.position = briefHoldPosition.position;
            objectToMove.transform.rotation = briefHoldPosition.rotation;
            playerMovement.walkSpeed = 2.0f;
            playerMovement.crouchSpeed = 0.7f;
            playerMovement.runSpeed = 12.0f;
        }
        else if (objectToMove.CompareTag(tagIso))
        {
            objectToMove.transform.position = isoHoldPosition.position;
            objectToMove.transform.rotation = isoHoldPosition.rotation;
            playerMovement.walkSpeed = 1.5f;
            playerMovement.crouchSpeed = 0.3f;
            playerMovement.runSpeed = 4.5f;
        }
    }

    private void RotateHeldObject(GameObject objectToRotate)
    {
        objectToRotate.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
