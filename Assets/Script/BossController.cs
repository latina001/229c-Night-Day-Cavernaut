using UnityEngine;
using System.Collections;

/// <summary>
/// บอสนกพิราบ — 3 ท่าโจมตี
///   1. WalkAndPeck  : เดินเข้าหาแล้วจิก (กำหนดระยะเดิน + ระยะจิกได้)
///   2. BreadToss    : ยิงขนมปังไปหาผู้เล่น
///   3. Dive         : บินพุ่งข้ามแผนที่ เลือกระดับ Low/Mid/High แบบสุ่ม
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class BossController : MonoBehaviour, IDamageable
{
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    AudioSource audioSource;

    [Header("References")]
    public Transform player;

    [Header("Boss HP Bar UI")][Tooltip("ลาก GameObject ที่มี BossHealthBar script ใส่")] public BossHealthBar bossHealthBar; [Header("Victory Screen")][Tooltip("ลาก GameObject ที่มี VictoryScreen script ใส่")] public VictoryScreen victoryScreen;

    // ──────────── Health ────────────
    [Header("Health")]
    public float maxHealth = 300f;
    float currentHealth;

    // ──────────── Map Boundary ────────────
    [Header("Map Boundary")]
    public float mapLeftX = -12f;
    public float mapRightX = 12f;

    // ──────────── Walk & Peck ────────────
    [Header("Attack 1 — Walk & Peck")]

    [Tooltip("ความเร็วเดิน")]
    public float walkSpeed = 2.5f;

    [Tooltip("ระยะสูงสุดที่บอสจะเริ่มเดินหาผู้เล่น\nถ้าผู้เล่นอยู่ไกลกว่านี้ บอสจะยืนจิกแทน")]
    public float walkStartRange = 8f;

    [Tooltip("ระยะที่บอสหยุดเดินแล้วจิก")]
    public float peckRange = 1.5f;

    [Tooltip("เวลา timeout การเดิน (วิ)")]
    public float walkTimeout = 4f;

    [Tooltip("ดาเมจท่าจิก (หน่วย = หัวใจ)")]
    public float peckDamage = 1f;

    // ──────────── Bread Toss ────────────
    [Header("Attack 2 — Bread Toss")]
    public GameObject breadPrefab;
    public Transform breadSpawnPoint;
    public int breadCount = 3;
    public float breadDelay = 0.3f;

    // ──────────── Dive ────────────
    [Header("Attack 3 — Dive (3 ระดับ)")]

    [Tooltip("ความเร็วพุ่ง")]
    public float diveSpeed = 18f;

    [Tooltip("จำนวนรอบบินข้ามแผนที่")]
    public int diveLaps = 2;

    [Tooltip("ความสูงระดับ Low (Y world position)")]
    public float diveLow = 1f;

    [Tooltip("ความสูงระดับ Mid")]
    public float diveMid = 4f;

    [Tooltip("ความสูงระดับ High")]
    public float diveHigh = 8f;

    [Tooltip("เวลาเลื่อนขึ้น/ลงสู่ระดับ (วิ)")]
    public float diveTransitionTime = 0.4f;

    // ──────────── Phase ────────────
    [Header("Phase")]
    public float actionCooldown = 2.5f;

    // ──────────── Sound ────────────
    [Header("Sound")]
    public AudioClip soundWalk;
    public AudioClip soundPeck;
    public AudioClip soundBreadToss;
    public AudioClip soundDive;
    public AudioClip soundHurt;
    public AudioClip soundDeath;

    // ──────────── Internal ────────────
    enum BossState { Idle, WalkAndPeck, BreadToss, Dive, Hurt, Dead }
    BossState state = BossState.Idle;
    int actionIndex;
    Vector3 spawnPosition; // จุดเริ่มต้นของบอส — ใช้กลับหลัง Dive

    // ─────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentHealth = maxHealth;
        spawnPosition = transform.position; // เก็บจุดเริ่มต้น

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // เริ่ม HP bar
        if (bossHealthBar != null)
            bossHealthBar.Init(maxHealth);

        StartCoroutine(BossLoop());
    }

    // ══════════════════════════════════════════
    //  Loop หลัก
    // ══════════════════════════════════════════
    IEnumerator BossLoop()
    {
        yield return new WaitForSeconds(1f);

        while (state != BossState.Dead)
        {
            yield return new WaitForSeconds(actionCooldown);
            if (state == BossState.Dead) yield break;

            yield return StartCoroutine(DoAction(actionIndex % 3));
            actionIndex++;
        }
    }

    IEnumerator DoAction(int index)
    {
        switch (index)
        {
            case 0: yield return StartCoroutine(Action_WalkAndPeck()); break;
            case 1: yield return StartCoroutine(Action_BreadToss()); break;
            case 2: yield return StartCoroutine(Action_Dive()); break;
        }
    }

    // ══════════════════════════════════════════
    //  ท่า 1 : Walk & Peck
    // ══════════════════════════════════════════
    IEnumerator Action_WalkAndPeck()
    {
        state = BossState.WalkAndPeck;
        if (player == null) { state = BossState.Idle; yield break; }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= walkStartRange)
        {
            // ── อยู่ในระยะ → เดินเข้าหา ──
            anim.Play("Walk");
            PlaySound(soundWalk);

            float timer = 0f;
            while (player != null &&
                   Vector2.Distance(transform.position, player.position) > peckRange)
            {
                if (state == BossState.Dead) yield break;
                timer += Time.deltaTime;
                if (timer >= walkTimeout) break;

                FacePlayer();
                Vector2 dir = (player.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(dir.x * walkSpeed, 0f);
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
        }
        // ถ้าไกลเกิน walkStartRange → ไม่เดิน แค่จิกตรงที่ยืน

        // ── จิก ──
        FacePlayer();
        anim.Play("Attack 1");
        PlaySound(soundPeck);

        yield return new WaitForSeconds(0.2f); // windup

        if (player != null &&
            Vector2.Distance(transform.position, player.position) <= peckRange + 0.6f)
        {
            IDamageable dmg = player.GetComponent<IDamageable>();
            dmg?.TakeDamage(peckDamage, transform);
        }

        yield return new WaitForSeconds(0.8f);
        anim.Play("Idel");
        state = BossState.Idle;
    }

    // ══════════════════════════════════════════
    //  ท่า 2 : Bread Toss
    // ══════════════════════════════════════════
    IEnumerator Action_BreadToss()
    {
        state = BossState.BreadToss;
        rb.linearVelocity = Vector2.zero;
        FacePlayer();

        anim.Play("Attack 2");
        PlaySound(soundBreadToss);

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < breadCount; i++)
        {
            if (breadPrefab != null && player != null)
            {
                Vector3 spawnPos = breadSpawnPoint != null
                    ? breadSpawnPoint.position
                    : transform.position;

                Vector2 dir = (player.position - spawnPos).normalized;
                float spread = (i - breadCount / 2f) * 0.15f;
                dir = Quaternion.Euler(0, 0, spread * 20f) * dir;

                GameObject bread = Instantiate(breadPrefab, spawnPos, Quaternion.identity);
                BreadProjectile proj = bread.GetComponent<BreadProjectile>();
                if (proj) proj.Init(dir);
            }

            yield return new WaitForSeconds(breadDelay);
        }

        yield return new WaitForSeconds(0.5f);
        anim.Play("Idel");
        state = BossState.Idle;
    }

    // ══════════════════════════════════════════
    //  ท่า 3 : Dive — สุ่ม 3 ระดับ Low/Mid/High
    // ══════════════════════════════════════════
    IEnumerator Action_Dive()
    {
        state = BossState.Dive;
        rb.linearVelocity = Vector2.zero;

        anim.Play("Attack 3");
        PlaySound(soundDive);

        // สุ่มระดับ
        float chosenY = PickDiveHeight();

        // เลื่อนขึ้น/ลงสู่ระดับ
        yield return StartCoroutine(MoveToY(chosenY, diveTransitionTime));

        // พุ่งข้ามแผนที่
        for (int lap = 0; lap < diveLaps * 2; lap++)
        {
            float targetX = (lap % 2 == 0) ? mapRightX : mapLeftX;
            yield return StartCoroutine(DiveAcrossMap(targetX, chosenY));
            yield return new WaitForSeconds(0.08f);
        }

        // ── กลับสู่จุดเริ่มต้นเดิม ──
        // 1) เลื่อน Y กลับก่อน
        yield return StartCoroutine(MoveToY(spawnPosition.y, diveTransitionTime));
        // 2) เลื่อน X กลับ
        yield return StartCoroutine(MoveToX(spawnPosition.x, diveTransitionTime));
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.5f);
        anim.Play("Idel");
        state = BossState.Idle;
    }

    /// <summary>สุ่ม Low / Mid / High แบบเท่าๆ กัน</summary>
    float PickDiveHeight()
    {
        switch (Random.Range(0, 3))
        {
            case 0: return diveLow;
            case 1: return diveMid;
            default: return diveHigh;
        }
    }

    IEnumerator DiveAcrossMap(float targetX, float lockY)
    {
        float dirX = Mathf.Sign(targetX - transform.position.x);
        sr.flipX = (dirX < 0);

        while (Mathf.Abs(transform.position.x - targetX) > 0.3f)
        {
            if (state == BossState.Dead) yield break;

            rb.linearVelocity = new Vector2(dirX * diveSpeed, 0f);

            // ล็อค Y ตลอดการพุ่ง (ป้องกัน drift)
            Vector3 pos = transform.position;
            pos.y = lockY;
            transform.position = pos;

            // ชนผู้เล่น
            if (player != null &&
                Vector2.Distance(transform.position, player.position) < 1.2f)
            {
                IDamageable dmg = player.GetComponent<IDamageable>();
                dmg?.TakeDamage(1f, transform);
            }

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
    }

    IEnumerator MoveToY(float targetY, float duration)
    {
        float elapsed = 0f;
        float startY = transform.position.y;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = new Vector3(
                transform.position.x,
                Mathf.Lerp(startY, targetY, t),
                transform.position.z);
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    IEnumerator MoveToX(float targetX, float duration)
    {
        float elapsed = 0f;
        float startX = transform.position.x;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = new Vector3(
                Mathf.Lerp(startX, targetX, t),
                transform.position.y,
                transform.position.z);

            FacePlayer(); // หันหน้าหาผู้เล่นระหว่างเดินกลับ
            yield return null;
        }

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }

    // ──────────── IDamageable ────────────
    public void TakeDamage(float damage, Transform attacker)
    {
        if (state == BossState.Dead) return;
        currentHealth -= damage;
        PlaySound(soundHurt);

        // อัพเดต HP bar
        if (bossHealthBar != null)
            bossHealthBar.SetHP(currentHealth, maxHealth);

        StopCoroutine(nameof(HurtFlash));
        StartCoroutine(HurtFlash());
        if (currentHealth <= 0) StartCoroutine(DieRoutine());
    }

    IEnumerator HurtFlash()
    {
        sr.color = Color.red;
        anim.Play("Hurt");
        yield return new WaitForSeconds(0.15f);
        sr.color = Color.white;
    }

    IEnumerator DieRoutine()
    {
        state = BossState.Dead;
        StopAllCoroutines();
        StartCoroutine(DieRoutineInternal());
        yield break;
    }

    IEnumerator DieRoutineInternal()
    {
        state = BossState.Dead;
        rb.linearVelocity = Vector2.zero;
        anim.Play("Death");
        PlaySound(soundDeath);

        yield return new WaitForSeconds(2f);

        // แสดงหน้าจบเกม
        if (victoryScreen != null)
            victoryScreen.Show();
        else if (VictoryScreen.instance != null)
            VictoryScreen.instance.Show();

        // รอสักครู่แล้วค่อย Destroy บอส
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    // ──────────── Helpers ────────────
    void FacePlayer()
    {
        if (player == null) return;
        sr.flipX = (player.position.x < transform.position.x);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip && audioSource) audioSource.PlayOneShot(clip);
    }

    // ──────────── Gizmos (เห็นใน Scene View) ────────────
    void OnDrawGizmosSelected()
    {
        // ระยะจิก
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, peckRange);

        // ระยะเดิน
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, walkStartRange);

        // เส้น Dive 3 ระดับ
        Color lineColor = new Color(0.2f, 1f, 0.5f, 0.7f);
        Gizmos.color = lineColor;
        Gizmos.DrawLine(new Vector3(mapLeftX, diveLow, 0), new Vector3(mapRightX, diveLow, 0));
        Gizmos.DrawLine(new Vector3(mapLeftX, diveMid, 0), new Vector3(mapRightX, diveMid, 0));
        Gizmos.DrawLine(new Vector3(mapLeftX, diveHigh, 0), new Vector3(mapRightX, diveHigh, 0));

#if UNITY_EDITOR
        UnityEditor.Handles.color = lineColor;
        UnityEditor.Handles.Label(new Vector3(mapRightX + 0.2f, diveLow, 0), "Dive Low");
        UnityEditor.Handles.Label(new Vector3(mapRightX + 0.2f, diveMid, 0), "Dive Mid");
        UnityEditor.Handles.Label(new Vector3(mapRightX + 0.2f, diveHigh, 0), "Dive High");
#endif
    }
}