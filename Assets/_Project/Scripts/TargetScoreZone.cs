using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gắn lên GameObject mặt bia (Target) để phát hiện mũi tên va chạm,
/// tính toán khoảng cách từ điểm va chạm tới tâm bia và cộng điểm tương ứng.
/// </summary>
public class TargetScoreZone : MonoBehaviour
{
    [Serializable]
    public struct ScoreZone
    {
        public string zoneName;
        [Tooltip("Bán kính tối đa của vòng tính từ tâm (mét)")]
        public float maxRadius;
        [Tooltip("Số điểm nhận được khi bắn trúng vòng này")]
        public int points;
        public Color gizmoColor;

        public ScoreZone(string name, float radius, int pts, Color color)
        {
            zoneName = name;
            maxRadius = radius;
            points = pts;
            gizmoColor = color;
        }
    }

    [Header("Cấu hình tâm bia")]
    [Tooltip("Transform tâm bia. Nếu để trống sẽ mặc định dùng Transform của chính GameObject này.")]
    public Transform targetCenter;

    [Header("Các vòng tính điểm (theo thứ tự từ trong ra ngoài)")]
    public ScoreZone[] scoreZones;

    [Header("Sự kiện trúng bia cục bộ")]
    public UnityEvent<int, string, Vector3> onHitScored;

    private void Reset()
    {
        targetCenter = transform;
        // Cấu hình mặc định theo chuẩn kích thước bia ArcheryTarget (~1.16m đường kính)
        scoreZones = new ScoreZone[]
        {
            new ScoreZone("Tâm vàng (Bullseye)", 0.08f, 10, Color.yellow),
            new ScoreZone("Vòng đỏ", 0.18f, 8, Color.red),
            new ScoreZone("Vòng xanh", 0.30f, 6, Color.blue),
            new ScoreZone("Vòng đen", 0.42f, 4, Color.black),
            new ScoreZone("Vòng trắng", 0.58f, 2, Color.white)
        };
    }

    private void Awake()
    {
        if (targetCenter == null)
        {
            targetCenter = transform;
        }

        if (scoreZones == null || scoreZones.Length == 0)
        {
            Reset();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Kiểm tra xem vật va chạm có phải là mũi tên hay không (hỗ trợ cả MyArrow và Arrow)
        GameObject hitObj = collision.gameObject;
        bool isArrow = hitObj.GetComponent<MyArrow>() != null ||
                       hitObj.GetComponent<Arrow>() != null ||
                       hitObj.GetComponentInParent<MyArrow>() != null ||
                       hitObj.GetComponentInParent<Arrow>() != null ||
                       hitObj.CompareTag("Arrow");

        if (!isArrow)
        {
            // Bỏ qua nếu vật thể va chạm không phải là mũi tên
            return;
        }

        // 2. Lấy điểm tiếp xúc đầu tiên
        if (collision.contactCount == 0) return;
        ContactPoint contact = collision.contacts[0];
        Vector3 worldHitPoint = contact.point;

        // 3. Quy đổi điểm chạm sang hệ tọa độ cục bộ của mặt bia
        Transform center = targetCenter != null ? targetCenter : transform;
        Vector3 localHitPoint = center.InverseTransformPoint(worldHitPoint);

        // Mặt bia ArcheryTarget tròn trên mặt phẳng X-Y cục bộ
        float hitRadius = Mathf.Sqrt(localHitPoint.x * localHitPoint.x + localHitPoint.y * localHitPoint.y);

        // 4. Tra cứu vùng điểm từ trong ra ngoài
        int pointsEarned = 0;
        string hitZoneName = "Ngoài bia";

        for (int i = 0; i < scoreZones.Length; i++)
        {
            if (hitRadius <= scoreZones[i].maxRadius)
            {
                pointsEarned = scoreZones[i].points;
                hitZoneName = scoreZones[i].zoneName;
                break;
            }
        }

        // 5. Gửi điểm số tới ScoreManager nếu trúng vào các vòng có điểm
        if (pointsEarned > 0)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(pointsEarned, hitZoneName, hitObj.GetHashCode(), worldHitPoint);
            }
            else
            {
                Debug.LogWarning("[TargetScoreZone] Không tìm thấy ScoreManager.Instance trong scene!");
            }

            onHitScored?.Invoke(pointsEarned, hitZoneName, worldHitPoint);
        }
        else
        {
            Debug.Log($"[TargetScoreZone] Mũi tên chạm bia nhưng ở ngoài các vòng tính điểm (Bán kính: {hitRadius:F2}m).");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform center = targetCenter != null ? targetCenter : transform;
        if (center == null || scoreZones == null) return;

        Matrix4x4 prevMatrix = Gizmos.matrix;
        Gizmos.matrix = center.localToWorldMatrix;

        for (int i = 0; i < scoreZones.Length; i++)
        {
            Gizmos.color = scoreZones[i].gizmoColor;
            DrawWireCircle(scoreZones[i].maxRadius, 32);
        }

        Gizmos.matrix = prevMatrix;
    }

    private void DrawWireCircle(float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}
