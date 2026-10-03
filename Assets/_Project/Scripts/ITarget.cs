using UnityEngine;

/// <summary>
/// Giao diện chuẩn cho tất cả các đối tượng có thể bị bắn trúng và tính điểm (Target, NPC, v.v.).
/// </summary>
public interface ITarget
{
    /// <summary>
    /// Xử lý khi mũi tên cắm trúng mục tiêu.
    /// </summary>
    /// <param name="arrow">Mũi tên va chạm</param>
    /// <param name="hitPoint">Tọa độ tiếp xúc trong không gian thế giới</param>
    /// <param name="hitNormal">Pháp tuyến tại điểm tiếp xúc</param>
    /// <returns>Điểm số nhận được từ phát bắn</returns>
    int OnArrowHit(Arrow arrow, Vector3 hitPoint, Vector3 hitNormal);
}
