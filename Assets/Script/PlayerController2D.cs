using UnityEngine;
using System.Collections;

public class PlayerController2D : MonoBehaviour
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
    int jumpCount = 0;
    public int maxJump = 2;
    float groundBufferTime = 0.1f;
    float lastGroundTime;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashBackForce = 12f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f; // ‡«≈“§Ÿ≈¥“«πÏ
    float lastDashTime;
    public float normalGravity = 3f;

    [Header("Sound")]
    public AudioClip[] footstepSounds;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip attackSound;
    public AudioClip dashSound;
    public AudioClip dashBackSound;
    public AudioClip dieSound;
    public float stepDelay = 0.4f;

    float stepTimer;
    bool wasGrounded;

    bool isGrounded;
    bool isDead = false;
    bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        rb.gravityScale = normalGravity;
    }

    void Update()
    {
        if (isDead || isDashing) return;

        Move();
        Jump();
        Attack();
        Dash();
        UpdateAnimator();

        HandleFootstep();
        HandleLandingSound();
    }

    void Move()
    {
        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        if (move > 0)
            sr.flipX = false;
        else if (move < 0)
            sr.flipX = true;
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

            audioSource.PlayOneShot(jumpSound);
        }
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.ResetTrigger("ATK");
            anim.SetTrigger("ATK");

            audioSource.PlayOneShot(attackSound);
        }
    }

    void Dash()
    {
        // ‡™Á§§Ÿ≈¥“«πÏ
        if (Time.time < lastDashTime + dashCooldown) return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            lastDashTime = Time.time;
            StartCoroutine(DoDash(false));
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            lastDashTime = Time.time;
            StartCoroutine(DoDash(true));
        }
    }

    IEnumerator DoDash(bool isBack)
    {
        isDashing = true;

        float dir = sr.flipX ? -1 : 1;
        rb.gravityScale = 0;

        if (isBack)
        {
            dir *= -1;
            anim.SetTrigger("DashBack");
            rb.linearVelocity = new Vector2(dir * dashBackForce, 0);

            audioSource.PlayOneShot(dashBackSound);
        }
        else
        {
            anim.SetTrigger("Dash");
            rb.linearVelocity = new Vector2(dir * dashForce, 0);

            audioSource.PlayOneShot(dashSound);
        }

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = normalGravity;
        isDashing = false;
    }

    void UpdateAnimator()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    void HandleFootstep()
    {
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f && isGrounded)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                if (footstepSounds.Length > 0)
                {
                    audioSource.PlayOneShot(
                        footstepSounds[Random.Range(0, footstepSounds.Length)]
                    );
                }

                stepTimer = stepDelay;
            }
        }
        else
        {
            stepTimer = 0;
        }
    }

    void HandleLandingSound()
    {
        if (!wasGrounded && isGrounded)
        {
            audioSource.PlayOneShot(landSound);
        }

        wasGrounded = isGrounded;
    }

    public void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Die");

        audioSource.PlayOneShot(dieSound);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}