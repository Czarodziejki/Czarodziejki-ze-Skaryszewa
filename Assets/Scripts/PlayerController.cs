using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 15f;

    public Collider2D collider;
    public Rigidbody2D rigidBody;
    public LayerMask groundLayer;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (collider == null || rigidBody == null)
        {
            Debug.LogError("Rigidbody2D or Collider2D is missing on the player!");
        }
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("Speed", Mathf.Abs(horizontal));

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocityX, jumpingPower);
        }

        Flip();
    }

    private void FixedUpdate()
    {
        rigidBody.linearVelocity = new Vector2(horizontal * speed, rigidBody.linearVelocityY);
    }

    private bool IsGrounded()
    {
        Vector2 boxCenter = new Vector2(collider.bounds.center.x, collider.bounds.min.y - 0.1f);
        Vector2 boxSize = new Vector2(collider.bounds.size.x * 0.9f, 0.1f);

        return Physics2D.OverlapBox(
            boxCenter,
            boxSize,
            0.2f,
            groundLayer
        );
    }

    private void Flip()
    {
        if (horizontal < -0.01f)
            spriteRenderer.flipX = false;
        else if (horizontal > 0.01f)
            spriteRenderer.flipX = true;
    }

}
