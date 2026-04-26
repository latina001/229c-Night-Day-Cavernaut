using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (!other.CompareTag("Player")) return;

        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();

        if (respawn != null)
        {
            respawn.SetCheckpoint(transform.position);
            isActivated = true;

            Debug.Log("Checkpoint Activated!");
        }
    }
}