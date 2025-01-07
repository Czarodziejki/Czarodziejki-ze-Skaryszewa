using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;


public class BasePlayerController : NetworkBehaviour
{
    protected InputAction moveAction;


    protected void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }

    public void SetupLocalPlayerCamera()
    {
        Camera camera = GetComponentInChildren<Camera>();
        if (camera == null)
        {
            if (!TryGetComponent<Camera>(out camera))
            {
                Debug.LogError("Cannot find camera");
                return;
            }
        }

        // Enable camera in local player and disable in others
        bool enableCamera = isLocalPlayer;

        camera.enabled = enableCamera;
        if (camera.TryGetComponent<AudioListener>(out var audioListener))
            audioListener.enabled = enableCamera;

        if (enableCamera)
        {
            camera.tag = "MainCamera";
            Camera.SetupCurrent(camera);
        }
    }
}