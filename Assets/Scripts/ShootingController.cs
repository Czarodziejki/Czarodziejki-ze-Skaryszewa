using Mirror;
using UnityEngine;


public class ShootingController : NetworkBehaviour
{
    public GameObject projectilePrefab;
    public float coolDownTime = 0.2f;

    private Transform playerTransform;
    private float lastShootTime = 0f;   // Timestamp of the last shoot


    private void Start()
    {
        playerTransform = GetComponent<Transform>();
    }

    private void Update()
    {
        // Checking time prevents spamming fire button
        if (isLocalPlayer && Input.GetMouseButton(0) && Time.time - lastShootTime > coolDownTime)
        {
            lastShootTime = Time.time;

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;
            Vector2 shootingDirection = (mouseWorldPosition - playerTransform.position).normalized;
            Vector2 startPoint = playerTransform.position;
            startPoint += shootingDirection * 2.0f;
            CmdShootProjectile(startPoint, shootingDirection);
        }
    }

    [Command]
    private void CmdShootProjectile(Vector3 startPosition, Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
        projectile.GetComponent<Projectile>().Initialize(direction);
        NetworkServer.Spawn(projectile);
    }
}
