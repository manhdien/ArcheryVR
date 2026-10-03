using System;
using UnityEngine;

/// <summary>
/// Thành phần nhận diện va chạm mũi tên và tính điểm cho các NPC tuần tra / bảo vệ.
/// </summary>
public class NPCTarget : MonoBehaviour, ITarget
{
    [Header("Điểm số NPC")]
    [Tooltip("Số điểm cộng khi bắn trúng NPC này")]
    [SerializeField] private int npcScore = 75;

    public int NpcScore
    {
        get => npcScore;
        set => npcScore = value;
    }

    /// <summary>
    /// Event phát ra khi NPC bị bắn trúng: int score, Vector3 hitPoint
    /// </summary>
    public event Action<int, Vector3> OnHit;

    /// <summary>
    /// Xử lý va chạm khi mũi tên trúng vào NPC.
    /// </summary>
    public int OnArrowHit(Arrow arrow, Vector3 hitPoint, Vector3 hitNormal)
    {
        int score = npcScore;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(score, "NPC", hitPoint);
        }

        OnHit?.Invoke(score, hitPoint);
        Debug.Log($"[NPCTarget] NPC '{name}' hit! -> +{score} pts");

        return score;
    }
}
