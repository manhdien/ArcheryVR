using System;
using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BowShoot : MonoBehaviour
{
    [Header("BOW REFERENCES")]
    [SerializeField] private Transform stringTop;
    [SerializeField] private Transform stringBottom;
    [SerializeField] private Transform pullPoint;
    [SerializeField] private Transform bowCenter;
    [SerializeField] private Transform arrowSpawnPoint;

    [Header("CAMERA")]
    [SerializeField] private Camera playerCamera;

    [Header("ARROW")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Vector3 arrowRotationOffset = new Vector3(0f, 90f, 0f);

    [Header("PULL SETTINGS")]
    [SerializeField] private float maxPullDistance = 0.4f;
    [SerializeField] private float drawDuration = 1.0f;
    [SerializeField] private float minPullToShoot = 0.1f;

    [Header("SHOOT SETTINGS")]
    [SerializeField] private float minShootSpeed = 12f;
    [SerializeField] private float maxShootSpeed = 35f;
    [SerializeField] private float ignoreCollisionTime = 0.2f;

    [Header("AIM SETTINGS (RMB)")]
    [SerializeField] private float defaultFov = 60f;
    [SerializeField] private float aimFov = 45f;
    [SerializeField] private float fovTransitionSpeed = 10f;

    [Header("Hệ thống số lượng tên (Ammo)")]
    [Tooltip("Số lượng tên tối đa")]
    [SerializeField] private int maxAmmo = 20;

    [Tooltip("Số lượng tên hiện tại")]
    [SerializeField] private int currentAmmo = 20;

    public int MaxAmmo => maxAmmo;
    public int CurrentAmmo => currentAmmo;

    /// <summary>
    /// Event phát ra khi số lượng tên thay đổi: (int currentAmmo, int maxAmmo)
    /// </summary>
    public event Action<int, int> OnAmmoChanged;

    /// <summary>
    /// Event phát ra khi lực kéo dây cung thay đổi: (float normalizedCharge: 0.0 -> 1.0)
    /// </summary>
    public event Action<float> OnChargeChanged;

    // Runtime state
    private GameObject currentArrow;
    private Rigidbody currentArrowRb;
    private Collider[] bowColliders;

    private Vector3 pullPointRestLocalPos;
    private Vector3 pullDirection;

    private bool isDrawing = false;
    private bool isAiming = false;
    private float chargeTimer = 0f;
    private float currentPullPercent = 0f;

    public bool IsDrawing => isDrawing;
    public bool IsAiming => isAiming;
    public float CurrentPullPercent => currentPullPercent;

    private void Awake()
    {
        if (pullPoint != null)
        {
            pullPointRestLocalPos = pullPoint.localPosition;
        }

        bowColliders = GetComponentsInChildren<Collider>(true);
    }

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = GetComponentInParent<Camera>();
            }
        }

        CalculatePullDirection();

        currentAmmo = maxAmmo;
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        OnChargeChanged?.Invoke(0f);
    }

    private void CalculatePullDirection()
    {
        if (pullPoint != null && bowCenter != null)
        {
            // Vector from bowCenter (grip) to pullPoint (rest string position)
            Vector3 centerToRest = pullPoint.position - bowCenter.position;
            if (centerToRest.sqrMagnitude > 0.0001f)
            {
                pullDirection = pullPoint.parent.InverseTransformDirection(centerToRest.normalized);
            }
            else
            {
                pullDirection = Vector3.forward;
            }
        }
        else
        {
            pullDirection = Vector3.forward;
        }
    }

    private void Update()
    {
        HandleAim();
        HandleDrawInput();
        UpdatePull();
    }

    private void HandleAim()
    {
        bool aimPressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            aimPressed = Mouse.current.rightButton.isPressed;
        }
        else
#endif
        {
            aimPressed = Input.GetMouseButton(1);
        }

        isAiming = aimPressed;

        if (playerCamera != null)
        {
            float targetFov = isAiming ? aimFov : defaultFov;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, Time.deltaTime * fovTransitionSpeed);
        }
    }

    private void HandleDrawInput()
    {
        bool drawDown = false;
        bool drawHeld = false;
        bool drawUp = false;
        bool cancelPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            drawDown = Mouse.current.leftButton.wasPressedThisFrame;
            drawHeld = Mouse.current.leftButton.isPressed;
            drawUp = Mouse.current.leftButton.wasReleasedThisFrame;
        }
        if (Keyboard.current != null)
        {
            cancelPressed = Keyboard.current.rKey.wasPressedThisFrame;
        }
#endif
        if (!drawDown && Input.GetMouseButtonDown(0)) drawDown = true;
        if (!drawHeld && Input.GetMouseButton(0)) drawHeld = true;
        if (!drawUp && Input.GetMouseButtonUp(0)) drawUp = true;
        if (Input.GetKeyDown(KeyCode.R)) cancelPressed = true;

        // Cancel draw if R pressed
        if (cancelPressed && isDrawing)
        {
            CancelDraw();
            return;
        }

        // Start drawing
        if (drawDown && !isDrawing)
        {
            if (currentAmmo > 0)
            {
                StartDraw();
            }
            else
            {
                Debug.Log("[BowShoot] Hết tên! Không thể nạp mũi tên mới.");
            }
        }

        // Holding draw
        if (isDrawing && drawHeld)
        {
            chargeTimer += Time.deltaTime;
            currentPullPercent = Mathf.Clamp01(chargeTimer / drawDuration);
            OnChargeChanged?.Invoke(currentPullPercent);
        }

        // Release draw
        if (isDrawing && drawUp)
        {
            if (currentPullPercent >= minPullToShoot)
            {
                ShootArrow();
            }
            else
            {
                CancelDraw();
            }
        }
    }

    private void StartDraw()
    {
        isDrawing = true;
        chargeTimer = 0f;
        currentPullPercent = 0f;

        AudioManager.Instance?.PlayBowDraw();
        SpawnArrowOnString();
    }

    private void SpawnArrowOnString()
    {
        if (arrowPrefab == null || pullPoint == null)
            return;

        if (currentArrow != null)
            Destroy(currentArrow);

        Transform spawnOrigin = arrowSpawnPoint != null ? arrowSpawnPoint : pullPoint;
        currentArrow = Instantiate(arrowPrefab, spawnOrigin.position, spawnOrigin.rotation, pullPoint);

        currentArrowRb = currentArrow.GetComponent<Rigidbody>();
        if (currentArrowRb != null)
        {
            currentArrowRb.isKinematic = true;
            currentArrowRb.useGravity = false;
        }

        Collider arrowCol = currentArrow.GetComponent<Collider>();
        if (arrowCol != null)
        {
            arrowCol.enabled = false;
        }

        AlignArrowToString();
    }

    private void UpdatePull()
    {
        if (pullPoint == null) return;

        if (isDrawing)
        {
            // Pull the string point along pullDirection
            pullPoint.localPosition = pullPointRestLocalPos + pullDirection * (currentPullPercent * maxPullDistance);
            AlignArrowToString();
        }
        else
        {
            pullPoint.localPosition = pullPointRestLocalPos;
        }
    }

    private void AlignArrowToString()
    {
        if (currentArrow == null) return;

        Vector3 forwardDir;
        if (playerCamera != null)
        {
            forwardDir = playerCamera.transform.forward;
        }
        else if (bowCenter != null && pullPoint != null)
        {
            forwardDir = (bowCenter.position - pullPoint.position).normalized;
        }
        else
        {
            forwardDir = transform.forward;
        }

        currentArrow.transform.rotation = Quaternion.LookRotation(forwardDir) * Quaternion.Euler(arrowRotationOffset);

        if (arrowSpawnPoint != null)
        {
            currentArrow.transform.position = arrowSpawnPoint.position;
        }
    }

    private void ShootArrow()
    {
        if (currentArrow == null || currentArrowRb == null)
        {
            ResetBow();
            return;
        }

        float shootSpeed = Mathf.Lerp(minShootSpeed, maxShootSpeed, currentPullPercent);

        // Determine precise aim direction via screen center raycast
        Vector3 shootDirection;
        if (playerCamera != null)
        {
            Ray aimRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 targetPoint;
            if (Physics.Raycast(aimRay, out RaycastHit hit, 100f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = aimRay.GetPoint(100f);
            }

            Vector3 startPos = arrowSpawnPoint != null ? arrowSpawnPoint.position : pullPoint.position;
            shootDirection = (targetPoint - startPos).normalized;
        }
        else
        {
            shootDirection = transform.forward;
        }

        // Unparent arrow
        GameObject firedArrow = currentArrow;
        firedArrow.transform.SetParent(null, true);

        // Re-enable collider
        Collider arrowCol = firedArrow.GetComponent<Collider>();
        if (arrowCol != null)
        {
            arrowCol.enabled = true;
        }

        // Launch arrow
        Arrow arrowScript = firedArrow.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.rotationOffset = arrowRotationOffset;
            arrowScript.Launch(shootDirection * shootSpeed);
        }
        else
        {
            currentArrowRb.isKinematic = false;
            currentArrowRb.useGravity = true;
            currentArrowRb.linearVelocity = shootDirection * shootSpeed;
            firedArrow.transform.rotation = Quaternion.LookRotation(shootDirection) * Quaternion.Euler(arrowRotationOffset);
        }

        // Âm thanh buông cung & Camera shake nhẹ
        AudioManager.Instance?.PlayBowRelease();
        if (playerCamera != null)
        {
            var camCtrl = playerCamera.GetComponent<PlayerCameraController>();
            if (camCtrl != null)
            {
                camCtrl.ShakeCamera(0.12f, 0.03f + 0.05f * currentPullPercent);
            }
        }

        // Trừ ammo khi bắn thực tế
        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        Debug.Log($"[BowShoot] Đã bắn tên! Số tên còn lại: {currentAmmo}/{maxAmmo}");

        // Ghi nhận mũi tên đã bắn để tính Accuracy
        ScoreManager.Instance?.RecordArrowFired();

        // Kiểm tra hết tên -> Kích hoạt Game Over sau khi mũi tên chạm đích
        if (currentAmmo <= 0)
        {
            StartCoroutine(NotifyGameOverDelayed(1.8f));
        }

        // Ignore temporary collision with bow
        StartCoroutine(IgnoreBowCollisionTemporarily(firedArrow));

        // Clear current arrow reference
        currentArrow = null;
        currentArrowRb = null;

        ResetBow();
    }

    private void OnDisable()
    {
        if (isDrawing)
        {
            CancelDraw();
        }
        if (isAiming)
        {
            isAiming = false;
            if (playerCamera != null) playerCamera.fieldOfView = defaultFov;
        }
        OnChargeChanged?.Invoke(0f);
    }

    private IEnumerator NotifyGameOverDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    /// <summary>
    /// Nạp lại đầy số lượng tên.
    /// </summary>
    public void ResetAmmo()
    {
        currentAmmo = maxAmmo;
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        Debug.Log($"[BowShoot] Đã nạp lại toàn bộ tên: {currentAmmo}/{maxAmmo}");
    }

    /// <summary>
    /// Thêm số lượng tên cho người chơi.
    /// </summary>
    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }

    private void CancelDraw()
    {
        if (currentArrow != null)
        {
            Destroy(currentArrow);
            currentArrow = null;
            currentArrowRb = null;
        }

        ResetBow();
    }

    private void ResetBow()
    {
        isDrawing = false;
        chargeTimer = 0f;
        currentPullPercent = 0f;
        OnChargeChanged?.Invoke(0f);

        if (pullPoint != null)
        {
            pullPoint.localPosition = pullPointRestLocalPos;
        }
    }

    private IEnumerator IgnoreBowCollisionTemporarily(GameObject arrow)
    {
        if (arrow == null || bowColliders == null)
            yield break;

        Collider[] arrowColliders = arrow.GetComponentsInChildren<Collider>(true);
        foreach (Collider arrowCol in arrowColliders)
        {
            if (arrowCol == null) continue;
            foreach (Collider bowCol in bowColliders)
            {
                if (bowCol == null || bowCol == arrowCol) continue;
                Physics.IgnoreCollision(arrowCol, bowCol, true);
            }
        }

        yield return new WaitForSeconds(ignoreCollisionTime);

        if (arrow == null) yield break;

        foreach (Collider arrowCol in arrowColliders)
        {
            if (arrowCol == null) continue;
            foreach (Collider bowCol in bowColliders)
            {
                if (bowCol == null || bowCol == arrowCol) continue;
                Physics.IgnoreCollision(arrowCol, bowCol, false);
            }
        }
    }
}