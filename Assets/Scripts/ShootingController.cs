using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class ShootingController : NetworkBehaviour
{
    public GameObject projectilePrefab;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GetComponent<Transform>();
    }

    private void Update()
    {
        if (isLocalPlayer && Input.GetMouseButtonDown(0))
        {
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
