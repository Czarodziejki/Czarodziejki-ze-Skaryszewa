using Mirror;
using Mirror.Examples.Common;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 15f;

    private Collider2D playerCollider;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;

    public LayerMask groundLayer;
    private Camera playerCamera;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (!isLocalPlayer)
        {
            if (playerCamera != null)
            {
                playerCamera.enabled = false;

                var audioListener = playerCamera.GetComponent<AudioListener>();
                if (audioListener != null)
                {
                    audioListener.enabled = false;
                }
            }

            return;
        }
        SetupLocalPlayerCamera();
        rigidBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerCollider == null || rigidBody == null)
        {
            Debug.LogError("Rigidbody2D or Collider2D is missing on the player!");
        }
    }

    private void SetupLocalPlayerCamera()
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            var audioListener = playerCamera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }
            playerCamera.tag = "MainCamera";
            Camera.SetupCurrent(playerCamera);
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpingPower);
        }

        Flip();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        rigidBody.linearVelocity = new Vector2(horizontal * speed, rigidBody.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        Vector2 boxCenter = new Vector2(playerCollider.bounds.center.x, playerCollider.bounds.min.y - 0.1f);
        Vector2 boxSize = new Vector2(playerCollider.bounds.size.x * 0.9f, 0.1f);

        return Physics2D.OverlapBox(
            boxCenter,
            boxSize,
            0f,
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
