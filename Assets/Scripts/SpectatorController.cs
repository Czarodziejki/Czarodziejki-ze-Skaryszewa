using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpectatorController : BasePlayerController
{
    public float movementSpeedCoef = 0.5f;


    void Update()
    {
        if (!isLocalPlayer)
            return;

        Vector2 movement = moveAction.ReadValue<Vector2>();
        var transform = GetComponent<Transform>();

        transform.position += new Vector3(movement.x, movement.y) * movementSpeedCoef;
    }

    public void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        GUILayout.BeginArea(new Rect(10, 10f, 140f, 40f));
        GUI.Box(new Rect(10, 10, 100, 30), "Spectator mode");
        GUILayout.EndArea();
    }
}