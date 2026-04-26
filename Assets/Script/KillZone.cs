using UnityEngine;
using System.Collections;

public class KillZone : MonoBehaviour
{
    public float respawnDelay = 1f;
    private bool isDead = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (!other.CompareTag("Player")) return;

        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();

        if (respawn != null)
        {
            isDead = true;
            StartCoroutine(HandleDeath(other.gameObject, respawn));
        }
    }

    IEnumerator HandleDeath(GameObject player, PlayerRespawn respawn)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        // 👉 รอแป๊บนึงก่อน fade
        yield return new WaitForSeconds(respawnDelay);

        // 👉 Fade + Respawn
        yield return StartCoroutine(
            FadeController.Instance.FadeOutIn(() =>
            {
                respawn.Respawn();
            })
        );

        if (rb != null)
        {
            rb.gravityScale = 3f; // หรือค่าเดิมของคุณ
        }

        isDead = false;
    }
}