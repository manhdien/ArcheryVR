using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    [Header("Thông số bay")]
    [Tooltip("Tốc độ tối thiểu để mũi tên được coi là 'đang bay' và xoay theo hướng vận tốc")]
    public float minSpeedToRotate = 0.1f;

    [Header("Ghim mũi tên")]
    [Tooltip("Độ sâu mũi tên cắm vào vật thể (mét)")]
    public float stickDepth = 0.05f;

    [Tooltip("Layer những vật thể mũi tên có thể ghim vào (Target, Wall, Ground...)")]
    public LayerMask stickableLayers = ~0;

    [Tooltip("Thời gian (giây) trước khi mũi tên tự huỷ sau khi ghim, 0 = không tự huỷ")]
    public float destroyAfterStick = 10f;

    [Header("Hiệu ứng (tuỳ chọn)")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    private Rigidbody _rb;
    private Collider _col;
    private bool _hasStuck = false;
    private TrailRenderer _trail;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        _trail = GetComponent<TrailRenderer>();


        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }


    public void Launch(Vector3 initialVelocity)
    {
        _hasStuck = false;
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.linearVelocity = initialVelocity;
        transform.rotation = Quaternion.LookRotation(initialVelocity.normalized);
    }

    void FixedUpdate()
    {
        if (_hasStuck) return;


        Vector3 velocity = _rb.linearVelocity;
        if (velocity.sqrMagnitude > minSpeedToRotate * minSpeedToRotate)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = targetRotation;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_hasStuck) return;
        if ((stickableLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        StickToTarget(collision);
    }

    private void StickToTarget(Collision collision)
    {
        _hasStuck = true;

        ContactPoint contact = collision.contacts[0];
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        if (_col != null) _col.enabled = false;
        transform.position = contact.point + transform.forward * stickDepth;
        transform.SetParent(collision.transform, true);
        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, contact.point);

        if (_trail != null)
            _trail.emitting = false;

        if (destroyAfterStick > 0f)
            Destroy(gameObject, destroyAfterStick);
    }
}
