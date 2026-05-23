using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 9f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    private float horizontalInput;
    private bool jumpPressed;
    private bool isRunning;
    private bool _inputEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    public void DisableInput()
    {
        _inputEnabled = false;
        horizontalInput = 0f;
        jumpPressed = false;
    }

    public void EnableInput()
    {
        _inputEnabled = true;
    }

    public void Die()
    {
        DisableInput();
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();
    }

    private void Update()
    {
        if (!_inputEnabled) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
            jumpPressed = true;

        FlipSprite();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyJump();
        ApplyGravityModifiers();
    }

    private void ApplyMovement()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }

    private void ApplyJump()
    {
        if (!jumpPressed) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpPressed = false;
    }

    private void ApplyGravityModifiers()
    {
        // Faster fall when descending
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        // Shorter jump when button released early
        else if (rb.linearVelocity.y > 0f && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private void FlipSprite()
    {
        if (horizontalInput > 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (horizontalInput < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
