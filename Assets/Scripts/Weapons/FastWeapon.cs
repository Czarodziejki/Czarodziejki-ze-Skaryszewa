using UnityEngine;


public class FastWeapon : WeaponWithLimitedAmmo
{
    protected new void Start()
    {
        coolDownTime = 0.1f;
        projectilePrefab = Resources.Load("FastProjectile") as GameObject;

        maxAmmo = 100;

        base.Start();
    }
}