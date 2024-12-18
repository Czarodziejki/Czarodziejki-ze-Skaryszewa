using UnityEngine;


public class FastWeapon : WeaponWithLimitedAmmo
{
    protected new void Start()
    {
        projectilePrefab = Resources.Load("FastProjectile") as GameObject;

        base.Start();
    }
}