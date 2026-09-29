using System.Collections;
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
    [SerializeField] private float deathDelay = 0f;
    [SerializeField] private float particleLifetime = 1.5f;

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
        moveAction.Enable();
        jumpAction.Enable();

        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;

        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        // Skip input handling if dead
        if (isDead) return;

        // Read movement input
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
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                groundCheckPoint.position,
                groundCheckRadius,
                groundLayer
            );
            isGrounded = colliders.Length > 0;
        }
        else
        {
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
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isJumping)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsOnGround", isGrounded);

            bool isWalking = Mathf.Abs(moveInput.x) > 0.01f;
            animator.SetBool("IsWalking", isWalking);
        }
    }

    private void UpdateFacingDirection()
    {
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isJumping = false;
    }

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

            Destroy(blood, particleLifetime);
        }

        // Hide the sprite so only the blood shows
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Reload the scene from a coroutine so nothing gets cancelled
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        // Wait for the death delay (0 = immediate)
        if (deathDelay > 0f)
        {
            yield return new WaitForSeconds(deathDelay);
        }

        // Reload the scene FIRST, while this script is still alive
        ReloadCurrentScene();

        // Then destroy the player (it's already gone after scene reload,
        // but this keeps things clean if the reload is delayed)
        Destroy(gameObject);
    }

    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}