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

    [Header("Fade Settings")]
    [Tooltip("ระยะเวลา Fade ดำ (วิ) ตอนผู้เล่นตาย")]
    public float fadeOutDuration = 0.5f;

    [Tooltip("ระยะเวลา Fade กลับ (วิ) ตอน respawn")]
    public float fadeInDuration = 0.8f;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (player != null)
            respawnPoint = player.position;

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
        yield return StartCoroutine(Fade(1f, fadeOutDuration));   // ดำ

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

        yield return StartCoroutine(Fade(0f, fadeInDuration));    // กลับ
    }

    /// <summary>
    /// Fade ไปยัง alpha ที่กำหนด
    /// </summary>
    /// <param name="target">alpha ปลายทาง (0 = ใส, 1 = ดำ)</param>
    /// <param name="duration">ระยะเวลา (วิ)</param>
    public IEnumerator Fade(float target, float duration)
    {
        if (fadeImage == null) yield break;

        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;

        // ป้องกัน duration = 0 หาร 0
        if (duration <= 0f)
        {
            Color instant = fadeImage.color;
            instant.a = target;
            fadeImage.color = instant;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Color c = fadeImage.color;
            c.a = Mathf.Lerp(startAlpha, target, t);
            fadeImage.color = c;

            yield return null;
        }

        // บังคับตรงเป๊ะ
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