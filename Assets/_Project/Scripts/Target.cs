using System;
using UnityEngine;

/// <summary>
/// Quản lý bia tập bắn. Task 6: mọi phát trúng bia đều được +10 điểm (flat).
/// </summary>
public class Target : MonoBehaviour, ITarget
{
    [Header("Điểm số")]
    [Tooltip("Điểm cố định khi bắn trúng bia (Task 6: flat 10 điểm)")]
    public int hitScore = 10;

    [Header("Tham chiếu Hierarchy (Tùy chọn)")]
    public Transform targetCenter;

    /// <summary>
    /// Event phát ra khi bia bị bắn trúng: int score, string hitType, Vector3 hitPoint
    /// </summary>
    public event Action<int, string, Vector3> OnHit;

    void Awake()
    {
        if (targetCenter == null)
        {
            // Tự tìm hoặc lấy chính transform nếu chưa gán
            Transform foundTarget = transform.Find("Target");
            targetCenter = foundTarget != null ? foundTarget : transform;
        }
    }

    /// <summary>
    /// Xử lý va chạm từ mũi tên và tính điểm flat +10.
    /// </summary>
    public int OnArrowHit(Arrow arrow, Vector3 hitPoint, Vector3 hitNormal)
    {
        int score = hitScore;
        const string hitType = "Hit";

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(score, hitType, hitPoint);
        }

        OnHit?.Invoke(score, hitType, hitPoint);
        Debug.Log($"[Target] '{name}' hit! -> +{score} pts");

        return score;
    }

    /// <summary>
    /// API hỗ trợ reset trạng thái của bia khi bắt đầu ván mới.
    /// </summary>
    public void ResetTarget()
    {
        // Có thể mở rộng để dọn mũi tên cắm trên bia nếu cần
    }

    void OnDrawGizmosSelected()
    {
        Transform c = targetCenter != null ? targetCenter : transform;
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.8f);
        Gizmos.DrawWireSphere(c.position, 0.3f);
    }
}
