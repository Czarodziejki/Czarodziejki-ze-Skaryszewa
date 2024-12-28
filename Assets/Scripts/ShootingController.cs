using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;


public class ShootingController : NetworkBehaviour
{
    private BaseWeapon weapon;

    private InputAction fireAction;

    public void EquipWeapon<WeaponType>() where WeaponType : BaseWeapon
    {
		weapon = gameObject.GetComponent<WeaponType>();
	}

    private void Start()
    {
        if (isLocalPlayer)
        {
			EquipWeapon<SniperWeapon>();
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