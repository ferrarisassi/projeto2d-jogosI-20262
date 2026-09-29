using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Coin Settings")]
    [SerializeField] private TextMeshProUGUI coinText;
    private int coinCounter = 0;

    [Header("Death Settings")]
    [SerializeField] private GameObject bloodParticlePrefab;
    [SerializeField] private float deathDelay = 0f; // Set > 0 if you want a death animation to play first


    // Input Actions - using your PlayerActionMap asset
    private PlayerActionMap inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;

    // Movement state
    private Vector2 moveInput;
    private bool isJumping;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isDead = false;

    private void Awake()
    {
        // Get or add Rigidbody2D
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Get Animator if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();

        // Get SpriteRenderer if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize input actions using YOUR PlayerActionMap asset
        inputActions = new PlayerActionMap();

        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
    }

    private void OnEnable()
    {
        // Enable input actions
        moveAction.Enable();
        jumpAction.Enable();

        // Subscribe to input events
        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;

        // Disable input actions
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        // Skip input handling if dead
        if (isDead) return;

        // Read movement input from YOUR configured Move action
        moveInput = moveAction.ReadValue<Vector2>();

        // Check if grounded
        CheckGrounded();

        // Apply jump physics
        ApplyJumpPhysics();

        // Update animation state
        UpdateAnimation();

        // Update facing direction
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        // Stop applying movement if dead
        if (isDead) return;

        // Apply movement
        MovePlayer();
    }

    private void CheckGrounded()
    {
        if (groundCheckPoint != null)
        {
            // Ground check using circle cast at ground check point
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                groundCheckPoint.position,
                groundCheckRadius,
                groundLayer
            );
            isGrounded = colliders.Length > 0;
        }
        else
        {
            // Simple ground check using raycast from center
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                1.1f,
                groundLayer
            );
            isGrounded = hit.collider != null;
        }
    }

    private void MovePlayer()
    {
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void ApplyJumpPhysics()
    {
        // Better jump physics (variable jump height)
        if (rb.linearVelocity.y < 0)
        {
            // Falling - increase gravity
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isJumping)
        {
            // Jump button released early - reduce jump height
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            // Set IsOnGround parameter - drives the jump transition
            animator.SetBool("IsOnGround", isGrounded);

            // Set IsWalking only when grounded (so jump anim takes priority via AnyState)
            bool isWalking = Mathf.Abs(moveInput.x) > 0.01f;
            animator.SetBool("IsWalking", isWalking);
        }
    }

    private void UpdateFacingDirection()
    {
        // Only update facing when there's actual horizontal input
        if (moveInput.x > 0.01f && !facingRight)
        {
            Flip();
        }
        else if (moveInput.x < -0.01f && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        // Use localScale if you prefer this method instead of flipX:
        // Vector3 scale = transform.localScale;
        // scale.x *= -1;
        // transform.localScale = scale;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isDead) return;

        if (isGrounded)
        {
            // Apply jump force
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isJumping = false;
    }

    // Visual debug for ground check
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
        else
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position, Vector2.down * 1.1f);
        }
    }

    public void ChangeTextCoin()
    {
        coinCounter += 1;
        coinText.text = coinCounter.ToString();
    }

    public void Die()
    {
        // Guard against multiple death calls
        if (isDead) return;
        isDead = true;

        // Stop the character
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Spawn blood particle at the character's position
        if (bloodParticlePrefab != null)
        {
            GameObject blood = Instantiate(
                bloodParticlePrefab,
                transform.position,
                Quaternion.identity
            );

            // Auto-destroy the particle after its duration
            // (adjust 1.5f to match your particle's longest lifetime)
            Destroy(blood, 1.5f);
        }

        // Disable visuals so only the particle shows
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Destroy the player and reload after a delay (0 = immediate)
        if (deathDelay > 0f)
        {
            Destroy(gameObject, deathDelay);
            Invoke(nameof(ReloadCurrentScene), deathDelay);
        }
        else
        {
            Destroy(gameObject);
            ReloadCurrentScene();
        }
    }

    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadSceneAsync(currentSceneName);
    }
}