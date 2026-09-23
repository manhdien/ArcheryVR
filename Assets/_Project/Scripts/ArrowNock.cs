using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ArrowNock : MonoBehaviour
{
    [Header("Nock")]
    public Transform nockPoint;

    [Header("Settings")]
    public float nockDistance = 0.12f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private bool isNocked = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isNocked)
            return;

        if (grabInteractable != null &&
            grabInteractable.isSelected &&
            nockPoint != null)
        {
            float distance = Vector3.Distance(
                transform.position,
                nockPoint.position
            );

            if (distance <= nockDistance)
            {
                NockArrow();
            }
        }
    }

    private void NockArrow()
    {
        isNocked = true;

        // Đưa tên đúng vào vị trí nock
        transform.SetParent(nockPoint);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Tắt vật lý khi đang nằm trên dây
        rb.isKinematic = true;

        // Không cho tay tiếp tục giữ tên
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }
    }
}