using System;
using UnityEngine;

/// <summary>
/// Quản lý điểm số tập trung (Single Source of Truth) cho toàn bộ game bắn cung.
/// Task 6: Thêm TotalArrowsUsed và Accuracy tracking.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private static ScoreManager _instance;
    public static ScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindAnyObjectByType<ScoreManager>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Điểm số hiện tại")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int totalHits = 0;
    [SerializeField] private int totalArrowsUsed = 0;

    public int CurrentScore => currentScore;
    public int TotalHits => totalHits;

    /// <summary>Tổng số mũi tên đã bắn (kể cả trượt)</summary>
    public int TotalArrowsUsed => totalArrowsUsed;

    /// <summary>Độ chính xác: TotalHits / TotalArrowsUsed * 100 (%), 0 nếu chưa bắn</summary>
    public float Accuracy => totalArrowsUsed > 0 ? (float)totalHits / totalArrowsUsed * 100f : 0f;

    /// <summary>
    /// Event kích hoạt khi điểm số thay đổi: int newScore
    /// </summary>
    public event Action<int> OnScoreChanged;

    /// <summary>
    /// Event kích hoạt khi có phát bắn trúng: int scoreAdded, string hitType, Vector3 hitPosition
    /// </summary>
    public event Action<int, string, Vector3> OnTargetHit;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Cộng điểm cho người chơi từ phát bắn trúng.
    /// </summary>
    /// <param name="amount">Số điểm cộng</param>
    /// <param name="hitType">Tên loại trúng ("Hit", "NPC", v.v.)</param>
    /// <param name="hitPosition">Vị trí va chạm trong thế giới</param>
    public void AddScore(int amount, string hitType = "", Vector3 hitPosition = default)
    {
        if (amount <= 0) return;

        currentScore += amount;
        totalHits++;

        Debug.Log($"[ScoreManager] +{amount} pts! Total Score: {currentScore} | Hits: {totalHits}/{totalArrowsUsed} | Accuracy: {Accuracy:F1}%");

        OnScoreChanged?.Invoke(currentScore);
        OnTargetHit?.Invoke(amount, hitType, hitPosition);
    }

    /// <summary>
    /// Ghi nhận một mũi tên đã bắn (cả trúng lẫn trượt).
    /// Gọi từ BowShoot.ShootArrow() mỗi khi bắn.
    /// </summary>
    public void RecordArrowFired()
    {
        totalArrowsUsed++;
        Debug.Log($"[ScoreManager] Arrow fired. Total arrows used: {totalArrowsUsed}");
    }

    /// <summary>
    /// Trả về điểm số hiện tại.
    /// </summary>
    public int GetScore()
    {
        return currentScore;
    }

    /// <summary>
    /// Đặt lại toàn bộ điểm số về 0 (chuẩn bị ván chơi mới).
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        totalHits = 0;
        totalArrowsUsed = 0;
        Debug.Log("[ScoreManager] Score has been reset to 0.");
        OnScoreChanged?.Invoke(currentScore);
    }
}
