using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArrowStick : MonoBehaviour
{
    [SerializeField] private Transform arrowTipPoint;

    // Độ sâu đầu tên xuyên vào bia, tính bằng mét.
    [SerializeField] private float penetrationDepth = 0.05f;

    private Rigidbody rb;
    private bool stuck;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stuck || rb.isKinematic)
            return;

        // Chỉ ghim vào mặt bia có tag Target.
        if (!collision.collider.CompareTag("Target"))
            return;

        if (arrowTipPoint == null)
        {
            Debug.LogError("Chưa gán Arrow Tip Point!", this);
            return;
        }

        ContactPoint contact = collision.GetContact(0);
        stuck = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.detectCollisions = false;

        // Đi từ bề mặt vào phía trong bia.
        Vector3 embeddedTipPosition =
            contact.point - contact.normal * penetrationDepth;

        // Di chuyển toàn bộ tên để đầu nhọn nằm trong bia.
        transform.position +=
            embeddedTipPosition - arrowTipPoint.position;

        // Giữ nguyên vị trí và hướng, cho tên đi theo bia.
        transform.SetParent(collision.collider.transform, true);
    }
}