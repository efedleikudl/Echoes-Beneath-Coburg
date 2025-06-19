// PlayerController.cs – First-Person-Controller mit Sprint, Sprung, Licht und Interaktion (URP-kompatibel)
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Bewegung")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float sprintDuration = 5f;
    public float sprintCooldown = 3f;
    public float jumpForce = 8f;
    public float gravity = -20f;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public Transform cameraHolder;

    [Header("Interaktion")]
    public float interactRange = 3f;
    public LayerMask interactLayer;
    public KeyCode interactKey = KeyCode.E;

    [Header("Licht")]
    public GameObject handheldLight;
    public KeyCode toggleLightKey = KeyCode.F;

    [Header("UI")]
    public Image sprintBar;

    private CharacterController controller;
    private float xRotation = 0f;
    private float yVelocity = 0f;

    private float sprintTimer;
    private float cooldownTimer;
    private bool isSprinting;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        sprintTimer = sprintDuration;
        cooldownTimer = 0f;
    }

    void Update()
    {
        Look();
        HandleSprint();
        Move();
        HandleLight();
        HandleInteraction();
        UpdateSprintUI();
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * 2f;
        float mouseY = Input.GetAxis("Mouse Y") * 2f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleSprint()
    {
        if (Input.GetKey(sprintKey) && sprintTimer > 0f && cooldownTimer <= 0f)
        {
            isSprinting = true;
            sprintTimer -= Time.deltaTime;
        }
        else
        {
            isSprinting = false;

            if (sprintTimer < sprintDuration)
                cooldownTimer += Time.deltaTime;

            if (cooldownTimer >= sprintCooldown)
            {
                sprintTimer = sprintDuration;
                cooldownTimer = 0f;
            }
        }
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        Vector3 move = (transform.right * x + transform.forward * z) * currentSpeed;

        if (controller.isGrounded)
        {
            yVelocity = -1f;

            if (Input.GetButtonDown("Jump"))
            {
                yVelocity = jumpForce;
            }
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
        }

        move.y = yVelocity;
        controller.Move(move * Time.deltaTime);
    }

    void HandleLight()
    {
        if (Input.GetKeyDown(toggleLightKey))
        {
            if (handheldLight != null)
                handheldLight.SetActive(!handheldLight.activeSelf);
        }
    }


void HandleInteraction()
{
    if (Input.GetKeyDown(interactKey))  // Standard ist KeyCode.E
    {
        Ray ray = new Ray(cameraHolder.position, cameraHolder.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            if (PlayerInventory.IsCarrying)
            {
                PlaceTarget target = hit.collider.GetComponent<PlaceTarget>();
                if (target != null)
                {
                    target.TryPlace();
                    return;
                }
            }
            else
            {
                CarryItem item = hit.collider.GetComponent<CarryItem>();
                if (item != null)
                {
                    item.PickUp();
                    return;
                }
            }
        }
    }
}



    void UpdateSprintUI()
    {
        if (sprintBar != null)
        {
            sprintBar.fillAmount = sprintTimer / sprintDuration;
        }
    }
}

// Interface für interaktive Objekte
public interface IInteractable
{
    void Interact();
}


// Beispiel: Tür, die Schlüssel benötigt
public class LockableDoor : MonoBehaviour
{
    public string requiredKeyID;
    public Transform doorPivot;
    public float openAngle = 90f;
    public float openSpeed = 2f;

    private bool isOpen = false;


    private System.Collections.IEnumerator OpenDoor()
    {
        Quaternion startRot = doorPivot.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, openAngle, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            doorPivot.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
    }
}



// Beispiel: Kerze ein-/ausschalten
public class CandleToggle : MonoBehaviour, IInteractable
{
    public GameObject flame;

    public void Interact()
    {
        if (flame != null)
            flame.SetActive(!flame.activeSelf);
    }
}

    

