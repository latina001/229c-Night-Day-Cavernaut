using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 8f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Wall Check (2 sides)")]
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float wallRadius = 0.3f;
    public LayerMask wallLayer;
    public float wallSlideSpeed = 1.5f;

    [Header("Wall Jump")]
    public float wallJumpForceX = 6f;
    public float wallJumpForceY = 12f;
    public float wallJumpLockTime = 0.15f;

    private float moveInput;
    private bool isGrounded;
    private bool onWallLeft;
    private bool onWallRight;
    private bool isWallJumping;
    private float lockTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Flip
        if (moveInput != 0)
            sr.flipX = moveInput < 0;

        // Ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Wall (2 ด้าน)
        onWallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, wallRadius, wallLayer);
        onWallRight = Physics2D.OverlapCircle(wallCheckRight.position, wallRadius, wallLayer);

        bool isOnWall = (onWallLeft || onWallRight) && !isGrounded;

        // 🧗 Slide
        if (isOnWall && moveInput != 0 && !isWallJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }

        // ⬆️ Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (onWallLeft)
            {
                WallJump(1); // เด้งไปขวา
            }
            else if (onWallRight)
            {
                WallJump(-1); // เด้งไปซ้าย
            }
        }

        // ⏱ lock control ชั่วคราว
        if (isWallJumping)
        {
            lockTimer -= Time.deltaTime;
            if (lockTimer <= 0)
                isWallJumping = false;
        }

        // Animation
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isOnWall", isOnWall);
        anim.SetBool("isWalking", Mathf.Abs(moveInput) > 0.01f);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    void WallJump(int direction)
    {
        isWallJumping = true;
        lockTimer = wallJumpLockTime;

        // 🔥 รีเซ็ตแรงตกก่อน
        rb.linearVelocity = new Vector2(0, 0);

        // 🔥 เด้งออก + ขึ้น
        rb.linearVelocity = new Vector2(
            direction * wallJumpForceX,
            wallJumpForceY
        );
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
        if (wallCheckLeft)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheckLeft.position, wallRadius);
        }
        if (wallCheckRight)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(wallCheckRight.position, wallRadius);
        }
    }
}