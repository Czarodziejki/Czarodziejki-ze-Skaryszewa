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
        if (!isLocalPlayer || !activated)
            return;

        MoveCamera();
        ZoomCamera();
    }


    public void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        if (activated)
        {
            GUILayout.BeginArea(new Rect(10, 10f, 140f, 40f));
            GUI.Box(new Rect(10, 10, 100, 30), "Spectator mode");
            GUILayout.EndArea();
        }
        else
        {
            float messageWidth = 200;
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 40,
            };
            style.normal.textColor = Color.red;

            GUI.Box(new Rect((Screen.width-messageWidth)/2.0f, 20, messageWidth, 50), "You died", style);

            float buttonWidth = 140;
            if (GUI.Button(new Rect((Screen.width-buttonWidth)/2.0f, 85, buttonWidth, 30), "Spectator mode"))
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
}