using UnityEngine;


public class SniperWeapon : WeaponWithLimitedAmmo
{
    protected new void Start()
    {
        projectilePrefab = Resources.Load("SniperProjectile") as GameObject;

        base.Start();
    }
}
