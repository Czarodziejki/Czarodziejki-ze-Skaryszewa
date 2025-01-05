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
}