using Mirror;
using UnityEngine;


public class WeaponToPickUpController : NetworkBehaviour
{
    public WeaponType weaponType;

    private WeaponSpawnersManager spawnersManager;
    private WeaponSpawnerController spawner;


    public void Initialize(WeaponSpawnersManager manager, WeaponSpawnerController spawner)
    {
        spawnersManager = manager;
        this.spawner = spawner;
    }


    [Server]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<ShootingController>().EquipWeapon(weaponType);
            spawnersManager.WeaponWasCollected(spawner);
            NetworkServer.Destroy(gameObject);
        }
    }
}
