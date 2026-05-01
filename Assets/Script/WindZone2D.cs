using UnityEngine;

public class WindZone2D : MonoBehaviour
{
    public Vector2 windDirection = new Vector2(0, 1); // ทิศลม
    public float forcePower = 20f; // แรงลม

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

            // F = ma → เราใส่ Force เข้าไป
            Vector2 force = windDirection.normalized * forcePower;

            rb.AddForce(force, ForceMode2D.Force);
        }
    }
}