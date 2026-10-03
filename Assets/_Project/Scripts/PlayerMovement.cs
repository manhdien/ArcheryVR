using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 7.5f;

    [Header("Physics")]
    [SerializeField] private float gravity = -18f;
    [SerializeField] private float groundedGravity = -2f;

    [Header("Khu vực bắn (Shooting Area Boundary)")]
    [Tooltip("Bật giới hạn khu vực bắn — Player không thể tiến về phía target area")]
    public bool enableShootingAreaBoundary = true;

    [Tooltip("Giới hạn Z tối đa — Player không thể vượt qua ranh giới này (hướng về bia)")]
    public float maxZ = 22f;

    [Tooltip("Giới hạn Z tối thiểu — phía sau Player")]
    public float minZ = 10f;

    [Tooltip("Giới hạn X tối thiểu")]
    public float minX = 8f;

    [Tooltip("Giới hạn X tối đa")]
    public float maxX = 40f;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;
        bool isSprinting = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed) vertical -= 1f;
            if (Keyboard.current.dKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
            isSprinting = Keyboard.current.leftShiftKey.isPressed;
        }
        else
#endif
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 moveDirection = transform.right * inputDir.x + transform.forward * inputDir.z;

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        controller.Move(moveDirection * (currentSpeed * Time.deltaTime));

        // Gravity & Ground Check
        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = groundedGravity;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        controller.Move(verticalVelocity * Time.deltaTime);

        // Clamp vị trí trong Shooting Area
        if (enableShootingAreaBoundary)
        {
            ClampToShootingArea();
        }
    }

    private void ClampToShootingArea()
    {
        Vector3 pos = transform.position;
        bool clamped = false;

        if (pos.x < minX) { pos.x = minX; clamped = true; }
        if (pos.x > maxX) { pos.x = maxX; clamped = true; }
        if (pos.z < minZ) { pos.z = minZ; clamped = true; }
        if (pos.z > maxZ) { pos.z = maxZ; clamped = true; }

        if (clamped)
        {
            // Phải disable/enable CharacterController để teleport không gây lỗi
            controller.enabled = false;
            transform.position = pos;
            controller.enabled = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!enableShootingAreaBoundary) return;
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.25f);
        Vector3 center = new Vector3((minX + maxX) / 2f, transform.position.y, (minZ + maxZ) / 2f);
        Vector3 size = new Vector3(maxX - minX, 2f, maxZ - minZ);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.8f);
        Gizmos.DrawWireCube(center, size);
    }
}
