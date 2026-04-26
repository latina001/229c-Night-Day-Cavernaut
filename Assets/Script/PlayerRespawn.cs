using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 respawnPoint;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        respawnPoint = transform.position; // จุดเริ่มต้น
    }

    public void SetCheckpoint(Vector3 newPoint)
    {
        respawnPoint = newPoint;
    }

    public void Respawn()
    {
        // รีเซ็ตความเร็ว (สำคัญมาก)
        rb.linearVelocity = Vector2.zero;

        // วาร์ปกลับ
        transform.position = respawnPoint;
    }
}