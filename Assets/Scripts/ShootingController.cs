using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;


public class ShootingController : NetworkBehaviour
{
    public GameObject projectilePrefab;
    public float coolDownTime = 0.2f;

    private Transform playerTransform;
    private float lastShootTime = 0f;   // Timestamp of the last shoot

    private InputAction fireAction;
    private void Start()
    {
        playerTransform = GetComponent<Transform>();
        fireAction = InputSystem.actions.FindAction("Attack");
    }

    private void Update()
    {
        // Checking time prevents spamming fire button
        if (isLocalPlayer && fireAction.IsPressed() && Time.time - lastShootTime > coolDownTime)
        {
            lastShootTime = Time.time;

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorldPosition.z = 0f;
            Vector2 shootingDirection = (mouseWorldPosition - playerTransform.position).normalized;
            Vector2 startPoint = playerTransform.position;
            startPoint += shootingDirection * 2.5f;
            CmdShootProjectile(startPoint, shootingDirection);
        }
    }

    [Command]
    private void CmdShootProjectile(Vector3 startPosition, Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
        // Initialize the projectile with the direction and the player that shot it
        GameObject player = GetComponent<NetworkIdentity>().gameObject;
        projectile.GetComponent<Projectile>().Initialize(direction, player);
        NetworkServer.Spawn(projectile);
    }
}
