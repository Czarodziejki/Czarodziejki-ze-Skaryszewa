using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;


public class ShootingController : NetworkBehaviour
{
    private BaseWeapon weapon;

    private InputAction fireAction;

    private void Start()
    {
        if (isLocalPlayer)
        {
            // NOT WORKING :(
            //weapon = gameObject.AddComponent<DefaultWeapon>();

            weapon = gameObject.GetComponent<DefaultWeapon>();
            fireAction = InputSystem.actions.FindAction("Attack");
        }
    }

    private void Update()
    {
        // Checking time prevents spamming fire button
        if (isLocalPlayer && fireAction.IsPressed())
        {
            weapon.TryToUseWeapon();
        }
    }

    //[Command]
    //private void CmdShootProjectile(Vector3 startPosition, Vector2 direction)
    //{
    //    GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
    //    // Initialize the projectile with the direction and the player that shot it
    //    GameObject player = GetComponent<NetworkIdentity>().gameObject;
    //    projectile.GetComponent<Projectile>().Initialize(direction, player);
    //    NetworkServer.Spawn(projectile);
    //}
}