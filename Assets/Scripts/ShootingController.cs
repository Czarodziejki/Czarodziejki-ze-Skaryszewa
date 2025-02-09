using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum WeaponType : byte
{
    DefaultWeapon = 0,
    FastWeapon = 1,
    SniperWeapon = 2,
    AOEWeapon = 3
}


public class ShootingController : NetworkBehaviour
{
    private BaseWeapon weapon;

    private InputAction fireAction;

    private Dictionary<WeaponType, BaseWeapon> weaponRepository;

    private WeaponUIController uiController;

    public void EquipWeapon(WeaponType weaponType)
    {
        if (!isLocalPlayer)
            return;

        weapon = weaponRepository[weaponType];

        if (weapon is WeaponWithLimitedAmmo w)
        {
            w.ResetAmmo();
        }

        uiController.UpdateWeaponType(weaponType);
        uiController.UpdateAmmo(weapon);
    }


    private void Start()
    {
        if (isLocalPlayer)
        {
            uiController = gameObject.GetComponent<WeaponUIController>();
            fireAction = InputSystem.actions.FindAction("Attack");

            weaponRepository = new Dictionary<WeaponType, BaseWeapon>
            {
                { WeaponType.DefaultWeapon, GetComponent<DefaultWeapon>() },
                { WeaponType.FastWeapon, GetComponent<FastWeapon>() },
                { WeaponType.SniperWeapon, GetComponent<SniperWeapon>() },
                { WeaponType.AOEWeapon, GetComponent<AOEWeapon>() },
            };

            EquipWeapon(WeaponType.AOEWeapon);
        }
    }

    private void Update()
    {
        if (isLocalPlayer && fireAction.IsPressed())
        {
            if (weapon.TryToUseWeapon())
                uiController.UpdateAmmo(weapon);
        }
    }
}