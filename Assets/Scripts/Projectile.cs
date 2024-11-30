using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Projectile : NetworkBehaviour
{
    public float speed = 1f;
    public float lifetime = 10f;

    [SyncVar]
    private Vector2 direction;

    private float timeAlive = 0f;

    private Tilemap tilemap;

    [Server]
    public void Initialize(Vector2 startDirection)
    {
        direction = startDirection.normalized;
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

    [ServerCallback]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            NetworkIdentity identity = collision.collider.GetComponent<NetworkIdentity>();
            if (identity != null && identity.connectionToClient != null)
            {
                int connectionId = identity.connectionToClient.connectionId;
                Debug.Log($"Player hit with connectionId: {connectionId}");
            }
        }

        Tilemap collidedTilemap = collision.collider.GetComponentInParent<Tilemap>();
        if(collidedTilemap == tilemap)
        {
            Vector3 worldPosition = collision.contacts[0].point + direction * 0.1f;
            Vector3Int tilePosition = collidedTilemap.WorldToCell(worldPosition);
            GameMapManager.Instance.TryDestroyTile(tilePosition);
        }

        NetworkServer.Destroy(gameObject);
    }
}
