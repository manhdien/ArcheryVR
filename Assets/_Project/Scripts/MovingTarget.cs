using UnityEngine;

/// <summary>
/// Làm cho bia (ArcheryTarget) bay di chuyển lượn sóng theo hướng X và Y quanh vị trí gốc (Origin Position).
/// Mỗi bia có vị trí xuất phát riêng biệt, chuyển động liên tục và độc lập.
/// Giữ trong phạm vi TargetMovementArea và giữ khoảng cách an toàn với Player.
/// </summary>
public class MovingTarget : MonoBehaviour
{
    [Header("Vị trí gốc (Origin Position)")]
    [Tooltip("Tọa độ gốc độc lập của từng bia. Quỹ đạo bay dao động quanh tọa độ này.")]
    [SerializeField] public Vector3 originPosition;

    [Header("Chuyển động")]
    [Tooltip("Tốc độ di chuyển ngang (mét/giây)")]
    public float horizontalSpeed = 1.5f;

    [Tooltip("Tốc độ dao động theo Y (mét/giây)")]
    public float verticalSpeed = 0.8f;

    [Tooltip("Biên độ di chuyển ngang (mét)")]
    public float horizontalRange = 3.5f;

    [Tooltip("Biên độ di chuyển dọc (mét) — bay lên/xuống")]
    public float verticalRange = 1.0f;

    [Tooltip("Offset pha chuyển động để các bia không di chuyển đồng bộ")]
    public float phaseOffset = 0f;

    [Header("Giới hạn khu vực Target Movement")]
    [Tooltip("Giới hạn Z tối thiểu của bia (phía gần Player)")]
    public float minZ = 20f;

    [Tooltip("Giới hạn Z tối đa của bia (phía xa Player)")]
    public float maxZ = 50f;

    [Tooltip("Giới hạn X tối thiểu (bên trái)")]
    public float minX = 8f;

    [Tooltip("Giới hạn X tối đa (bên phải)")]
    public float maxX = 45f;

    [Tooltip("Độ cao Y tối thiểu tính từ mặt đất")]
    public float minY = 0.5f;

    [Tooltip("Độ cao Y tối đa")]
    public float maxY = 6.0f;

    [Header("Khoảng cách an toàn với Player")]
    [Tooltip("Khoảng cách tối thiểu (mét) giữa bia và player")]
    public float minPlayerDistance = 8f;

    private Transform _playerTransform;
    private bool _isInitialized = false;

    void Awake()
    {
        InitializeIfNeeded();
    }

    void Start()
    {
        InitializeIfNeeded();
        FindPlayer();
    }

    /// <summary>
    /// Khởi tạo vị trí gốc nếu chưa được thiết lập từ Editor
    /// </summary>
    public void InitializeIfNeeded()
    {
        if (_isInitialized) return;

        // Nếu originPosition chưa được gán (hoặc bằng zero) nhưng transform.position hợp lệ
        if (originPosition == Vector3.zero && transform.position != Vector3.zero)
        {
            originPosition = transform.position;
        }
        else if (originPosition != Vector3.zero && transform.position == Vector3.zero)
        {
            transform.position = originPosition;
        }

        // Đảm bảo phaseOffset không bị trùng
        if (phaseOffset == 0f)
        {
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Thiết lập vị trí gốc cụ thể cho bia và đặt Transform vào vị trí này
    /// </summary>
    public void SetOriginPosition(Vector3 newOrigin)
    {
        originPosition = newOrigin;
        transform.position = newOrigin;
        _isInitialized = true;
    }

    private void FindPlayer()
    {
        if (_playerTransform == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null) playerGO = GameObject.Find("PC_Player");
            if (playerGO != null) _playerTransform = playerGO.transform;
        }
    }

    void Update()
    {
        // Đảm bảo có vị trí gốc hợp lệ
        if (originPosition == Vector3.zero)
        {
            if (transform.position != Vector3.zero)
            {
                originPosition = transform.position;
            }
            else
            {
                return; // Tránh tính toán sai khi chưa có tọa độ
            }
        }

        float t = Time.time + phaseOffset;

        // Tính độ lệch Sinwave quanh vị trí gốc
        float offsetX = Mathf.Sin(t * horizontalSpeed) * horizontalRange;
        float offsetY = Mathf.Sin(t * verticalSpeed) * verticalRange;

        Vector3 targetPos = new Vector3(
            originPosition.x + offsetX,
            originPosition.y + offsetY,
            originPosition.z
        );

        // Clamp trong phạm vi cho phép của khu vực bia
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        // Giữ khoảng cách an toàn với Player
        if (_playerTransform != null)
        {
            Vector3 toTarget = targetPos - _playerTransform.position;
            toTarget.y = 0f; // Chỉ xét khoảng cách mặt phẳng XZ
            float distXZ = toTarget.magnitude;
            if (distXZ < minPlayerDistance && distXZ > 0.001f)
            {
                Vector3 safeDir = toTarget.normalized;
                Vector3 safePosXZ = _playerTransform.position + safeDir * minPlayerDistance;
                targetPos.x = Mathf.Clamp(safePosXZ.x, minX, maxX);
                targetPos.z = Mathf.Clamp(safePosXZ.z, minZ, maxZ);
            }
        }

        transform.position = targetPos;
    }

    /// <summary>
    /// Đưa bia về vị trí gốc ban đầu (gọi khi Restart ván chơi)
    /// </summary>
    public void ResetToOrigin()
    {
        if (originPosition != Vector3.zero)
        {
            transform.position = originPosition;
        }
    }

    [ContextMenu("Capture Current Position as Origin")]
    public void CaptureCurrentPosition()
    {
        originPosition = transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = originPosition != Vector3.zero ? originPosition : transform.position;
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
        Gizmos.DrawWireCube(center, new Vector3(horizontalRange * 2f, verticalRange * 2f, 1f));
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, 0.2f);
    }
}

