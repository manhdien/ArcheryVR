using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerCameraController : MonoBehaviour
{
    [Header("Sensitivity & Limits")]
    [SerializeField] private float mouseSensitivity = 1.8f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Header("References")]
    [SerializeField] private Transform playerBody;

    private float pitch = 0f;
    private Vector3 initialLocalPos;
    private float shakeTimer = 0f;
    private float shakeIntensity = 0f;

    private void Start()
    {
        initialLocalPos = transform.localPosition;

        if (playerBody == null && transform.parent != null)
        {
            playerBody = transform.parent;
        }

        LockCursor(true);
    }

    private void Update()
    {
        HandleCursorLock();
        HandleLook();
        HandleShake();
    }

    private void HandleShake()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * shakeIntensity;
            transform.localPosition = initialLocalPos + randomOffset;
        }
        else
        {
            transform.localPosition = initialLocalPos;
        }
    }

    public void ShakeCamera(float duration = 0.12f, float intensity = 0.05f)
    {
        shakeTimer = duration;
        shakeIntensity = intensity;
    }

    private void HandleCursorLock()
    {
        // Unlock cursor on Escape
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            LockCursor(false);
        }
#endif
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(false);
        }

        // Lock cursor on Mouse click if unlocked
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor(true);
        }
#endif
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor(true);
        }
    }

    private void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        float mouseX = 0f;
        float mouseY = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            mouseX = delta.x * 0.1f * mouseSensitivity;
            mouseY = delta.y * 0.1f * mouseSensitivity;
        }
        else
#endif
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    public void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
