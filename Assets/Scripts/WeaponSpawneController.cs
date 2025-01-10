using UnityEngine;


public class WeaponSpawnerController : MonoBehaviour
{
    public float spawnTime = 5.0f;
    public GameObject[] possibleWeapons;

    public Vector3 weaponPosition { get; private set; }


    private void Awake()
    {
        // Skipping parent's transform
        weaponPosition = GetComponentsInChildren<Transform>()[1].position;
        if (weaponPosition == null)
            Debug.LogError("Cannot find weapon position");
    }
}
