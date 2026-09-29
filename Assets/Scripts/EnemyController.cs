using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform position1;
    public Transform position2;

    public float velocity;

    private bool seguindoPos1 = true;
    private Rigidbody2D rb;

    public float minimumDistance;
    private float targetX;

    [Header("Facing")]
    [Tooltip("Check if the enemy sprite is drawn facing RIGHT by default. Uncheck if it faces LEFT.")]
    [SerializeField] private bool spriteFacesRightByDefault = true;

    private bool facingRight;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetX = position1.position.x;

        // Apply the initial facing direction
        facingRight = spriteFacesRightByDefault;
        ApplyFacing();
    }

    void Update()
    {
        float personagemX = transform.position.x;
        float distance = Mathf.Abs(personagemX - targetX);

        if (distance < minimumDistance)
        {
            if (seguindoPos1)
                targetX = position2.position.x;
            else
                targetX = position1.position.x;

            seguindoPos1 = !seguindoPos1;
        }

        if (personagemX > targetX)
        {
            rb.linearVelocity = new Vector2(-velocity, rb.linearVelocity.y);
            SetFacing(false); // moving left -> face left
        }
        else
        {
            rb.linearVelocity = new Vector2(velocity, rb.linearVelocity.y);
            SetFacing(true);  // moving right -> face right
        }
    }

    private void SetFacing(bool faceRight)
    {
        if (facingRight == faceRight) return;
        facingRight = faceRight;
        ApplyFacing();
    }

    private void ApplyFacing()
    {
        // Preserve the original scale magnitude, only change the sign of X
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        transform.localScale = scale;
    }
}