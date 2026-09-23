using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Bow : MonoBehaviour
{
    [Header("Settings")]
    public Transform nockPoint;          // Kéo Transform ArrowNockPoint vào đây
    public Transform stringRestPoint;    // Vị trí dây ở trạng thái nghỉ
    public float maxPullDistance = 0.5f; // Khoảng cách kéo tối đa (mét)
    public float maxShootForce = 40f;    // Lực bắn tối đa

    private XRGrabInteractable nockInteractable;
    private ARROW currentArrow;          // Đã đổi từ Arrow -> ARROW

    void Start()
    {
        if (nockPoint != null)
            nockInteractable = nockPoint.GetComponent<XRGrabInteractable>();
    }

    // Gọi hàm này khi Mũi tên hít vào NockPoint
    public void AttachArrow(ARROW arrow)
    {
        currentArrow = arrow;

        // Gán cha con
        arrow.transform.SetParent(nockPoint);

        // Đưa về đúng gốc tọa độ và góc xoay chuẩn của NockPoint
        arrow.transform.localPosition = Vector3.zero;
        arrow.transform.localRotation = Quaternion.identity; // Đảm bảo NockPoint trong Unity đã xoay trục Z về phía trước

        // Tắt Rigidbody vật lý khi đang gài trên dây
        Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();
        if (arrowRb != null)
        {
            arrowRb.isKinematic = true;
        }

        // Tắt va chạm giữa Cung và Mũi tên để tránh bị giật lag vật lý
        Collider bowCollider = GetComponent<Collider>();
        Collider arrowCollider = arrow.GetComponent<Collider>();
        if (bowCollider != null && arrowCollider != null)
        {
            Physics.IgnoreCollision(bowCollider, arrowCollider, true);
        }
    }

    // Gọi hàm này khi nhả tay kéo dây
    public void ReleaseArrow()
    {
        if (currentArrow == null) return;

        // Bật lại va chạm vật lý sau khi bắn khỏi cung
        Collider bowCollider = GetComponent<Collider>();
        Collider arrowCollider = currentArrow.GetComponent<Collider>();
        if (bowCollider != null && arrowCollider != null)
        {
            Physics.IgnoreCollision(bowCollider, arrowCollider, false);
        }

        // Lực bắn...
        float pullDistance = Vector3.Distance(nockPoint.position, stringRestPoint.position);
        float forceRatio = Mathf.Clamp01(pullDistance / maxPullDistance);

        currentArrow.Fire(nockPoint.forward * (forceRatio * maxShootForce));
        currentArrow = null;

        nockPoint.position = stringRestPoint.position;
    }
}