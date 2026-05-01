using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    public static HeartUI instance;

    [Header("Heart Images (ลากใส่ตามลำดับ 0=ดวงที่1)")]
    public Image[] hearts;

    [Header("Sprites")]
    public Sprite heartFull;
    public Sprite heartEmpty;

    public int maxHearts = 3;
    int currentHearts;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentHearts = maxHearts;
        UpdateUI();
    }

    /// <summary>ลดหัวใจ 1 ดวง</summary>
    public void LoseHeart()
    {
        currentHearts = Mathf.Max(0, currentHearts - 1);
        UpdateUI();
    }

    /// <summary>รีเซ็ตหัวใจกลับเต็ม (ใช้ตอน respawn)</summary>
    public void ResetHearts()
    {
        currentHearts = maxHearts;
        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].sprite = (i < currentHearts) ? heartFull : heartEmpty;
        }
    }
}