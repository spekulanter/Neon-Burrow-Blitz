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
    public int maxAirJumps = 1;
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
    int airJumpsRemaining;
    bool wasGrounded;

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
        EnforceReadableScale();
        UpdateGrounded();

        if (input.JumpPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (IsGrounded)
        {
            coyoteCounter = coyoteTime;
            if (!wasGrounded)
                airJumpsRemaining = maxAirJumps;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && !input.CrouchHeld)
        {
            if (coyoteCounter > 0f)
                Jump(false);
            else if (airJumpsRemaining > 0)
                Jump(true);
        }

        if (!input.JumpHeld && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.55f);

        UpdateCrouchCollider();
        UpdateAnimator();
        wasGrounded = IsGrounded;
    }

    void FixedUpdate()
    {
        EnforceReadableScale();
        float move = input.Move;
        float targetSpeed = move * moveSpeed * (input.CrouchHeld ? crouchSpeedMultiplier : 1f);
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float speed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

        if (Mathf.Abs(move) > 0.01f)
        {
            FacingDirection = move > 0f ? 1 : -1;
            var scale = transform.localScale;
            scale.x = FacingDirection;
            scale.y = 1f;
            scale.z = 1f;
            transform.localScale = scale;
        }
    }

    void EnforceReadableScale()
    {
        float x = FacingDirection >= 0 ? 1f : -1f;
        if (Mathf.Abs(transform.localScale.x - x) > 0.001f ||
            Mathf.Abs(transform.localScale.y - 1f) > 0.001f ||
            Mathf.Abs(transform.localScale.z - 1f) > 0.001f)
        {
            transform.localScale = new Vector3(x, 1f, 1f);
        }
    }

    public void ApplyKnockback(Vector2 force)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    void Jump(bool airJump)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        if (airJump)
            airJumpsRemaining--;
    }

    void UpdateGrounded()
    {
        Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down * 0.08f;
        IsGrounded = Physics2D.OverlapCircle(origin, 0.28f, groundMask) ||
            Physics2D.Raycast(origin + Vector2.up * 0.08f, Vector2.down, 0.45f, groundMask) ||
            (capsule != null && capsule.IsTouchingLayers(groundMask));
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
