using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI แท่ง HP บอส
/// 
/// Setup ใน Canvas:
///   [Canvas]
///     └─ [BossHPBar]  ← ใส่ Script นี้
///          ├─ Background   (Image — สีเข้ม)
///          ├─ DelayedBar   (Image — สีแดงอ่อน, Filled)  ← bar ที่ค่อยๆ หาย
///          ├─ HPBar        (Image — สีหลัก, Filled)      ← bar หลัก
///          ├─ BorderLeft   (Image — เส้นกรอบซ้าย)
///          ├─ BorderRight  (Image — เส้นกรอบขวา)
///          └─ BossNameText (TextMeshProUGUI หรือ Text)
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar instance;

    [Header("Bars")]
    [Tooltip("Image แท่ง HP หลัก (Image Type = Filled, Fill Method = Horizontal)")]
    public Image hpBar;

    [Tooltip("Image แท่ง delay สีแดงอ่อน (อยู่ข้างหลัง hpBar)")]
    public Image delayedBar;

    [Header("Settings")]
    [Tooltip("ความเร็วที่ delayedBar ไล่ตามลง")]
    public float delaySpeed = 0.8f;

    [Tooltip("หน่วงกี่วิก่อน delayedBar เริ่มลด")]
    public float delayTime = 0.4f;

    [Tooltip("ความเร็ว pulse เมื่อ HP ต่ำ")]
    public float pulseSpeed = 2.5f;

    [Header("Colors")]
    public Color colorHigh   = new Color(0.18f, 0.85f, 0.35f);  // เขียว
    public Color colorMid    = new Color(0.95f, 0.75f, 0.10f);  // เหลือง
    public Color colorLow    = new Color(0.90f, 0.20f, 0.15f);  // แดง
    public Color colorDelay  = new Color(0.85f, 0.25f, 0.25f, 0.6f);

    // ──────────── Internal ────────────
    float maxHP;
    float currentHP;
    float targetFill;       // fill ที่ต้องการ (HP จริง)
    float delayedFill;      // fill ของ delayedBar (ตามช้า)
    float delayTimer;

    bool isInitialized;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false); // ซ่อนไว้ก่อน ให้บอสเรียก Init
    }

    void Update()
    {
        if (!isInitialized) return;

        // ── อัพเดต delayedBar ──
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
        }
        else if (delayedFill > targetFill)
        {
            delayedFill = Mathf.MoveTowards(delayedFill, targetFill, delaySpeed * Time.deltaTime);
            if (delayedBar != null)
                delayedBar.fillAmount = delayedFill;
        }

        // ── pulse เมื่อ HP < 25% ──
        if (currentHP / maxHP < 0.25f && hpBar != null)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            hpBar.color = Color.Lerp(colorLow, Color.white, pulse * 0.25f);
        }
    }

    // ─────────────────────────────────────────
    /// <summary>บอสเรียกตอน Start เพื่อเริ่มต้น HP bar</summary>
    public void Init(float max)
    {
        maxHP          = max;
        currentHP      = max;
        targetFill     = 1f;
        delayedFill    = 1f;
        isInitialized  = true;

        if (hpBar != null)    { hpBar.fillAmount    = 1f; hpBar.color = colorHigh; }
        if (delayedBar != null) { delayedBar.fillAmount = 1f; delayedBar.color = colorDelay; }

        gameObject.SetActive(true);
        StartCoroutine(SlideIn());
    }

    /// <summary>บอสเรียกตอน TakeDamage</summary>
    public void SetHP(float current, float max)
    {
        maxHP     = max;
        currentHP = current;
        float ratio = Mathf.Clamp01(current / max);

        targetFill = ratio;
        delayTimer = delayTime; // หน่วงก่อน delayedBar ไล่ตาม

        // อัพเดต hpBar ทันที
        if (hpBar != null)
        {
            hpBar.fillAmount = ratio;
            hpBar.color = GetBarColor(ratio);
        }

        // Shake เล็กน้อยเมื่อโดนตี
        StopCoroutine(nameof(ShakeBar));
        StartCoroutine(ShakeBar());

        if (current <= 0)
            StartCoroutine(HideAfterDelay(2f));
    }

    // ──────────── Color by HP ratio ────────────
    Color GetBarColor(float ratio)
    {
        if (ratio > 0.5f)
            return Color.Lerp(colorMid, colorHigh, (ratio - 0.5f) * 2f);
        else
            return Color.Lerp(colorLow, colorMid, ratio * 2f);
    }

    // ──────────── Effects ────────────
    IEnumerator SlideIn()
    {
        // Slide จากล่างขึ้นมา
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) yield break;

        Vector2 endPos = rt.anchoredPosition;
        Vector2 startPos = endPos + new Vector2(0, -60f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        rt.anchoredPosition = endPos;
    }

    IEnumerator ShakeBar()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) yield break;

        Vector2 origin = rt.anchoredPosition;
        float elapsed  = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.Sin(elapsed * 60f) * 4f * (1f - elapsed / duration);
            rt.anchoredPosition = origin + new Vector2(x, 0);
            yield return null;
        }

        rt.anchoredPosition = origin;
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Fade out
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * 1.5f;
            cg.alpha = t;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
