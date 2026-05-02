using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject panel;

    void Start()
    {
        // 🔥 สำคัญมาก
        Time.timeScale = 1f;

        // 🖱️ ให้ใช้เมาส์ได้
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowPanel()
    {
        panel.SetActive(true);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("up");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}