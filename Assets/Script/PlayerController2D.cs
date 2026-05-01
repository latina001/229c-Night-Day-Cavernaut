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
    bool isDashing = false;

    [Header("Attack")]
    public AttackHitbox hitbox;

    [Header("Health")]
    public float maxHealth = 100f;
    float currentHealth;

    [Header("Death")]
    public float fallGravity = 3f;
    public float deathPush = 6f;

    [Header("Sound")]
    public AudioClip jumpSound;
    public AudioClip attackSound;
    public AudioClip dashSound;
    public AudioClip dashBackSound;
    public AudioClip dieSound;

    bool isGrounded;
    bool isDead;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        currentHealth = maxHealth;

        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (isDead) return;
        if (isDashing) return;

        Move();
        Jump();
        Attack();
        Dash();
        UpdateAnimator();
    }

    void Move()
    {
        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        if (move > 0) sr.flipX = false;
        else if (move < 0) sr.flipX = true;
    }

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

    void UpdateAnimator()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    // ================= DAMAGE =================
    public void TakeDamage(float damage, Transform attacker)
    {
        if (isDead) return;

        currentHealth -= damage;

        Vector2 dir = (transform.position - attacker.position).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * 6f, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            StartCoroutine(DeathRoutine(attacker));
        }
    }

    IEnumerator DeathRoutine(Transform attacker)
    {
        isDead = true;

        Vector2 dir = (transform.position - attacker.position).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * deathPush, ForceMode2D.Impulse);

        rb.gravityScale = fallGravity;

        anim.Play("Die", 0, 0f);

        if (dieSound) audioSource.PlayOneShot(dieSound);

        yield return new WaitForSeconds(1.5f);

        GameManager.instance.PlayerDied();
    }

    public void Respawn()
    {
        isDead = false;

        currentHealth = maxHealth;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 3f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        anim.Rebind();
        anim.Update(0f);
        anim.Play("Idle");
    }
}