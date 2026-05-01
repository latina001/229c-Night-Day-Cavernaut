using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Player")]
    public Transform player;
    public Vector3 respawnPoint;

    [Header("Life")]
    public int maxLives = 3;
    int currentLives;

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeSpeed = 2f;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // หา Player อัตโนมัติ
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        currentLives = maxLives;

        if (player != null)
            respawnPoint = player.position;

        // หา Fade อัตโนมัติ
        GameObject fadeObj = GameObject.Find("Fade");
        if (fadeObj != null)
        {
            fadeCanvas = fadeObj.GetComponent<CanvasGroup>();
        }
    }

    public void PlayerDied()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        currentLives--;

        // 🌑 Fade ดำ
        yield return StartCoroutine(Fade(1));

        if (currentLives <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield break;
        }

        // 🔄 Respawn
        player.position = respawnPoint;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        player.GetComponent<PlayerController2D>().Respawn();

        // 🌑 Fade กลับ
        yield return StartCoroutine(Fade(0));
    }

    IEnumerator Fade(float target)
    {
        if (fadeCanvas == null) yield break;

        while (!Mathf.Approximately(fadeCanvas.alpha, target))
        {
            fadeCanvas.alpha = Mathf.MoveTowards(
                fadeCanvas.alpha,
                target,
                fadeSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    public void SetCheckpoint(Vector3 pos)
    {
        respawnPoint = pos;
    }
}