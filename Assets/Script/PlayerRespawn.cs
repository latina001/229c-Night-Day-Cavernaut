using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    Vector3 startPos;

    void Start()
    {
        startPos = transform.position; // เก็บจุดเกิด
    }

    public void Respawn()
    {
        transform.position = startPos;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}