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

    [Header("Wall Check")]
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float wallRadius = 0.3f;
    public LayerMask wallLayer;
    public float wallSlideSpeed = 1.5f; // 🔥 เพิ่มตัวนี้

    [Header("Wall Jump")]
    public float wallJumpForceX = 7f;
    public float wallJumpForceY = 12f;
    public float wallJumpLockTime = 0.2f;

    [Header("Physics")]
    public float defaultGravity = 3f;

    private float moveInput;
    private bool isGrounded;
    private bool onWallLeft;
    private bool onWallRight;

    private bool isWallJumping;
    private float wallJumpTimer;

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

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Wall check
        onWallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, wallRadius, wallLayer);
        onWallRight = Physics2D.OverlapCircle(wallCheckRight.position, wallRadius, wallLayer);

        bool isOnWall = (onWallLeft || onWallRight) && !isGrounded;

        // 🧗 Wall Slide (ไถลลงช้า ๆ)
        if (isOnWall && !isWallJumping)
        {
            rb.gravityScale = defaultGravity;

            if (rb.linearVelocity.y < -wallSlideSpeed)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    -wallSlideSpeed
                );
            }
        }
        else
        {
            rb.gravityScale = defaultGravity;
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
                WallJump(1);
            }
            else if (onWallRight)
            {
                WallJump(-1);
            }
        }

        // ⏱ lock
        if (isWallJumping)
        {
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0)
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
        wallJumpTimer = wallJumpLockTime;

        rb.gravityScale = defaultGravity;

        rb.linearVelocity = Vector2.zero;

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