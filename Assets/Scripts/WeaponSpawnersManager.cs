using Mirror;
using System.Collections;
using UnityEngine;



public class WeaponSpawnersManager: NetworkBehaviour
{
    private WeaponSpawnerData[] spawnersControllers;
    private System.Random rnd;

    private void Start()
    {
        if (!isServer)
            return;

        spawnersControllers = GetComponentsInChildren<WeaponSpawnerData>();
        rnd = new System.Random();

        foreach (var spawner in spawnersControllers)
            SpawnWeapon(spawner);
    }


    public void WeaponWasCollected(WeaponSpawnerData spawner)
    {
        if (!isServer)
            return;

        StartCoroutine(WaitAndSpawnWeapon(spawner));
    }


    [Server]
    private void SpawnWeapon(WeaponSpawnerData spawner)
    {
        GameObject weapon = GetWeaponToSpawn(spawner);
        if (weapon == null)
            return;

        GameObject instantiatedWeapon = Instantiate(weapon, spawner.weaponPosition, Quaternion.identity);
        NetworkServer.Spawn(instantiatedWeapon);
        instantiatedWeapon.GetComponent<WeaponToPickUpController>().Initialize(this, spawner);
    }


    [Server]
    private GameObject GetWeaponToSpawn(WeaponSpawnerData spawner)
    {
        if (spawner.possibleWeapons.Length == 0)
            return null;

        int index = rnd.Next(0, spawner.possibleWeapons.Length);
        return spawner.possibleWeapons[index];
    }


    private IEnumerator WaitAndSpawnWeapon(WeaponSpawnerData spawner)
    {
        yield return new WaitForSeconds(spawner.spawnTime);

        SpawnWeapon(spawner);
    }
}
