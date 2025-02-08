using Mirror;
using UnityEngine;


public class FastWeapon : WeaponWithLimitedAmmo
{
    protected new void Start()
    {
        projectilePrefab = Resources.Load("FastProjectile") as GameObject;

        base.Start();
    }

    [ClientRpc]
    protected override void EmitBurstParticles(Vector3 startPosition, Vector2 direction)
    {
        burstParticleSystem.transform.SetPositionAndRotation(
            startPosition,
            Quaternion.Euler(0, 0, -0.5f * burstParticleSystemController.shape.arc) * Quaternion.FromToRotation(new(1, 0, 0), -direction));
        burstParticleSystemController.Play();
    }
}