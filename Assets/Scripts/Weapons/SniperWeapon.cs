using UnityEngine;


public class SniperWeapon : WeaponWithLimitedAmmo
{
    protected new void Start()
    {
        coolDownTime = 5.0f;
        projectilePrefab = Resources.Load("SniperProjectile") as GameObject;

        maxAmmo = 20;

        base.Start();
    }
}
