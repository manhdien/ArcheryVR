using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BowString : MonoBehaviour
{
    [Header("Cấu hình bắn")]
    public GameObject arrowPrefab;
    public Transform bowTransform;
    public float shootForce = 20f;
    public float pullThreshold = 0.2f;  // Khoảng cách kéo tối thiểu
    public float maxPullDistance = 0.5f; // Khoảng cách kéo tối đa
    public float returnSpeed = 15f;

    private XRGrabInteractable grabInteractable;
    private Vector3 initialLocalPosition;
    private bool isPulled = false;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            // Giới hạn chuyển động của dây cung chỉ lùi về phía sau theo trục Z local
            Vector3 currentLocalPos = transform.localPosition;

            // Giới hạn khoảng cách kéo giữa 0 và maxPullDistance (hướng lùi -Z)
            float zOffset = Mathf.Clamp(currentLocalPos.z, -maxPullDistance, 0f);

            transform.localPosition = new Vector3(initialLocalPosition.x, initialLocalPosition.y, zOffset);

            // Tính khoảng cách đã kéo
            float pullDistance = Vector3.Distance(transform.localPosition, initialLocalPosition);

            if (pullDistance > pullThreshold)
            {
                isPulled = true;
            }
        }
        else
        {
            // Khi thả tay ra
            if (isPulled)
            {
                ShootArrow();
                isPulled = false;
            }

            // Trả dây về vị trí ban đầu
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPosition, Time.deltaTime * returnSpeed);
        }
    }

    void ShootArrow()
    {
        if (arrowPrefab != null && bowTransform != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, transform.position, bowTransform.rotation);
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(bowTransform.forward * shootForce, ForceMode.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("Chưa gán Arrow Prefab hoặc Bow Transform!");
        }
    }
}