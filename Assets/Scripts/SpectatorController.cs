using UnityEngine;
using UnityEngine.InputSystem;


public class SpectatorController : BasePlayerController
{
    public float movementSpeedCoef = 0.3f;

    private bool activated = true;
    private GameObject darkOverlay;

    private InputAction zoomAction;
    private Camera mainCamera;

    public const float zoomSpeed = 20f; // Speed of zooming
    public const float minZoom = 5f;    // Minimum zoom distance
    public const float maxZoom = 20f;   // Maximum zoom distance


    private new void Start()
    {
        base.Start();

        zoomAction = InputSystem.actions.FindAction("Zoom");
        mainCamera = GetComponentInChildren<Camera>();
    }


    private void Awake()
    {
        darkOverlay = GameObject.Find("DarkOverlay");
        if (darkOverlay == null)
            Debug.LogError("DarkOverlay is null");
    }


    void Update()
    {
        if (!isLocalPlayer || !activated || !GetComponent<PlayerController>().Paused)
            return;

        MoveCamera();
        ZoomCamera();
    }


    public void OnGUI()
    {
        if (!isLocalPlayer || GetComponent<PlayerController>().Paused)
            return;

        if (activated)
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                font = Resources.Load<Font>("Fonts/VT323-Regular"),
                fontSize = 40,
            };

            GUI.Label(new Rect(10, 10, Screen.width * 0.2f, 60.0f), "Spectator mode", style);
        }
        else
        {
            float messageWidth = 200;
            var style = new GUIStyle(GUI.skin.box)
            {
                font = Resources.Load<Font>("Fonts/VT323-Regular"),
                fontSize = 40,
            };
            style.normal.textColor = Color.red;

            GUI.Box(new Rect((Screen.width-messageWidth)/2.0f, 20, messageWidth, 50), "You died", style);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
            buttonStyle.normal.textColor = Color.black;
            buttonStyle.hover.textColor = Color.black;
            buttonStyle.active.textColor = Color.black;
            buttonStyle.normal.background = MakeTex(2, 2, new Color(0.9607844f, 0.7294118f, 0.9215687f));
            buttonStyle.hover.background = MakeTex(2, 2, new Color(1f, 0.8820755f, 0.980345f));
            buttonStyle.active.background = MakeTex(2, 2, new Color(0.9811321f, 0.513706f, 0.9019072f));
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            float buttonWidth = 140;
            if (GUI.Button(new Rect((Screen.width-buttonWidth)/2.0f, 85, buttonWidth, 30), "Spectator mode", buttonStyle))
            {
                activated = true;
                darkOverlay.GetComponent<OverlayController>().StartFadeOut(0.5f);
            }
        }
    }


    public void ShowDeathMessage()
    {
        if (!isLocalPlayer)
            return;

        activated = false;
        darkOverlay.GetComponent<OverlayController>().StartFadeIn(1, 0.8f);
    }


    private void ZoomCamera()
    {
        float scrollInput = zoomAction.ReadValue<Vector2>().y;
        if (scrollInput == 0.0f)
            return;


        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize -= scrollInput * zoomSpeed * Time.deltaTime;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
        }
        else
        {
            mainCamera.fieldOfView -= scrollInput * zoomSpeed * Time.deltaTime;
            mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView, minZoom, maxZoom);
        }
    }


    private void MoveCamera()
    {
        Vector2 movement = moveAction.ReadValue<Vector2>();
        GetComponent<Transform>().position += new Vector3(movement.x, movement.y) * movementSpeedCoef;
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
}