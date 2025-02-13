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

    [Client]
    override protected void SetColors()
    {
        var player = playerTransform.GetComponent<PlayerController>();
        var particleRenderer = burstParticleSystemController.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material.SetColor("_Color", player.secondaryColor);
        particleRenderer.trailMaterial.SetColor("_Color", player.secondaryColor);

        var subemitters = burstParticleSystemController.GetComponent<ParticleSystem>().subEmitters;
        var particleSubemitter = subemitters.GetSubEmitterSystem(0).GetComponent<ParticleSystemRenderer>();
        particleSubemitter.material.SetColor("_Color", player.ternaryColor);
    }
}