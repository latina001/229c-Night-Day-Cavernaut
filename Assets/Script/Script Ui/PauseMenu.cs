using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    bool isPaused = false;

    void Start()
    {
        //  เริ่มเกม = เมาส์ล็อก
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        // กด ESC เพื่อ Pause
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        pausePanel.SetActive(isPaused);

        //  หยุดเวลา
        Time.timeScale = isPaused ? 0f : 1f;

        //  เมาส์
        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // ▶ ปุ่ม Resume
    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;

        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void StartGame()
    {
        SceneManager.LoadScene("MainMuenu");
    }
    public void ReGame()
    {
        SceneManager.LoadScene("up");
    }
}