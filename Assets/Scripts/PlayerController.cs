using Mirror;
using Mirror.Examples.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : BasePlayerController
{
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 15f;
    public GameObject crosshair;
    private GameObject myCrosshair = null;
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

    [SyncVar(hook = nameof(OnFlipChanged))]
    private bool isFlipped;

    private InputAction jumpAction;
    private Vector2 previousMovementInput;

    private InputAction pauseAction;
    private bool _paused = false;
    public bool Paused { 
        get => _paused;
        private set
        {
            if (myCrosshair != null) myCrosshair.GetComponent<CrosshairController>().SetPaused(value);
            _paused = value;
        }
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    new void Start()
    {
        base.Start();

        SetupLocalPlayerCamera();
        SetupLocalPlayerAudioListener();
        SetupLocalPlayerCrosshair();
        rigidBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        if (playerCollider == null || rigidBody == null)
        {
            Debug.LogError("Rigidbody2D or Collider2D is missing on the player!");
        }

        jumpAction = InputSystem.actions.FindAction("Jump");
        pauseAction = InputSystem.actions.FindAction("Pause");
    }


    private void SetupLocalPlayerCrosshair()
    {
        if (isLocalPlayer)
            myCrosshair = Instantiate(crosshair, gameObject.transform);
    }


    void Update()
    {
        if (pauseAction.triggered) Paused = !Paused;

        if (!isLocalPlayer)
            return;

        Vector2 movement = Paused ? Vector2.zero : moveAction.ReadValue<Vector2>();

        horizontal = movement.x;
        animator.SetFloat("Speed", Mathf.Abs(horizontal));

        if (!Paused) {
            bool jump = jumpAction.triggered || (movement.y > 0 && previousMovementInput.y <= 0);

            previousMovementInput = movement;

            if (jump && IsGrounded())
            {
                animator.SetBool("IsJumping", true);
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocityX, jumpingPower);
            }
        }

        if (IsGrounded() && rigidBody.linearVelocityY <= 0.01f)
        {
            animator.SetBool("IsJumping", false);
        }

        animator.SetFloat("yVelocity", rigidBody.linearVelocityY > 0.0f ? 1.0f : -1.0f);

        Flip();
    }

    public void OnGUI()
    {
        if (!isLocalPlayer || !Paused) return;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
        buttonStyle.normal.textColor = Color.black;
        buttonStyle.hover.textColor = Color.black;
        buttonStyle.active.textColor = Color.black;
        buttonStyle.fontSize = 64;
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.9607844f, 0.7294118f, 0.9215687f));
        buttonStyle.hover.background = MakeTex(2, 2, new Color(1f, 0.8820755f, 0.980345f));
        buttonStyle.active.background = MakeTex(2, 2, new Color(0.9811321f, 0.513706f, 0.9019072f));
        buttonStyle.alignment = TextAnchor.MiddleCenter;
        float buttonWidth = 500;
        float buttonHeight = 100;
        if (GUI.Button(new Rect((Screen.width - buttonWidth) / 2.0f, (int)(Screen.height / 2 - 1.5 * buttonHeight), buttonWidth, buttonHeight), "RESUME", buttonStyle))
        {
            Paused = false;
        }
        GameNetworkManager manager = NetworkManager.singleton as GameNetworkManager;
        bool isHost = manager.IsHost();
        if (GUI.Button(new Rect((Screen.width - buttonWidth) / 2.0f, (int)(Screen.height / 2 + 0.5 * buttonHeight), buttonWidth, buttonHeight), isHost ? "RETURN TO ROOM" : "LEAVE", buttonStyle))
        {
            if (isHost)
            {
                manager.ServerChangeScene(manager.RoomScene);
            }
            else
            {
                manager.StopClient();
            }
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
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
