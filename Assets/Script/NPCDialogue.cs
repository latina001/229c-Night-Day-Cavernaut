using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public GameObject dialogueUI;

    bool playerInRange;

    void Start()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDialogue();
        }
    }

    void ToggleDialogue()
    {
        if (dialogueUI == null) return;

        bool isActive = dialogueUI.activeSelf;
        dialogueUI.SetActive(!isActive);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("💬 Press E to talk");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (dialogueUI != null)
                dialogueUI.SetActive(false);
        }
    }
}