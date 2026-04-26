using UnityEngine;

public class FanForce2D : MonoBehaviour
{
    [Header("Fan Settings")]
    public float maxForce = 20f;     // แรงลมสูงสุด
    public float range = 5f;         // ระยะลม
    public Vector2 direction = Vector2.right; // ทิศลม

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.attachedRigidbody == null) return;

        Rigidbody2D rb = other.attachedRigidbody;

        // 👉 คำนวณระยะ
        float distance = Vector2.Distance(transform.position, other.transform.position);

        if (distance > range) return;

        // 👉 แรงลดตามระยะ (ยิ่งใกล้ยิ่งแรง)
        float forcePercent = 1 - (distance / range);

        // 👉 สูตรแรง (มีการคำนวณ)
        Vector2 force = direction.normalized * maxForce * forcePercent;

        rb.AddForce(force);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * 2);
    }
}