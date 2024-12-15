using Mirror;
using Mirror.Examples.Common;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 15f;
    public GameObject crosshair;
    public Vector2 boxSize;
    public float castDistance;
    public Color primaryColor;
    public Color secondaryColor;

	private Collider2D playerCollider;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;

    public LayerMask groundLayer;
    public Animator animator;
    private Camera playerCamera;

    [SyncVar(hook = nameof(OnFlipChanged))]
    private bool isFlipped;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

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
        SetupLocalPlayerCrosshair();
        rigidBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

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

	private void SetupLocalPlayerCrosshair()
    {
		var myCrosshair = Instantiate(crosshair, gameObject.transform);
	}


	void Update()
    {
        if (!isLocalPlayer)
            return;

        horizontal = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("Speed", Mathf.Abs(horizontal));

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            animator.SetBool("IsJumping", true);
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocityX, jumpingPower);
        }

        if (IsGrounded() && rigidBody.linearVelocityY <= 0.01f)
        {
            animator.SetBool("IsJumping", false);
        }

        animator.SetFloat("yVelocity", rigidBody.linearVelocityY > 0.0f ? 1.0f : -1.0f);

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
        if(Physics2D.BoxCast(playerCollider.bounds.center, boxSize, 0, -transform.up, castDistance, groundLayer))
        {
            animator.SetBool("IsJumping", false);
            return true;
        }
        else
        {
            animator.SetBool("IsJumping", true);
            return false;
        }
    }

    private void Flip()
    {
        if (horizontal < -0.01f)
            CmdSetFlipState(false);
        else if (horizontal > 0.01f)
            CmdSetFlipState(true);
    }

    [Command]
    private void CmdSetFlipState(bool flipState)
    {
        isFlipped = flipState;
    }

    private void OnFlipChanged(bool oldValue, bool newValue)
    {
        spriteRenderer.flipX = newValue;
    }

    private void OnDrawGizmos()
    {
        if(playerCollider != null)
        {
            Gizmos.DrawWireCube(playerCollider.bounds.center - transform.up * castDistance, boxSize);
        }
    }
}
