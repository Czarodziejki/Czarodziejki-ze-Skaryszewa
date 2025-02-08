using UnityEngine;


public class DefaultWeapon : BaseWeapon
{
    protected new void Start()
    {
        projectilePrefab = Resources.Load("BaseProjectile") as GameObject;

        base.Start();
    }
}