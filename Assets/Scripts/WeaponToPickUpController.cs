using Mirror;
using UnityEngine;


public class WeaponToPickUpController : NetworkBehaviour
{
    public WeaponType weaponType;

    private WeaponSpawnersManager spawnersManager;
    private WeaponSpawnerData spawner;


    public void Initialize(WeaponSpawnersManager manager, WeaponSpawnerData spawner)
    {
        spawnersManager = manager;
        this.spawner = spawner;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var shootingController = collision.gameObject.GetComponent<ShootingController>();
            shootingController.EquipWeapon(weaponType);

            if (isServer)
            {
                spawnersManager.WeaponWasCollected(spawner);
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}
