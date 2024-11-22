using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectGrabber : MonoBehaviour
{
    [Header("Settings")]
    public Transform[] isotopePlaceholders;   // Array of placeholder positions for Isotopes (large objects)
    public Transform[] briefcasePlaceholders; // Array of placeholder positions for Briefcases (medium objects)

    public float throwForce = 2.0f;
    public GameObject grabDetector;

    [Header("Input Actions")]
    public InputActionReference grabAction;     // For grabbing objects
    public InputActionReference releaseAction;  // For releasing objects

    private GameObject[] isotopes;    // Array for storing Isotopes (large objects)
    private GameObject[] briefcases;  // Array for storing Briefcases (medium objects)
    private GameObject[] files;       // Array for storing Files (small objects)

    private GameObject grabbedObject;
    private GameObject isotopeGameObject, briefcaseGameObject, fileGameObject;
    private int isotopeIndex = 0, briefcaseIndex = 0;
    private bool isThrowing = false;

    private PlayerMovement playerMovement;

    void OnEnable()
    {
        grabAction.action.started += OnGrab;
        releaseAction.action.started += OnRelease;
    }

    void OnDisable()
    {
        grabAction.action.started -= OnGrab;
        releaseAction.action.started -= OnRelease;
    }

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        // Initialize arrays based on number of placeholders
        isotopes = new GameObject[isotopePlaceholders.Length];
        briefcases = new GameObject[briefcasePlaceholders.Length];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Isotope") && isotopeIndex < isotopePlaceholders.Length)
        {
            grabbedObject = other.gameObject;
        }
        else if (other.CompareTag("Briefcase") && briefcaseIndex < briefcasePlaceholders.Length)
        {
            grabbedObject = other.gameObject;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == grabbedObject)
        {
            grabbedObject = null;
        }
    }

    private void OnGrab(InputAction.CallbackContext context)
    {
        if (grabbedObject != null)
        {
            if (grabbedObject.CompareTag("Isotope") && isotopeIndex < isotopePlaceholders.Length)
            {
                GrabObject(isotopes, isotopePlaceholders, ref isotopeIndex);
            }
            else if (grabbedObject.CompareTag("Briefcase") && briefcaseIndex < briefcasePlaceholders.Length)
            {
                GrabObject(briefcases, briefcasePlaceholders, ref briefcaseIndex);
            }
        }
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        isThrowing = true;
        ReleaseObjects(isotopes, isotopePlaceholders, ref isotopeIndex);    // Release Isotopes
        ReleaseObjects(briefcases, briefcasePlaceholders, ref briefcaseIndex); // Release Briefcases      // Release Files
    }

    private void GrabObject(GameObject[] objectArray, Transform[] placeholderArray, ref int index)
    {
        if (index < placeholderArray.Length)
        {
            objectArray[index] = grabbedObject;
            grabbedObject.transform.position = placeholderArray[index].position;
            grabbedObject.transform.parent = placeholderArray[index];

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Disable the Rigidbody so the object does not react to physics while grabbed
                rb.isKinematic = true;
                rb.detectCollisions = false;  // Optionally disable collision detection
            }

            grabbedObject = null;  // Reset grabbed object after storing
            index++;
        }
    }

    private void ReleaseObjects(GameObject[] objectArray, Transform[] placeholderArray, ref int index)
    {
        if (index > 0)
        {
            for (int i = 0; i < index; i++)
            {
                if (objectArray[i] != null)
                {
                    Rigidbody rb = objectArray[i].GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // Re-enable the Rigidbody so the object responds to physics again
                        objectArray[i].transform.parent = null;  // Unparent the object
                        rb.isKinematic = false;  // Enable physics interactions again
                        rb.detectCollisions = true;  // Enable collision detection again

                        rb.AddForce(placeholderArray[i].forward * throwForce, ForceMode.Impulse);  // Apply force to throw the object
                    }

                    objectArray[i] = null;  // Clear the object reference after release
                }
            }

            index = 0;  // Reset index after releasing all objects
        }
    }



    private void FixedUpdate()
    {

        if (isThrowing)
        {
            // Call the release function when the throw is triggered
            ReleaseObjects(isotopes, isotopePlaceholders, ref isotopeIndex);
            ReleaseObjects(briefcases, briefcasePlaceholders, ref briefcaseIndex);

            // Reset the throwing flag
            isThrowing = false;
        }

        if (grabDetector != null)
        {
            Collider[] hits = Physics.OverlapBox(grabDetector.transform.position, grabDetector.transform.localScale / 2, grabDetector.transform.rotation);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Isotope"))
                {
                    isotopeGameObject = hit.gameObject;
                    return;
                }

                if (hit.CompareTag("Briefcase"))
                {
                    briefcaseGameObject = hit.gameObject;
                    return;
                }

                if (hit.CompareTag("File"))
                {
                    fileGameObject = hit.gameObject;
                    return;
                }
            }

            isotopeGameObject = null;
            briefcaseGameObject = null;
            fileGameObject = null;
        }
    }
}

