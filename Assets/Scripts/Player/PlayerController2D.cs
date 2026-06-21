using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    public PlayerInputConfig input = new PlayerInputConfig();
    public float moveSpeed = 10f;
    public float jumpForce = 15f;
    public float acceleration = 60f;
    public float deceleration = 70f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;
    public float crouchSpeedMultiplier = 0.45f;
    public Transform groundCheck;
    public LayerMask groundMask;

    public int FacingDirection { get; private set; } = 1;
    public bool IsGrounded { get; private set; }

    Rigidbody2D rb;
    Animator animator;
    CapsuleCollider2D capsule;
    Vector2 defaultColliderSize;
    Vector2 defaultColliderOffset;
    float coyoteCounter;
    float jumpBufferCounter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider2D>();
        if (capsule != null)
        {
            defaultColliderSize = capsule.size;
            defaultColliderOffset = capsule.offset;
        }
    }

    void Update()
    {
        UpdateGrounded();

        if (input.JumpPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (IsGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteCounter > 0f && !input.CrouchHeld)
            Jump();

        if (!input.JumpHeld && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.55f);

        UpdateCrouchCollider();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        float move = input.Move;
        float targetSpeed = move * moveSpeed * (input.CrouchHeld ? crouchSpeedMultiplier : 1f);
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float speed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

        if (Mathf.Abs(move) > 0.01f)
        {
            FacingDirection = move > 0f ? 1 : -1;
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * FacingDirection;
            transform.localScale = scale;
        }
    }

    public void ApplyKnockback(Vector2 force)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
    }

    void UpdateGrounded()
    {
        Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down * 0.08f;
        IsGrounded = Physics2D.OverlapCircle(origin, 0.28f, groundMask) ||
            Physics2D.Raycast(origin + Vector2.up * 0.08f, Vector2.down, 0.45f, groundMask);
    }

    void UpdateCrouchCollider()
    {
        if (capsule == null)
            return;

        if (input.CrouchHeld && IsGrounded)
        {
            capsule.size = new Vector2(defaultColliderSize.x, defaultColliderSize.y * 0.6f);
            capsule.offset = defaultColliderOffset + Vector2.down * (defaultColliderSize.y * 0.2f);
        }
        else
        {
            capsule.size = defaultColliderSize;
            capsule.offset = defaultColliderOffset;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsGrounded", IsGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        animator.SetBool("IsCrouching", input.CrouchHeld && IsGrounded);
    }
}
