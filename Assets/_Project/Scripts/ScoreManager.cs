using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý điểm số trung tâm của trò chơi bắn cung VR.
/// Hoạt động độc lập, không can thiệp vào logic của cung và mũi tên có sẵn.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Trạng thái hiện tại")]
    [SerializeField] private int _totalScore = 0;
    [SerializeField] private int _hitCount = 0;
    [SerializeField] private int _lastAddedScore = 0;
    [SerializeField] private string _lastZoneName = string.Empty;

    public int TotalScore => _totalScore;
    public int HitCount => _hitCount;
    public int LastAddedScore => _lastAddedScore;
    public string LastZoneName => _lastZoneName;

    [Header("Sự kiện")]
    [Tooltip("Sự kiện bắn ra khi điểm số thay đổi: (Tổng điểm, Điểm vừa cộng, Tên vùng trúng)")]
    public UnityEvent<int, int, string> onScoreChanged;

    [Tooltip("Sự kiện khi reset bảng điểm")]
    public UnityEvent onScoreReset;

    // C# Actions cho lập trình viên
    public event Action<int, int, string> OnScoreChangedEvent;
    public event Action OnScoreResetEvent;

    // Tập hợp lưu trữ ID của các mũi tên đã được cộng điểm nhằm chống tính điểm lặp lại
    private readonly HashSet<int> _processedArrowIds = new HashSet<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Khởi tạo hiển thị ban đầu
        NotifyScoreChanged(0, string.Empty);
    }

    /// <summary>
    /// Cộng điểm cho người chơi khi mũi tên bắn trúng bia.
    /// </summary>
    /// <param name="points">Số điểm của phát bắn</param>
    /// <param name="zoneName">Tên vùng trúng (ví dụ: Tâm vàng, Vòng đỏ...)</param>
    /// <param name="arrowInstanceId">Mã định danh duy nhất của mũi tên</param>
    /// <param name="hitPosition">Tọa độ va chạm</param>
    /// <returns>True nếu điểm được cộng thành công, False nếu mũi tên này đã được tính điểm trước đó</returns>
    public bool AddScore(int points, string zoneName, int arrowInstanceId, Vector3 hitPosition)
    {
        // Chống tính điểm lặp lại: nếu mũi tên này đã từng tính điểm thì bỏ qua
        if (_processedArrowIds.Contains(arrowInstanceId))
        {
            Debug.Log($"[ScoreManager] Mũi tên (ID: {arrowInstanceId}) đã được tính điểm trước đó, bỏ qua.");
            return false;
        }

        _processedArrowIds.Add(arrowInstanceId);

        _totalScore += points;
        _hitCount++;
        _lastAddedScore = points;
        _lastZoneName = zoneName;

        Debug.Log($"[ScoreManager] TRÚNG BIA! Vùng: {zoneName} | +{points} điểm | Tổng: {_totalScore} | Vị trí: {hitPosition}");

        NotifyScoreChanged(points, zoneName);
        return true;
    }

    /// <summary>
    /// Reset điểm số và xóa danh sách mũi tên đã xử lý.
    /// </summary>
    public void ResetScore()
    {
        _totalScore = 0;
        _hitCount = 0;
        _lastAddedScore = 0;
        _lastZoneName = string.Empty;
        _processedArrowIds.Clear();

        Debug.Log("[ScoreManager] Bảng điểm đã được đặt lại về 0.");

        onScoreReset?.Invoke();
        OnScoreResetEvent?.Invoke();
        NotifyScoreChanged(0, "Sẵn sàng");
    }

    private void NotifyScoreChanged(int addedScore, string zoneName)
    {
        onScoreChanged?.Invoke(_totalScore, addedScore, zoneName);
        OnScoreChangedEvent?.Invoke(_totalScore, addedScore, zoneName);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
