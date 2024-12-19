using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseWeapon : NetworkBehaviour
{ 
    public float coolDownTime;      // Minimal time between weapon uses in seconds
    public float projectileSpeed;
    public int damage;

    protected GameObject projectilePrefab;
    protected Transform playerTransform;
    
    private float lastShootTime = 0f;   // Timestamp of the last shoot

    private const float projectileOffset = 2.5f;


    protected void Start()
    {
        playerTransform = GetComponent<Transform>();

        if (playerTransform == null)
            Debug.LogError("Player transform is null");

        if (projectilePrefab == null)
            Debug.LogError("Projectile is null");
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
            startPoint += shootingDirection * projectileOffset;
            CmdShootProjectile(startPoint, shootingDirection);

            return true;
        }

        return false;
    }


    [Command]
    private void CmdShootProjectile(Vector3 startPosition, Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
		// Initialize the projectile with the direction and the player that shot it
		GameObject player = GetComponent<NetworkIdentity>().gameObject;
        projectile.GetComponent<Projectile>().Initialize(direction, player, projectileSpeed, damage);
        NetworkServer.Spawn(projectile);
    }
}
