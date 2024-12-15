using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Projectile : NetworkBehaviour
{
    public float speed = 10f;
    public float lifetime = 15f;
    public int damage = 5;

    [SyncVar]
    private Vector2 direction;

    private float timeAlive = 0f;

    private Tilemap tilemap;

    private GameObject shootingPlayer;  // the player that shot the projectile

    [Server]
    public void Initialize(Vector2 startDirection, GameObject shootingPlayer)
    {
        direction = startDirection.normalized;

        // Rotate projectile so it's facing it's direction
        transform.rotation *= Quaternion.FromToRotation(new Vector3(1f, 0f, 0f), (Vector3)direction);

        this.shootingPlayer = shootingPlayer;
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
            NetworkServer.Destroy(gameObject);
        }
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
            if (GameMapManager.Instance.TryDestroyTile(tilePosition))
            {
                shootingPlayer.GetComponent<BuildController>().blocksInInventory++;
            }
        }

        NetworkServer.Destroy(gameObject);
    }
}
