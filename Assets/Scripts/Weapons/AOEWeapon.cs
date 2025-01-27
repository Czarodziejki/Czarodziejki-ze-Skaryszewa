using Mirror;
using UnityEngine;


public class AOEWeapon : WeaponWithLimitedAmmo
{
    public int range;
    public float distanceMultiplier;

    protected new void Start()
    {
        projectilePrefab = Resources.Load("ExlodingProjectile") as GameObject;

        base.Start();
    }

    [Server]
    protected override void SetUpProjectile(GameObject projectile)
    {
        base.SetUpProjectile(projectile);

        var explodingComp = projectile.GetComponent<ExplodingProjectile>();
        explodingComp.range = range;
        explodingComp.distanceMultiplier = distanceMultiplier;
    }
}