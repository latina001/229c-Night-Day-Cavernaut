using UnityEngine;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialogueUI;
    public TMP_Text dialogueText;

    [Header("Dialogue Lines")]
    [TextArea]
    public string[] lines;

    int index = 0;
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
            ShowDialogue();
        }
    }

    void ShowDialogue()
    {
        if (lines.Length == 0) return;

        // เปิด UI ถ้ายังไม่เปิด
        if (!dialogueUI.activeSelf)
        {
            dialogueUI.SetActive(true);
            index = 0;
        }

        // แสดงข้อความปัจจุบัน
        dialogueText.text = lines[index];

        // เลื่อนไปข้อความถัดไป
        index++;

        // วนกลับถ้าหมด
        if (index >= lines.Length)
        {
            index = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (dialogueUI != null)
                dialogueUI.SetActive(false);

            index = 0; // รีเซ็ตบทสนทนา
        }
    }
}