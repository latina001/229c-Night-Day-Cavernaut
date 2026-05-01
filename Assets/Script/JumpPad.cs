using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce = 15f;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = col.gameObject.GetComponent<Rigidbody2D>();

            // แรงกระแทกทันที (Impulse)
            Vector2 force = Vector2.up * jumpForce;

            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}