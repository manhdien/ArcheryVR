using TMPro;
using UnityEngine;

/// <summary>
/// Quản lý hiển thị Bảng điểm VR (World Space UI).
/// Lắng nghe sự kiện từ ScoreManager để cập nhật điểm số, lần bắn gần nhất và số mũi tên trúng.
/// </summary>
public class ScoreboardUI : MonoBehaviour
{
    [Header("Tham chiếu TextMeshPro UI")]
    [Tooltip("Text hiển thị tổng điểm (cỡ to, nổi bật)")]
    public TextMeshProUGUI totalScoreText;

    [Tooltip("Text hiển thị kết quả phát bắn gần nhất (ví dụ: +10 Tâm vàng!)")]
    public TextMeshProUGUI lastHitText;

    [Tooltip("Text hiển thị tổng số mũi tên trúng bia")]
    public TextMeshProUGUI hitCountText;

    [Header("Định dạng hiển thị")]
    public string totalScoreFormat = "{0}";
    public string lastHitFormat = "+{0} ({1})";
    public string hitCountFormat = "Trúng: {0}";

    private bool _isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        if (ScoreManager.Instance != null)
        {
            UpdateDisplay(ScoreManager.Instance.TotalScore, ScoreManager.Instance.LastAddedScore, ScoreManager.Instance.LastZoneName, ScoreManager.Instance.HitCount);
        }
        else
        {
            UpdateDisplay(0, 0, "Sẵn sàng", 0);
        }
    }

    private void TrySubscribe()
    {
        if (_isSubscribed) return;
        var sm = ScoreManager.Instance != null ? ScoreManager.Instance : FindFirstObjectByType<ScoreManager>();
        if (sm != null)
        {
            sm.OnScoreChangedEvent += HandleScoreChanged;
            sm.OnScoreResetEvent += HandleScoreReset;
            _isSubscribed = true;
        }
    }

    private void OnDisable()
    {
        if (_isSubscribed)
        {
            var sm = ScoreManager.Instance != null ? ScoreManager.Instance : FindFirstObjectByType<ScoreManager>();
            if (sm != null)
            {
                sm.OnScoreChangedEvent -= HandleScoreChanged;
                sm.OnScoreResetEvent -= HandleScoreReset;
            }
            _isSubscribed = false;
        }
    }

    private void HandleScoreChanged(int totalScore, int addedPoints, string zoneName)
    {
        int hitCount = ScoreManager.Instance != null ? ScoreManager.Instance.HitCount : 0;
        UpdateDisplay(totalScore, addedPoints, zoneName, hitCount);
    }

    private void HandleScoreReset()
    {
        UpdateDisplay(0, 0, "Bắt đầu mới", 0);
    }

    /// <summary>
    /// Cập nhật nội dung hiển thị trên bảng điểm
    /// </summary>
    public void UpdateDisplay(int totalScore, int addedPoints, string zoneName, int hitCount)
    {
        if (totalScoreText != null)
        {
            totalScoreText.text = string.Format(totalScoreFormat, totalScore);
        }

        if (lastHitText != null)
        {
            if (addedPoints > 0)
            {
                lastHitText.text = string.Format(lastHitFormat, addedPoints, zoneName);
            }
            else if (!string.IsNullOrEmpty(zoneName))
            {
                lastHitText.text = zoneName;
            }
            else
            {
                lastHitText.text = "Sẵn sàng";
            }
        }

        if (hitCountText != null)
        {
            hitCountText.text = string.Format(hitCountFormat, hitCount);
        }
    }

    /// <summary>
    /// Nút gọi để đặt lại bảng điểm từ giao diện (nếu có UI Button)
    /// </summary>
    public void OnResetButtonClicked()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
    }
}
