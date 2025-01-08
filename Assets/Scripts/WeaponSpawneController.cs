using UnityEngine;


public class WeaponSpawnerController : MonoBehaviour
{
    public float spawnTime = 5.0f;
    public GameObject[] possibleWeapons;

    //private GameObject spawnedObject = null;
    //private Coroutine coroutine;
    //private System.Random rnd;

    //private bool weaponSpawned { get { return spawnedObject != null; } }

    //private void Start()
    //{
    //    NetworkIdentity identity = GetComponentInParent<NetworkIdentity>();
    //    if (identity != null)
    //    {
    //        Destroy(gameObject.GetComponent<NetworkIdentity>());
    //    }

    //    Debug.Log("Weapons spawner started");
    //    if (!isServer)
    //        return;

    //    if (possibleWeapons.Length > 0)
    //    {
    //        rnd = new System.Random();
    //        coroutine = StartCoroutine(TryToSpawnWeapon());
    //    }
            
    //}


    //IEnumerator TryToSpawnWeapon()
    //{
    //    Debug.Log("Coroutine started");
    //    while (!weaponSpawned)
    //    {
    //        Debug.Log("Starting to spawn weapon");
    //        SpawnWeapon();

    //        yield return new WaitForSeconds(updateTime);
    //    }
    //}


    //private void SpawnWeapon()
    //{
    //    int index = rnd.Next(0, possibleWeapons.Length);

    //    var transform = gameObject.GetComponent<Transform>();
    //    Vector3 weaponPosition = transform.position + new Vector3(0, 1);
    //    spawnedObject = Instantiate(possibleWeapons[index], weaponPosition, Quaternion.identity);
    //    NetworkServer.Spawn(spawnedObject);

    //    Debug.Log("Weapon spawned");
    //}


    //public GameObject GetWeaponToSpawn()
    //{
    //    if (possibleWeapons.Length == 0)
    //        return null;

    //    int index = rnd.Next(0, possibleWeapons.Length);
    //    return possibleWeapons[index];
    //}
}
