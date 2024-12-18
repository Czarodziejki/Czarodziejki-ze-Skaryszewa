using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;


public class ShootingController : NetworkBehaviour
{
    private BaseWeapon weapon;

    private InputAction fireAction;

    private void Start()
    {
        if (isLocalPlayer)
        {
            weapon = gameObject.GetComponent<DefaultWeapon>();
            fireAction = InputSystem.actions.FindAction("Attack");
        }
    }

    private void Update()
    {
        // Checking time prevents spamming fire button
        if (isLocalPlayer && fireAction.IsPressed())
        {
            weapon.TryToUseWeapon();
        }
    }
}