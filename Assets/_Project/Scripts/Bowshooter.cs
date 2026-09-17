using UnityEngine;
using UnityEngine.InputSystem;

public class MyBowShooter : MonoBehaviour
{
    [Header("Tham chiếu")]
    [Tooltip("Điểm đặt mũi tên khi lên dây (thường là 1 Empty GameObject ở đầu cung)")]
    public Transform nockPoint;

    [Tooltip("Prefab mũi tên (phải có script MyArrow.cs)")]
    public GameObject arrowPrefab;

    [Header("Input (XR)")]
    [Tooltip("Action đọc giá trị Trigger (0-1) của tay cầm VR. " +
             "Mặc định bind sẵn vào <XRController>/trigger, có thể đổi trong Inspector " +
             "hoặc trỏ tới 1 action trong Input Action Asset của project (ví dụ XRI Default Input Actions).")]
    public InputActionProperty drawAction =
        new InputActionProperty(new InputAction("Draw Bow", InputActionType.Value, "<XRController>/trigger"));

    [Tooltip("Giá trị Trigger (0-1) tối thiểu để tính là bắt đầu kéo cung")]
    [Range(0f, 1f)] public float pressThreshold = 0.1f;

    [Header("Lực bắn")]
    [Tooltip("Lực bắn tối thiểu (khi vừa nhấn Trigger)")]
    public float minForce = 5f;

    [Tooltip("Lực bắn tối đa (khi kéo hết cỡ)")]
    public float maxForce = 40f;

    [Tooltip("Thời gian (giây) để kéo từ lực tối thiểu đến tối đa")]
    public float maxChargeTime = 1.5f;

    [Tooltip("Đường cong lực theo thời gian kéo, chỉnh trong Inspector để mô phỏng lực dây cung phi tuyến")]
    public AnimationCurve forceCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Hồi chiêu")]
    [Tooltip("Thời gian chờ tối thiểu giữa 2 lần bắn")]
    public float shootCooldown = 0.3f;
    private bool _isDrawing = false;
    private float _chargeTimer = 0f;
    private float _lastShootTime = -999f;
    private GameObject _currentArrow;

    void OnEnable()
    {
        drawAction.action.Enable();
    }

    void OnDisable()
    {
        drawAction.action.Disable();
    }

    void Update()
    {
        HandleDrawInput();
    }

    private void HandleDrawInput()
    {
        float triggerValue = drawAction.action.ReadValue<float>();
        bool isPressed = triggerValue >= pressThreshold;
        if (isPressed && !_isDrawing && Time.time - _lastShootTime >= shootCooldown)
        {
            StartDraw();
        }
        if (_isDrawing && isPressed)
        {
            _chargeTimer += Time.deltaTime;
            _chargeTimer = Mathf.Clamp(_chargeTimer, 0f, maxChargeTime);

            float chargeRatio = _chargeTimer / maxChargeTime;
            OnCharging(chargeRatio);
        }
        if (_isDrawing && !isPressed)
        {
            Shoot();
        }
    }

    private void StartDraw()
    {
        _isDrawing = true;
        _chargeTimer = 0f;
        if (arrowPrefab != null && nockPoint != null)
        {
            _currentArrow = Instantiate(arrowPrefab, nockPoint.position, nockPoint.rotation, nockPoint);
            Rigidbody rb = _currentArrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            Collider col = _currentArrow.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }
    protected virtual void OnCharging(float chargeRatio)
    {

    }

    private void Shoot()
    {
        _isDrawing = false;
        _lastShootTime = Time.time;

        float chargeRatio = _chargeTimer / maxChargeTime;
        float curvedRatio = forceCurve.Evaluate(chargeRatio);
        float finalForce = Mathf.Lerp(minForce, maxForce, curvedRatio);

        GameObject arrowToLaunch = _currentArrow;

        if (arrowToLaunch == null && arrowPrefab != null && nockPoint != null)
        {
            arrowToLaunch = Instantiate(arrowPrefab, nockPoint.position, nockPoint.rotation);
        }

        if (arrowToLaunch != null)
        {
            arrowToLaunch.transform.SetParent(null);
            Collider col = arrowToLaunch.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            MyArrow arrowScript = arrowToLaunch.GetComponent<MyArrow>();
            Vector3 shootDirection = nockPoint.forward;

            if (arrowScript != null)
            {
                arrowScript.Launch(shootDirection * finalForce);
            }
            else
            {
                Rigidbody rb = arrowToLaunch.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.linearVelocity = shootDirection * finalForce;
                }
            }
        }

        _currentArrow = null;
        _chargeTimer = 0f;
    }
}