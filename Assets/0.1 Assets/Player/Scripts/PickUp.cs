using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleObjectPicker : MonoBehaviour
{
    public InputActionReference pickUpLAction;
    public InputActionReference pickUpRAction;
    public InputActionReference testAction;

    [Header("Hold Positions")]
    public Transform leftBriefHoldPosition;
    public Transform leftIsoHoldPosition;
    public Transform rightBriefHoldPosition;
    public Transform rightIsoHoldPosition;

    [Header("Test Placeholder Position")]
    public Transform testLeftBriefPlaceholder;
    public Transform testRightBriefPlaceholder;
    public Transform testIsoPlaceholder;

    [Header("Rotation Speed")]
    public float rotationSpeed = 200f;

    private GameObject grabbedObjectL;
    private GameObject grabbedObjectR;
    public bool isHoldingL = false;
    public bool isHoldingR = false;

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
        HandlePickUp(ref grabbedObjectL, ref isHoldingL, pickUpLAction, leftBriefHoldPosition, leftIsoHoldPosition, leftDetectionOffset);
        HandlePickUp(ref grabbedObjectR, ref isHoldingR, pickUpRAction, rightBriefHoldPosition, rightIsoHoldPosition, rightDetectionOffset);

        HandleRelease(ref grabbedObjectL, ref isHoldingL);
        HandleRelease(ref grabbedObjectR, ref isHoldingR);

        UpdateHeldObject(grabbedObjectL, isHoldingL, leftBriefHoldPosition, leftIsoHoldPosition);
        UpdateHeldObject(grabbedObjectR, isHoldingR, rightBriefHoldPosition, rightIsoHoldPosition);

        anim.SetBool("PickUpR", isHoldingR);
        anim.SetBool("PickUpL", isHoldingL);

        if (testAction.action.WasPressedThisFrame() && (isHoldingL || isHoldingR))
        {
            if (grabbedObjectL != null && (grabbedObjectL.CompareTag("Iso") || grabbedObjectL.CompareTag("Brief")))
            {

                StartCoroutine(OnBackL(grabbedObjectL, testLeftBriefPlaceholder, testIsoPlaceholder));
            }

            if (grabbedObjectR != null && (grabbedObjectR.CompareTag("Iso") || grabbedObjectR.CompareTag("Brief")))
            {

                StartCoroutine(OnBackR(grabbedObjectR, testRightBriefPlaceholder, testIsoPlaceholder));
            }
        }
    }

    private void HandlePickUp(ref GameObject grabbedObject, ref bool isHolding, InputActionReference pickUpAction, Transform briefHoldPosition, Transform isoHoldPosition, float sideOffset)
    {
        if (pickUpAction.action.WasPressedThisFrame() && !isHolding)
        {
            TryPickUp(ref grabbedObject, ref isHolding, briefHoldPosition, isoHoldPosition, sideOffset);
        }
    }

    private void HandleRelease(ref GameObject grabbedObject, ref bool isHolding)
    {
        if (grabbedObject != null && (pickUpLAction.action.WasReleasedThisFrame() || pickUpRAction.action.WasReleasedThisFrame() || playerMovement.isProning))
        {
            ReleaseObject(ref grabbedObject, ref isHolding);
        }
    }

    private void TryPickUp(ref GameObject grabbedObject, ref bool isHolding, Transform briefHoldPosition, Transform isoHoldPosition, float sideOffset)
    {
        Vector3 detectionPosition = transform.position + transform.right * sideOffset;
        Collider[] colliders = Physics.OverlapSphere(detectionPosition, detectionRadius);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Brief") || collider.CompareTag("Iso"))
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
            }

            grabbedObject = null;
            isHolding = false;
        }
    }

    private void UpdateHeldObject(GameObject grabbedObject, bool isHolding, Transform briefHoldPosition, Transform isoHoldPosition)
    {
        if (grabbedObject == null) return;

        if (grabbedObject.CompareTag("Brief"))
        {
            grabbedObject.transform.position = briefHoldPosition.position;
            grabbedObject.transform.rotation = briefHoldPosition.rotation;
        }
        else if (grabbedObject.CompareTag("Iso"))
        {
            grabbedObject.transform.position = isoHoldPosition.position;
            grabbedObject.transform.rotation = isoHoldPosition.rotation;
        }

        if (isHolding)
        {
            RotateHeldObject(grabbedObject);
        }
    }

    private void RotateHeldObject(GameObject objectToRotate)
    {
        objectToRotate.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    // Coroutine to move the object after 2 seconds
    private IEnumerator OnBackL(GameObject grabbedObjectL, Transform briefPlaceholder, Transform isoPlaceholder)
    {
        anim.SetBool("OnBackL", true);

        yield return new WaitForSeconds(0.13f);  // Wait for 3 seconds for the animation to finish

        anim.SetBool("OnBackL", false);  // Stop the animation

        if (grabbedObjectL.CompareTag("Iso"))
        {
            MoveObjectToIsoPlaceholder(grabbedObjectL);
        }
        else if (grabbedObjectL.CompareTag("Brief"))
        {
            MoveObjectToBriefPlaceholder(grabbedObjectL, briefPlaceholder);
        }
    }

    private IEnumerator OnBackR(GameObject grabbedObjectR, Transform briefPlaceholder, Transform isoPlaceholder)
    {

        anim.SetBool("OnBackR", true);

        yield return new WaitForSeconds(0.13f);  // Wait for 3 seconds for the animation to finish

        anim.SetBool("OnBackR", false);  // Stop the animation

        if (grabbedObjectR.CompareTag("Iso"))
        {
            MoveObjectToIsoPlaceholder(grabbedObjectR);
        }
        else if (grabbedObjectR.CompareTag("Brief"))
        {
            MoveObjectToBriefPlaceholder(grabbedObjectR, briefPlaceholder);
        }
    }

    private void MoveObjectToIsoPlaceholder(GameObject grabbedObject)
    {
        if (grabbedObject != null && testIsoPlaceholder.childCount == 0)
        {
            grabbedObject.transform.position = testIsoPlaceholder.position;
            grabbedObject.transform.rotation = testIsoPlaceholder.rotation;
            grabbedObject.transform.SetParent(testIsoPlaceholder);

            grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
            grabbedObject.GetComponent<Rigidbody>().detectCollisions = false;

            playerMovement.walkSpeed -= 0.6f;
            playerMovement.crouchSpeed -= 0.6f;
            playerMovement.runSpeed -= 0.6f;

            if (grabbedObject == grabbedObjectL)
            {
                grabbedObjectL = null;
                isHoldingL = false;
            }
            else if (grabbedObject == grabbedObjectR)
            {
                grabbedObjectR = null;
                isHoldingR = false;
            }
        }
    }

    private void MoveObjectToBriefPlaceholder(GameObject grabbedObject, Transform briefPlaceholder)
    {
        if (grabbedObject != null && briefPlaceholder.childCount == 0)
        {
            grabbedObject.transform.position = briefPlaceholder.position;
            grabbedObject.transform.rotation = briefPlaceholder.rotation;
            grabbedObject.transform.SetParent(briefPlaceholder);

            grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
            grabbedObject.GetComponent<Rigidbody>().detectCollisions = false;

            playerMovement.walkSpeed -= 0.3f;
            playerMovement.crouchSpeed -= 0.3f;
            playerMovement.runSpeed -= 0.3f;

            if (grabbedObject == grabbedObjectL)
            {
                grabbedObjectL = null;
                isHoldingL = false;
            }
            else if (grabbedObject == grabbedObjectR)
            {
                grabbedObjectR = null;
                isHoldingR = false;
            }
        }
    }
}
