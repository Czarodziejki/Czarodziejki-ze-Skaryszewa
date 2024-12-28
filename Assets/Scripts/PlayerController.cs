using Mirror;
using Mirror.Examples.Common;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 15f;
    public GameObject crosshair;
    public Vector2 boxSize;
    public float castDistance;

    [ColorUsage(true, true)]
    public Color primaryColor;
    [ColorUsage(true, true)]
    public Color secondaryColor;
    [ColorUsage(true, true)]
    public Color ternaryColor;

    private Collider2D playerCollider;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;

    public LayerMask groundLayer;
    public Animator animator;
    private Camera playerCamera;

    [SyncVar(hook = nameof(OnFlipChanged))]
    private bool isFlipped;

    private InputAction moveAction, jumpAction;
    private Vector2 previousMovementInput;

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

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
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
		Instantiate(crosshair, gameObject.transform);
	}


	void Update()
    {
        if (!isLocalPlayer)
            return;

        Vector2 movement = moveAction.ReadValue<Vector2>();

        horizontal = movement.x;
        animator.SetFloat("Speed", Mathf.Abs(horizontal));

        bool jump = jumpAction.triggered || (movement.y > 0 && previousMovementInput.y <= 0);

        previousMovementInput = movement;

        if (jump && IsGrounded())
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
