using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ARROW : MonoBehaviour
{
    private Rigidbody rb;
    private bool isFired = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 force)
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(force, ForceMode.Impulse);
        isFired = true;
    }

    void Update()
    {
        // Làm mũi tên xoay theo hướng bay khi đang trên không
        if (isFired && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.forward = rb.linearVelocity.normalized;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFired)
        {
            isFired = false;
            rb.isKinematic = true; // Cắm mũi tên vào mục tiêu
            transform.SetParent(collision.transform);
        }
    }
}