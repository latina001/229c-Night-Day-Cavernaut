using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panel;        // เมนูหลัก
    public GameObject creditsPanel; // เครดิต

    void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panel != null)
            panel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    // 🔹 เมนูหลัก
    public void ShowPanel()
    {
        panel.SetActive(true);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }

    // 🔹 Credits
    public void ShowCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);

        if (panel != null)
            panel.SetActive(false);
    }

    public void HideCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    // 🔹 เริ่มเกม
    public void StartGame()
    {
        SceneManager.LoadScene("Level1");
    }

    public void BackGame()
    {
        SceneManager.LoadScene("Main Menu");
    }

    // 🔹 ออกจากเกม
    public void QuitGame()
    {
        Application.Quit();
    }
}