using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Projectile : NetworkBehaviour
{
    public float lifetime = 15f;

    [SyncVar]
    private Vector2 direction;

    private float timeAlive = 0f;
    private float speed = 10f;
    private int damage = 5;

    private Tilemap tilemap;

    [SyncVar]
    private GameObject shootingPlayer;  // the player that shot the projectile

    [Server]
    public void Initialize(Vector2 startDirection, GameObject shootingPlayer, float speed, int damage)
    {
        direction = startDirection.normalized;

        // Rotate projectile so it's facing its direction
        transform.rotation *= Quaternion.FromToRotation(new Vector3(1f, 0f, 0f), (Vector3)direction);

        this.shootingPlayer = shootingPlayer;
        this.speed = speed;
        this.damage = damage;
    }

    [ClientRpc]
    public void SetColors()
    {
        // Set sprite color
        var playerController = shootingPlayer.GetComponent<PlayerController>();
        GetComponent<SpriteRenderer>().color = playerController.secondaryColor;

        // Set colors of the trails
        var materialList = GetComponentInChildren<TrailRenderer>().materials;
        if (materialList == null || materialList.Length == 0)
        {
            Debug.LogError("Trail material list should not be empty!");
        }

        materialList[0].SetColor("_Color1", playerController.secondaryColor);
        materialList[0].SetColor("_Color2", playerController.ternaryColor);

        // Random texture animation offset
        float timeOffset = Random.Range(0.0f, 1.0f);
        materialList[0].SetFloat("_TimeOffset", timeOffset);
    }

    private void Awake()
    {
        tilemap = GameMapManager.Instance.GetComponentInChildren<Tilemap>();
    }

    private void Update()
    {
        if (isServer)
        {
            MoveProjectile();
            CheckLifetime();
        }
    }

    [Server]
    private void MoveProjectile()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    [Server]
    private void CheckLifetime()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            PrepareToDestroy();
            NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    private void PrepareToDestroy()
    {
        void DetachTrail()
        {
            // Set parent of the trail to null so that it persists after the projectile is destroyed
            var trail = gameObject.GetComponentInChildren<TrailRenderer>();
            trail.transform.parent = null;
            // Adjust the parameters to create an absorption-like effect
            trail.time = 0.2f;
            trail.widthMultiplier *= 0.3f;
            trail.widthCurve = new AnimationCurve(new(0.0f, 1.0f), new(1.0f, 0.0f));
        }

        [ClientRpc]
        void DetachTrailOnClients()
        {
            DetachTrail();
        }

        DetachTrail();
        DetachTrailOnClients();
    }

    

    [Server]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            NetworkIdentity identity = collision.collider.GetComponent<NetworkIdentity>();
            if (identity != null && identity.connectionToClient != null)
            {
                int connectionId = identity.connectionToClient.connectionId;
                int shooterConnectionId = shootingPlayer.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
                Debug.Log($"Player hit with connectionId: {connectionId} by the player with connectionId: {shooterConnectionId}");
            }

            var healthController = collision.collider.GetComponent<HealthController>();
            healthController.Damage(damage);
        }

        Tilemap collidedTilemap = collision.collider.GetComponentInParent<Tilemap>();
        if(collidedTilemap == tilemap)
        {
            Vector3 worldPosition = collision.contacts[0].point + direction * 0.1f;
            Vector3Int tilePosition = collidedTilemap.WorldToCell(worldPosition);
            if (GameMapManager.Instance.DamageTile(tilePosition, damage))
            {
                shootingPlayer.GetComponent<BuildController>().blocksInInventory++;
            }
        }

        PrepareToDestroy();
        NetworkServer.Destroy(gameObject);
    }
}
