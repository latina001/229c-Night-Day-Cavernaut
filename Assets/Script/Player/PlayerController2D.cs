using UnityEngine;
using System.Collections;

public class PlayerController2D : MonoBehaviour, IDamageable
{
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    AudioSource audioSource;

    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    int jumpCount;
    public int maxJump = 2;
    float lastGroundTime;
    float groundBufferTime = 0.1f;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashBackForce = 12f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;
    float lastDashTime;

    bool isDashing;
    bool isKnockback;
    bool isDead;

    [Header("Attack")]
    public AttackHitbox hitbox;

    [Header("Health (Hearts)")]
    public int maxHearts = 3;
    int currentHearts;

    [Header("Knockback (โดนตี)")]
    public float hitKnockbackX = 4f;   // 🟡 เด้งซ้าย/ขวา
    public float hitKnockbackY = 3f;   // 🟡 เด้งขึ้นนิดหน่อย
    public float knockbackTime = 0.2f;

    [Header("Invincibility (อมตะชั่วคราว)")]
    public float invincibleDuration = 1.5f;  // วินาทีที่อมตะ
    public float blinkInterval = 0.1f;       // กระพริบทุก X วินาที
    bool isInvincible;

    [Header("Death")]
    public float deathKnockback = 10f;
    public float fallGravity = 3f;

    [Header("Sound")]
    public AudioClip jumpSound;
    public AudioClip attackSound;
    public AudioClip dashSound;
    public AudioClip dashBackSound;
    public AudioClip dieSound;
    public AudioClip hurtSound;

    bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        currentHearts = maxHearts;

        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (isDead) return;
        if (isDashing) return;
        if (isKnockback) return;

        Move();
        Jump();
        Attack();
        Dash();

        UpdateAnimator();
    }

    // ================= MOVE =================
    void Move()
    {
        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        if (move > 0) sr.flipX = false;
        else if (move < 0) sr.flipX = true;
    }

    // ================= JUMP =================
    void Jump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (isGrounded)
            lastGroundTime = Time.time;

        if (Time.time - lastGroundTime < groundBufferTime)
            jumpCount = 0;

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;

            if (jumpSound) audioSource.PlayOneShot(jumpSound);
        }
    }

    // ================= ATTACK =================
    void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("ATK");
            if (attackSound) audioSource.PlayOneShot(attackSound);
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        hitbox.EnableHitbox();

        yield return new WaitForSeconds(0.2f);
        hitbox.DisableHitbox();
    }

    // ================= DASH =================
    void Dash()
    {
        if (Time.time < lastDashTime + dashCooldown) return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            lastDashTime = Time.time;
            if (dashSound) audioSource.PlayOneShot(dashSound);
            StartCoroutine(DoDash(false));
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            lastDashTime = Time.time;
            if (dashBackSound) audioSource.PlayOneShot(dashBackSound);
            StartCoroutine(DoDash(true));
        }
    }

    IEnumerator DoDash(bool back)
    {
        isDashing = true;

        float dir = sr.flipX ? -1 : 1;
        rb.gravityScale = 0;

        if (back)
        {
            dir *= -1;
            anim.SetTrigger("DashBack");
            rb.linearVelocity = new Vector2(dir * dashBackForce, 0);
        }
        else
        {
            anim.SetTrigger("Dash");
            rb.linearVelocity = new Vector2(dir * dashForce, 0);
        }

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = 3f;
        isDashing = false;
    }

    // ================= DAMAGE =================
    public void TakeDamage(float damage, Transform attacker)
    {
        if (isDead) return;
        if (isInvincible) return;   // 🛡️ กำลังอมตะ ไม่รับดาเมจ

        currentHearts--;

        // อัพเดต UI หัวใจ
        if (HeartUI.instance != null)
            HeartUI.instance.LoseHeart();

        if (hurtSound) audioSource.PlayOneShot(hurtSound);

        if (currentHearts <= 0)
        {
            StartCoroutine(DeathRoutine(attacker));
        }
        else
        {
            // 🟡 เด้งกลับเบาๆ + เปิดอมตะ
            Vector2 knockDir = (transform.position - attacker.position).normalized;
            StartCoroutine(HitRoutine(knockDir));
        }
    }

    // ================= HIT ROUTINE =================
    IEnumerator HitRoutine(Vector2 knockDir)
    {
        isKnockback = true;
        isInvincible = true;

        // เด้งซ้าย/ขวา + ขึ้นนิดหน่อย
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(knockDir.x * hitKnockbackX, hitKnockbackY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackTime);
        isKnockback = false;

        // 🌟 กระพริบตลอดช่วงอมตะ
        yield return StartCoroutine(BlinkRoutine(invincibleDuration));

        isInvincible = false;
        sr.enabled = true; // ให้แน่ใจว่า sprite ปรากฏ
    }

    IEnumerator BlinkRoutine(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }
        sr.enabled = true;
    }

    // ================= DEATH =================
    IEnumerator DeathRoutine(Transform attacker)
    {
        isDead = true;
        isInvincible = false;
        sr.enabled = true;

        Vector2 dir = (transform.position - attacker.position).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * deathKnockback, ForceMode2D.Impulse);

        rb.gravityScale = fallGravity;

        anim.Play("Die", 0, 0f);

        if (dieSound) audioSource.PlayOneShot(dieSound);

        yield return new WaitForSeconds(1.5f);

        GameManager.instance.PlayerDied();
    }

    // ================= ANIMATION =================
    void UpdateAnimator()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    // ================= RESPAWN =================
    public void Respawn()
    {
        isDead = false;
        isKnockback = false;
        isDashing = false;
        isInvincible = false;

        currentHearts = maxHearts;
        sr.enabled = true;

        // รีเซ็ต UI หัวใจ
        if (HeartUI.instance != null)
            HeartUI.instance.ResetHearts();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 3f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        anim.Rebind();
        anim.Update(0f);
        anim.Play("Idle");
    }
}