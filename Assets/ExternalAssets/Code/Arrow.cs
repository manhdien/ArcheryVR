using System;
using UnityEngine;

public enum ArrowState
{
    Nocked,  // Đang nạp trên dây cung
    Flying,  // Đang bay trong không khí
    Stuck    // Đã cắm vào mục tiêu / vật thể
}

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    [Header("Trạng thái mũi tên")]
    [SerializeField] private ArrowState state = ArrowState.Nocked;
    public ArrowState State => state;

    [Tooltip("Đã tính điểm cho phát bắn này hay chưa (ngăn tính điểm nhiều lần)")]
    public bool hasScored = false;

    [Header("Thông số bay")]
    [Tooltip("Tốc độ tối thiểu để mũi tên được coi là 'đang bay' và xoay theo hướng vận tốc")]
    public float minSpeedToRotate = 0.1f;

    [Tooltip("Góc bù nếu model mũi tên lệch trục forward")]
    public Vector3 rotationOffset = new Vector3(0f, 90f, 0f);

    [Header("Ghim mũi tên")]
    [Tooltip("Độ sâu mũi tên cắm vào vật thể (mét)")]
    public float stickDepth = 0.05f;

    [Tooltip("Layer những vật thể mũi tên có thể ghim vào (Target, Wall, Ground...)")]
    public LayerMask stickableLayers = ~0;

    [Tooltip("Thời gian (giây) trước khi mũi tên tự huỷ sau khi ghim, 0 = không tự huỷ")]
    public float destroyAfterStick = 15f;

    [Header("Hiệu ứng (tuỳ chọn)")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    [Header("Hiệu ứng Va chạm Riêng biệt (Task 5 VFX)")]
    public GameObject targetHitEffectPrefab;
    public GameObject npcHitEffectPrefab;
    public GameObject groundHitEffectPrefab;

    private Rigidbody _rb;
    private Collider _col;
    private bool _hasStuck = false;
    private TrailRenderer _trail;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        _trail = GetComponent<TrailRenderer>();

        if (_rb != null)
        {
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.mass = 0.1f;
        }
    }

    /// <summary>
    /// Phóng mũi tên từ dây cung với vận tốc ban đầu.
    /// </summary>
    public void Launch(Vector3 initialVelocity)
    {
        state = ArrowState.Flying;
        _hasStuck = false;
        hasScored = false;

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.linearVelocity = initialVelocity;
        }

        if (_col != null)
        {
            _col.enabled = true;
        }

        if (initialVelocity.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(initialVelocity.normalized) * Quaternion.Euler(rotationOffset);
        }
    }

    void FixedUpdate()
    {
        if (state != ArrowState.Flying || _hasStuck) return;

        if (_rb != null)
        {
            Vector3 velocity = _rb.linearVelocity;
            if (velocity.sqrMagnitude > minSpeedToRotate * minSpeedToRotate)
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized) * Quaternion.Euler(rotationOffset);
                transform.rotation = targetRotation;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Chống kích hoạt nhiều lần
        if (state != ArrowState.Flying || _hasStuck) return;

        // Kiểm tra layer hợp lệ
        if ((stickableLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        ContactPoint contact = collision.contacts.Length > 0 ? collision.contacts[0] : default;

        // 1. Kiểm tra đối tượng bị bắn trúng có triển khai ITarget không
        ITarget target = collision.gameObject.GetComponentInParent<ITarget>();
        if (target != null && !hasScored)
        {
            hasScored = true;
            target.OnArrowHit(this, contact.point, contact.normal);
        }

        // 2. Cắm mũi tên vào vật thể
        StickToTarget(collision, contact);
    }

    private void StickToTarget(Collision collision, ContactPoint contact)
    {
        _hasStuck = true;
        state = ArrowState.Stuck;

        Vector3 forwardDir = (_rb != null && _rb.linearVelocity.sqrMagnitude > 0.01f)
            ? _rb.linearVelocity.normalized
            : transform.forward;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }

        if (_col != null)
        {
            _col.enabled = false;
        }

        // Đặt vị trí cắm vào bề mặt
        if (contact.point != Vector3.zero)
        {
            transform.position = contact.point + forwardDir * stickDepth;
        }

        // Neo vào vật thể để di chuyển cùng (nếu target hoặc NPC di chuyển)
        transform.SetParent(collision.transform, true);

        // Hiệu ứng âm thanh & hạt va chạm (Target, NPC, Ground)
        bool isTarget = collision.gameObject.GetComponentInParent<Target>() != null;
        bool isNPC = collision.gameObject.GetComponentInParent<NPCTarget>() != null;

        if (isTarget)
        {
            AudioManager.Instance?.PlayHitTarget(contact.point);
            GameObject vfx = targetHitEffectPrefab != null ? targetHitEffectPrefab : hitEffectPrefab;
            if (vfx != null && contact.point != Vector3.zero)
                Instantiate(vfx, contact.point, Quaternion.LookRotation(contact.normal));
        }
        else if (isNPC)
        {
            AudioManager.Instance?.PlayHitNPC(contact.point);
            GameObject vfx = npcHitEffectPrefab != null ? npcHitEffectPrefab : hitEffectPrefab;
            if (vfx != null && contact.point != Vector3.zero)
                Instantiate(vfx, contact.point, Quaternion.LookRotation(contact.normal));
        }
        else
        {
            AudioManager.Instance?.PlayHitGround(contact.point);
            GameObject vfx = groundHitEffectPrefab != null ? groundHitEffectPrefab : hitEffectPrefab;
            if (vfx != null && contact.point != Vector3.zero)
                Instantiate(vfx, contact.point, Quaternion.LookRotation(contact.normal));
        }

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position);

        if (_trail != null)
            _trail.emitting = false;

        // Tự hủy sau một khoảng thời gian nếu cấu hình
        if (destroyAfterStick > 0f)
            Destroy(gameObject, destroyAfterStick);
    }
}
