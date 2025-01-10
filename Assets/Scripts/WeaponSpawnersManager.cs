using Mirror;
using System.Collections;
using UnityEngine;



public class WeaponSpawnersManager: NetworkBehaviour
{
    private WeaponSpawnerController[] spawnersControllers;
    private System.Random rnd;

    private void Start()
    {
        if (!isServer)
            return;

        spawnersControllers = GetComponentsInChildren<WeaponSpawnerController>();
        rnd = new System.Random();

        foreach (var spawner in spawnersControllers)
            SpawnWeapon(spawner);
    }


    public void WeaponWasCollected(WeaponSpawnerController spawner)
    {
        if (!isServer)
            return;

        StartCoroutine(WaitAndSpawnWeapon(spawner));
    }


    [Server]
    private void SpawnWeapon(WeaponSpawnerController spawner)
    {
        GameObject weapon = GetWeaponToSpawn(spawner);
        if (weapon == null)
            return;

        GameObject instantiatedWeapon = Instantiate(weapon, spawner.weaponPosition, Quaternion.identity);
        NetworkServer.Spawn(instantiatedWeapon);
        instantiatedWeapon.GetComponent<WeaponToPickUpController>().Initialize(this, spawner);
    }


    [Server]
    private GameObject GetWeaponToSpawn(WeaponSpawnerController spawner)
    {
        if (spawner.possibleWeapons.Length == 0)
            return null;

        int index = rnd.Next(0, spawner.possibleWeapons.Length);
        return spawner.possibleWeapons[index];
    }


    private IEnumerator WaitAndSpawnWeapon(WeaponSpawnerController spawner)
    {
        yield return new WaitForSeconds(spawner.spawnTime);

        SpawnWeapon(spawner);
    }
}
