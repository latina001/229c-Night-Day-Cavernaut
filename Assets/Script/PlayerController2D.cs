using UnityEngine;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashBackForce = 12f;
    public float dashTime = 0.2f;
    public float normalGravity = 3f;

    bool isGrounded;
    bool isDead = false;
    bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = normalGravity;
    }

    void Update()
    {
        if (isDead || isDashing) return;

        Move();
        Jump();
        Attack();
        Dash();
        UpdateAnimation();
    }

    void Move()
    {
        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        if (move > 0)
            sr.flipX = false;
        else if (move < 0)
            sr.flipX = true;

        anim.SetFloat("Speed", Mathf.Abs(move));
    }

    void Jump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("ATK");
        }
    }

    void Dash()
    {
        // Dash ไปข้างหน้า
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(DoDash(false));
        }

        // Dash ถอยหลัง
        if (Input.GetKeyDown(KeyCode.Q))
        {
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
        }
        else
        {
            anim.SetTrigger("Dash");
            rb.linearVelocity = new Vector2(dir * dashForce, 0);
        }

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = normalGravity;
        isDashing = false;
    }

    void UpdateAnimation()
    {
        float yVel = rb.linearVelocity.y;

        if (!isGrounded)
        {
            if (yVel > 0)
                anim.Play("Jump");
            else if (yVel < 0)
                anim.Play("Down Jump");
        }
        else
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                anim.Play("run");
            else
                anim.Play("Idle");
        }
    }

    public void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.Play("Die");
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