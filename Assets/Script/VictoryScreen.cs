using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// หน้าจบเกม — แสดงเมื่อบอสตาย
///
/// Hierarchy ใน Canvas:
///   [VictoryScreen]  ← GameObject นี้ + Script นี้ (เริ่มต้น SetActive false)
///     ├─ Overlay          (Image สีดำ alpha=0 เต็มหน้าจอ)
///     ├─ Panel            (Image กลางหน้าจอ)
///     │    ├─ TitleText       (TextMeshProUGUI — "STAGE CLEAR")
///     │    ├─ SubText         (TextMeshProUGUI — ข้อความเสริม)
///     │    ├─ BtnNextScene    (Button — ไปซีนถัดไป)
///     │    │    └─ Text "NEXT STAGE"
///     │    └─ BtnMainMenu     (Button — กลับ Main Menu)
///     │         └─ Text "MAIN MENU"
///     └─ FeatherParticles     (ParticleSystem ขนนก — optional)
/// </summary>
public class VictoryScreen : MonoBehaviour
{
    public static VictoryScreen instance;

    [Header("UI Elements")]
    public Image overlay;          // พื้นหลังดำโปร่งใส
    public RectTransform panel;      // กรอบกลาง
    public Text titleText;        // "STAGE CLEAR"
    public Text subText;          // ข้อความเสริม

    [Header("Buttons")]
    public Button btnNextScene;
    public Button btnMainMenu;

    [Header("Scene Names")]
    [Tooltip("ชื่อ Scene ที่จะไปหลังกด Next Stage")]
    public string nextSceneName = "NextLevel";

    [Tooltip("ชื่อ Scene Main Menu")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Animation")]
    public float overlayFadeTime = 0.6f;   // เวลา fade ฉากดำ
    public float panelSlideTime = 0.5f;   // เวลา panel เลื่อนขึ้น
    public float textRevealDelay = 0.3f;   // หน่วงก่อนตัวหนังสือโผล่

    [Header("Particles (optional)")]
    public ParticleSystem featherParticles;

    // ──────────── Internal ────────────
    bool isShowing;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    void Start()
    {
        // ผูก Button
        if (btnNextScene != null)
            btnNextScene.onClick.AddListener(GoNextScene);
        if (btnMainMenu != null)
            btnMainMenu.onClick.AddListener(GoMainMenu);
    }

    // ──────────── เรียกจาก BossController ────────────
    public void Show()
    {
        if (isShowing) return;
        isShowing = true;

        gameObject.SetActive(true);
        Time.timeScale = 1f; // ให้แน่ใจว่าเวลาไม่หยุด

        StartCoroutine(PlayVictorySequence());
    }

    // ──────────── ลำดับ Animation ────────────
    IEnumerator PlayVictorySequence()
    {
        // 0) ซ่อนทุกอย่างก่อน
        SetPanelAlpha(0f);
        SetTextAlpha(titleText, 0f);
        SetTextAlpha(subText, 0f);
        SetButtonAlpha(btnNextScene, 0f);
        SetButtonAlpha(btnMainMenu, 0f);

        if (overlay != null)
        {
            Color c = overlay.color;
            c.a = 0f;
            overlay.color = c;
        }

        if (panel != null)
        {
            Vector2 pos = panel.anchoredPosition;
            panel.anchoredPosition = pos + new Vector2(0, -80f);
        }

        // 1) Fade overlay ดำ
        yield return StartCoroutine(FadeOverlay(0f, 0.55f, overlayFadeTime));

        // 2) ปล่อย particle ขนนก
        if (featherParticles != null)
            featherParticles.Play();

        // 3) Slide panel ขึ้น + fade in
        yield return StartCoroutine(SlidePanel());

        yield return new WaitForSeconds(textRevealDelay);

        // 4) ตัวอักษร title โผล่
        yield return StartCoroutine(FadeText(titleText, 1f, 0.4f));

        yield return new WaitForSeconds(0.15f);
        yield return StartCoroutine(FadeText(subText, 1f, 0.3f));

        yield return new WaitForSeconds(0.2f);

        // 5) ปุ่มโผล่
        yield return StartCoroutine(FadeButton(btnNextScene, 1f, 0.25f));
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(FadeButton(btnMainMenu, 1f, 0.25f));
    }

    // ──────────── Coroutine Helpers ────────────
    IEnumerator FadeOverlay(float from, float to, float duration)
    {
        if (overlay == null) yield break;

        float elapsed = 0f;
        Color c = overlay.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            overlay.color = c;
            yield return null;
        }

        c.a = to;
        overlay.color = c;
    }

    IEnumerator SlidePanel()
    {
        if (panel == null) yield break;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        Vector2 startPos = panel.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, 80f);

        float elapsed = 0f;
        while (elapsed < panelSlideTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / panelSlideTime);
            panel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            cg.alpha = t;
            yield return null;
        }

        panel.anchoredPosition = endPos;
        cg.alpha = 1f;
    }

    IEnumerator FadeText(Text txt, float to, float duration)
    {
        if (txt == null) yield break;

        float elapsed = 0f;
        Color c = txt.color;
        float from = c.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            txt.color = c;
            yield return null;
        }
        c.a = to;
        txt.color = c;
    }

    IEnumerator FadeButton(Button btn, float to, float duration)
    {
        if (btn == null) yield break;

        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        float from = cg.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    // ──────────── Alpha Shortcuts ────────────
    void SetPanelAlpha(float a)
    {
        if (panel == null) return;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = a;
        cg.interactable = a > 0f;
        cg.blocksRaycasts = a > 0f;
    }

    void SetTextAlpha(Text txt, float a)
    {
        if (txt == null) return;
        Color c = txt.color;
        c.a = a;
        txt.color = c;
    }

    void SetButtonAlpha(Button btn, float a)
    {
        if (btn == null) return;
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = a;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    // ──────────── Button Actions ────────────
    public void GoNextScene()
    {
        StartCoroutine(LoadScene(nextSceneName));
    }

    public void GoMainMenu()
    {
        StartCoroutine(LoadScene(mainMenuSceneName));
    }

    IEnumerator LoadScene(string sceneName)
    {
        // Fade ดำออกก่อนเปลี่ยน Scene
        yield return StartCoroutine(FadeOverlay(overlay != null ? overlay.color.a : 0f, 1f, 0.4f));
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}