using UnityEngine;


public class DefaultWeapon : BaseWeapon
{
    protected new void Start()
    {
        coolDownTime = 0.5f;
        projectilePrefab = Resources.Load("BaseProjectile") as GameObject;

        base.Start();
    }
}