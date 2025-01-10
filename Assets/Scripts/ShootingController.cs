using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum WeaponType : byte
{
    DefaultWeapon = 0,
    FastWeapon = 1,
    SniperWeapon = 2
}


public class ShootingController : NetworkBehaviour
{
    private BaseWeapon weapon;

    private InputAction fireAction;

    private Dictionary<WeaponType, BaseWeapon> weaponRepository;

    public void EquipWeapon(WeaponType weaponType)
    {
        if (!isLocalPlayer)
            return;

        weapon = weaponRepository[weaponType];

        if (weapon is WeaponWithLimitedAmmo w)
        {
            w.ResetAmmo();
        }
    }

    //[TargetRpc]
    //public void RpcEquipWeapon(NetworkConnectionToClient target, WeaponType weaponType)
    //{
    //    weapon = weaponRepository[weaponType];

    //    if (weapon is WeaponWithLimitedAmmo w)
    //    {
    //        w.ResetAmmo();
    //    }
    //}



    private void Start()
    {
        if (isLocalPlayer)
        {
            fireAction = InputSystem.actions.FindAction("Attack");

            weaponRepository = new Dictionary<WeaponType, BaseWeapon>
            {
                { WeaponType.DefaultWeapon, GetComponent<DefaultWeapon>() },
                { WeaponType.FastWeapon, GetComponent<FastWeapon>() },
                { WeaponType.SniperWeapon, GetComponent<SniperWeapon>() },
            };

            EquipWeapon(WeaponType.DefaultWeapon);
        }
    }

    private void Update()
    {
        if (isLocalPlayer && fireAction.IsPressed())
        {
            weapon.TryToUseWeapon();
        }
    }
}