// PlayerController.cs – First‑Person‑Controller mit Sprint, Sprung, Licht, Interaktion und einfacher Schritt-/Lauf‑Audioausgabe (URP‑kompatibel)
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Inspector Fields
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

    [Header("Audio (Loopende Quellen)")]
    [Tooltip("Schrittgeräusch – wird beim normalen Gehen abgespielt")]
    public AudioSource footstepAudioSource;

    [Tooltip("Laufgeräusch – wird beim Sprinten abgespielt")]
    public AudioSource runningAudioSource;
    #endregion

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

        // Sicherstellen, dass beide Quellen nicht spielen, wenn das Spiel startet
        footstepAudioSource?.Stop();
        runningAudioSource?.Stop();
    }

    void Update()
    {
        Look();
        HandleSprint();
        Move();
        HandleAudio();   // <‑‑ NEU
        HandleLight();
        HandleInteraction();
        UpdateSprintUI();
    }

    #region Kamera & Bewegung
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
        if (Input.GetKey(sprintKey) && sprintTimer > 0f && cooldownTimer <= 0f && controller.velocity.sqrMagnitude > 0.1f)
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
    #endregion

    #region Einfaches Footstep/Runningsound‑Handling
    /*
     * Diese Methode schaltet die beiden loopenden AudioQuellen abhängig vom Bewegungszustand ein bzw. aus.
     * Voraussetzungen im Inspector:
     *  – Beide AudioSource‑Komponenten haben "Loop" aktiviert
     *  – Es sind passende Audioclips zugewiesen (z.B. Schritte, Sprinten)
     *  – Lautstärke/Pitch nach Bedarf einstellen
     */
    void HandleAudio()
    {
        if (controller == null) return;

        // Bewegungs‑ und Zustandsprüfungen
        bool isGrounded = controller.isGrounded;
        // Nur horizontale Geschwindigkeit betrachten, damit Sprung‑Y‑Geschwindigkeit ignoriert wird
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;
        bool isMoving = horizontalVelocity.magnitude > 0.1f;

        // Wenn der Spieler läuft oder geht
        if (isGrounded && isMoving)
        {
            if (isSprinting)
            {
                // Sprintgeräusch aktivieren
                runningAudioSource.enabled = true;
                // Gehgeräusch deaktivieren
                footstepAudioSource.enabled = false;
            }
            else
            {
                // Gehgeräusch aktivieren
                footstepAudioSource.enabled = true;
                // Sprintgeräusch deaktivieren
                runningAudioSource.enabled = false;
            }
        }
        else
        {
            // Keine Bodenberührung oder Stillstand – beides ausschalten
            footstepAudioSource.enabled = false;
            runningAudioSource.enabled = false;
        }
    }
    #endregion

    #region Licht & Interaktion
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
        if (Input.GetKeyDown(interactKey))
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
    #endregion

    #region UI
    void UpdateSprintUI()
    {
        if (sprintBar != null)
        {
            sprintBar.fillAmount = sprintTimer / sprintDuration;
        }
    }
    #endregion
}
