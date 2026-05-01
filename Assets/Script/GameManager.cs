using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Transform player;
    public Vector3 respawnPoint;

    public Image fadeImage;
    public float fadeSpeed = 2f;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 🔥 หา Player อัตโนมัติ
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        // 🔥 ตั้งจุดเกิดเริ่มต้น
        if (player != null)
            respawnPoint = player.position;

        // 🔥 ตั้งค่า fade เริ่ม
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    public void PlayerDied()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        Debug.Log("🔥 เริ่ม Fade");

        yield return StartCoroutine(Fade(1f)); // 🌑 ดำ

        if (player != null)
        {
            player.position = respawnPoint;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            player.GetComponent<PlayerController2D>().Respawn();
        }

        yield return StartCoroutine(Fade(0f)); // 🌕 กลับ
    }

    IEnumerator Fade(float target)
    {
        if (fadeImage == null)
        {
           
            yield break;
        }

        while (!Mathf.Approximately(fadeImage.color.a, target))
        {
            float newAlpha = Mathf.MoveTowards(
                fadeImage.color.a,
                target,
                fadeSpeed * Time.deltaTime
            );

            Color c = fadeImage.color;
            c.a = newAlpha;
            fadeImage.color = c;

            yield return null;
        }

        // 🔥 บังคับให้ตรงเป๊ะ
        Color final = fadeImage.color;
        final.a = target;
        fadeImage.color = final;
    }

    // 📍 Checkpoint
    public void SetCheckpoint(Vector3 pos)
    {
        respawnPoint = pos;
       
    }
}