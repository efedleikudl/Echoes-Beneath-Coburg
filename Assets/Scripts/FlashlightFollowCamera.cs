using UnityEngine;

/// <summary>
/// Keeps a Light (flashlight) locked to the centre of a camera,
/// so the beam always points exactly where the player is looking.
/// </summary>
[RequireComponent(typeof(Light))]
public class FlashlightFollowCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera the flashlight should follow (defaults to Camera.main).")]
    public Camera targetCamera;

    [Header("Positional Offsets (in camera space)")]
    [Tooltip("Move the light forward from the camera, e.g. to avoid clipping.")]
    public float forwardOffset = 0.25f;
    [Tooltip("Lift the light slightly if you want it above the lens.")]
    public float verticalOffset = 0f;

    [Header("Rotation Smoothing")]
    [Tooltip("0 = snap instantly, higher = smoother but laggier.")]
    [Range(0f, 0.2f)]
    public float rotationSmoothTime = 0.05f;

    Quaternion _targetRotation;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;      // Fall back to the main camera
    }

    void LateUpdate()                      // After all other transforms have updated
    {
        if (targetCamera == null) return;

        // ─── Position ──────────────────────────────────────────────────────────
        // Build an offset in camera space, then convert it to world space.
        Vector3 camPos = targetCamera.transform.position;
        Vector3 camFwd = targetCamera.transform.forward;
        Vector3 camUp = targetCamera.transform.up;

        transform.position =
            camPos + camFwd * forwardOffset + camUp * verticalOffset;

        // ─── Rotation ─────────────────────────────────────────────────────────
        _targetRotation = targetCamera.transform.rotation;

        if (rotationSmoothTime <= 0f)
        {
            transform.rotation = _targetRotation;        // Snap
        }
        else
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                _targetRotation,
                Time.deltaTime / rotationSmoothTime);    // Smooth
        }
    }
}
