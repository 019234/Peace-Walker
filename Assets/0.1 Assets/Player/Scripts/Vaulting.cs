using Mirror.Examples.PickupsDropsChilds;
using UnityEngine;

public class Vaulting : MonoBehaviour
{
    [Header("Vault Settings")]

    public float vaultDuration = 0.5f;
    public float vaultDistance = 2f;
    public string vaultableTag = "Vault";
    public MonoBehaviour playerController;
    public Collider playerCollider;
    public Collider detectionCollider;

    private bool isVaulting = false;
    private Vector3 vaultStartPosition;
    private Vector3 vaultEndPosition;
    private float vaultTimer = 0f;
    public int layerIndexL = 1;
    public int layerIndexR = 2;


    private Animator anim;

    private PickUpScript simpleObjectPicker;

    private void Start()
    {
        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider>();
        }

        anim = GetComponent<Animator>();

        simpleObjectPicker = GetComponent<PickUpScript>();

    }

    private void Update()
    {
        if (isVaulting)
        {
            VaultMovement();
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(vaultableTag) && !isVaulting)
        {
            Debug.Log("Player entered a vaultable area. Ready to vault.");
            TriggerVault();
        }
    }

    public void TriggerVault()
    {
        if (isVaulting) return;

        isVaulting = true;
        vaultTimer = 0f;

        if (playerController != null)
        {
            playerController.enabled = false;
        }


        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }


        vaultStartPosition = transform.position;
        vaultEndPosition = vaultStartPosition + transform.forward * vaultDistance;
    }

    private void VaultMovement()
    {
        anim.SetBool("Vaulting", true);

        if (simpleObjectPicker.isHoldingR)
        {
            anim.SetLayerWeight(layerIndexL, 0f);
        }

        if (simpleObjectPicker.isHoldingL)
        {

            anim.SetLayerWeight(layerIndexR, 0f);
        }

        vaultTimer += Time.deltaTime / vaultDuration;


        transform.position = Vector3.Lerp(vaultStartPosition, vaultEndPosition, vaultTimer);

        if (vaultTimer >= 1f)
        {

            EndVault();
        }
    }

    private void EndVault()
    {
        isVaulting = false;

        anim.SetBool("Vaulting", false);

        anim.SetLayerWeight(layerIndexR, 1f);
        anim.SetLayerWeight(layerIndexL, 1f);

        if (playerController != null)
        {
            playerController.enabled = true;
        }


        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        Debug.Log("Vaulting complete.");
    }

    public void OnDetectionColliderEnter(Collider other)
    {

        if (other.CompareTag(vaultableTag) && !isVaulting)
        {
            Debug.Log("Detection Collider hit a vaultable object. Vaulting initiated.");
            TriggerVault();
        }
    }
}
