using UnityEngine;
public class BowShooter : MonoBehaviour
{
    [Header("Tham chiếu")]
    [Tooltip("Điểm đặt mũi tên khi lên dây (thường là 1 Empty GameObject ở đầu cung)")]
    public Transform nockPoint;

    [Tooltip("Prefab mũi tên (phải có script Arrow.cs)")]
    public GameObject arrowPrefab;

    [Header("Lực bắn")]
    [Tooltip("Lực bắn tối thiểu (khi vừa nhấn nút)")]
    public float minForce = 5f;

    [Tooltip("Lực bắn tối đa (khi kéo hết cỡ)")]
    public float maxForce = 40f;

    [Tooltip("Thời gian (giây) để kéo từ lực tối thiểu đến tối đa")]
    public float maxChargeTime = 1.5f;

    [Tooltip("Đường cong lực theo thời gian kéo (mặc định: tuyến tính). " +
             "Có thể chỉnh trong Inspector để mô phỏng lực dây cung phi tuyến.")]
    public AnimationCurve forceCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Input (tuỳ hệ thống input của bạn)")]
    public KeyCode drawKey = KeyCode.Mouse0;

    [Header("Hồi chiêu")]
    [Tooltip("Thời gian chờ tối thiểu giữa 2 lần bắn")]
    public float shootCooldown = 0.3f;
    private bool _isDrawing = false;
    private float _chargeTimer = 0f;
    private float _lastShootTime = -999f;
    private GameObject _currentArrow;

    void Update()
    {
        HandleDrawInput();
    }

    private void HandleDrawInput()
    {
        if (Input.GetKeyDown(drawKey) && Time.time - _lastShootTime >= shootCooldown)
        {
            StartDraw();
        }
        if (_isDrawing && Input.GetKey(drawKey))
        {
            _chargeTimer += Time.deltaTime;
            _chargeTimer = Mathf.Clamp(_chargeTimer, 0f, maxChargeTime);
            float chargeRatio = _chargeTimer / maxChargeTime;
            OnCharging(chargeRatio);
        }
        if (_isDrawing && Input.GetKeyUp(drawKey))
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

            Arrow arrowScript = arrowToLaunch.GetComponent<Arrow>();
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
