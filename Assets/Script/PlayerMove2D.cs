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

    [Header("Double Jump")]
    public int maxJumpCount = 2;
    public float doubleJumpForce = 7f;
    private int jumpCount;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float wallRadius = 0.3f;
    public LayerMask wallLayer;
    public float wallSlideSpeed = 1.5f;

    [Header("Wall Jump")]
    public float wallJumpForceX = 7f;
    public float wallJumpForceY = 12f;
    public float wallJumpLockTime = 0.2f;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashTime = 0.2f;
    public float dashCooldown = 0.5f;

    [Header("Physics")]
    public float defaultGravity = 3f;

    private float moveInput;
    private bool isGrounded;
    private bool onWallLeft;
    private bool onWallRight;

    private bool isWallJumping;
    private float wallJumpTimer;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // รีเซ็ต jump เมื่อแตะพื้น
        if (isGrounded)
        {
            jumpCount = 0;
        }

        // Wall check
        onWallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, wallRadius, wallLayer);
        onWallRight = Physics2D.OverlapCircle(wallCheckRight.position, wallRadius, wallLayer);

        bool isOnWall = (onWallLeft || onWallRight) && !isGrounded;

        // Flip
        HandleFlip(isOnWall);

        // Wall Slide
        if (isOnWall && !isWallJumping && !isDashing)
        {
            rb.gravityScale = defaultGravity;

            if (rb.linearVelocity.y < -wallSlideSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
        }
        else if (!isDashing)
        {
            rb.gravityScale = defaultGravity;
        }

        // Jump + Double Jump + Wall Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Wall Jump (สำคัญ: ให้มาก่อน)
            if (!isGrounded)
            {
                if (onWallLeft)
                {
                    WallJump(1);
                    jumpCount = 1;
                    return;
                }
                else if (onWallRight)
                {
                    WallJump(-1);
                    jumpCount = 1;
                    return;
                }
            }

            // Jump ปกติ + Double Jump
            if (jumpCount < maxJumpCount)
            {
                if (jumpCount == 0)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                else
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);

                jumpCount++;
            }
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0 && !isDashing)
        {
            StartDash();
        }

        // Dash timer
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                StopDash();
        }

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        // WallJump lock
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
        if (isDashing) return;

        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    void HandleFlip(bool isOnWall)
    {
        if (isOnWall)
        {
            if (onWallLeft) sr.flipX = false;
            if (onWallRight) sr.flipX = true;
        }
        else if (!isDashing)
        {
            if (moveInput != 0)
                sr.flipX = moveInput < 0;
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

        sr.flipX = direction < 0;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;

        rb.gravityScale = 0f;

        float direction = sr.flipX ? -1 : 1;
        rb.linearVelocity = new Vector2(direction * dashForce, 0);
    }

    void StopDash()
    {
        isDashing = false;
        rb.gravityScale = defaultGravity;
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