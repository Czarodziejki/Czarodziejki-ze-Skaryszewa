using Mirror;
using Mirror.Examples.Common.Controllers.Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseWeapon : NetworkBehaviour
{ 
    public float coolDownTime;      // Minimal time between weapon uses in seconds
    public float projectileSpeed;
    public int damage;
    public float projectileOffset = 2.5f;
    public GameObject burstParticleSystem;

    protected GameObject projectilePrefab;
    protected Transform playerTransform;
    protected ParticleSystem burstParticleSystemController;
    
    private float lastShootTime = 0f;   // Timestamp of the last shoot


    protected void Start()
    {
        playerTransform = GetComponent<Transform>();

        if (playerTransform == null)
            Debug.LogError("Player transform is null");

        if (projectilePrefab == null)
            Debug.LogError("Projectile is null");

        if (burstParticleSystem == null)
            Debug.LogError("Burst particle system is null");

        burstParticleSystemController = burstParticleSystem.GetComponent<ParticleSystem>();
        if (burstParticleSystemController == null)
            Debug.LogError("Burst particle system does not have a ParticleSystem component");

        SetColors();
    }

    public virtual bool TryToUseWeapon()
    {
        // Checking time prevents spamming fire button
        if (Time.time - lastShootTime > coolDownTime)
        {
            lastShootTime = Time.time;

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorldPosition.z = 0f;
            Vector2 shootingDirection = (mouseWorldPosition - playerTransform.position).normalized;
            Vector2 startPoint = playerTransform.position;
            //startPoint += shootingDirection * projectileOffset;
            CmdShootProjectile(startPoint, shootingDirection);

            return true;
        }

        return false;
    }

    [ClientRpc]
    protected virtual void EmitBurstParticles(Vector3 startPosition, Vector2 direction)
    {
        burstParticleSystem.transform.SetPositionAndRotation(
            startPosition,
            Quaternion.Euler(0, 0, -0.5f * burstParticleSystemController.shape.arc) * Quaternion.FromToRotation(new(1, 0, 0), direction));
        burstParticleSystemController.Play();
    }

    [Client]
    private void SetColors()
    {
        var particleRenderer = burstParticleSystemController.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material.SetColor("_Color", playerTransform.GetComponent<PlayerController>().ternaryColor);
    }

    [Command]
    private void CmdShootProjectile(Vector3 startPosition, Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
		// Initialize the projectile with the direction and the player that shot it
		GameObject player = GetComponent<NetworkIdentity>().gameObject;
        projectile.GetComponent<Projectile>().Initialize(direction, player, projectileSpeed, damage);
        NetworkServer.Spawn(projectile);
        projectile.GetComponent<Projectile>().SetColors();
        EmitBurstParticles(startPosition, direction);
    }
}
