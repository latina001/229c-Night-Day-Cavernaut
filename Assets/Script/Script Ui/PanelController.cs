using UnityEngine;

public class PanelController : MonoBehaviour
{
    // ลาก Panel ที่ต้องการควบคุมมาใส่ใน Inspector
    public GameObject myPanel;

    // ฟังก์ชันสำหรับสั่งปิดหน้าจอ
    public void ClosePanel()
    {
        if (myPanel != null)
        {
            myPanel.SetActive(false); // สั่งปิด Panel
        }
    }
}