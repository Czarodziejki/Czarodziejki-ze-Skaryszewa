using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Projectile : NetworkBehaviour
{
    public float lifetime = 15f;

    private float timeAlive = 0f;
    private int damage = 5;

    private Tilemap tilemap;

    [SyncVar]
    private GameObject shootingPlayer;  // the player that shot the projectile

    private Rigidbody2D rigidbody;

    [Server]
    public void Initialize(Vector2 startDirection, GameObject shootingPlayer, float speed, int damage)
    {
        rigidbody.linearVelocity = startDirection.normalized * speed;

        this.shootingPlayer = shootingPlayer;
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

    Vector2 GetDirection()
    {
        return rigidbody.linearVelocity.normalized;
    }

    private void Awake()
    {
        tilemap = GameMapManager.Instance.GetComponentInChildren<Tilemap>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isServer)
        {
            CheckLifetime();
        }
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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            NetworkIdentity identity = collision.GetComponent<NetworkIdentity>();
            if (identity != null && identity.connectionToClient != null)
            {
                int connectionId = identity.connectionToClient.connectionId;
                int shooterConnectionId = shootingPlayer.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
                Debug.Log($"Player hit with connectionId: {connectionId} by the player with connectionId: {shooterConnectionId}");
            }

            var healthController = collision.GetComponent<HealthController>();
            healthController.Damage(damage);
        }

        Tilemap collidedTilemap = collision.GetComponentInParent<Tilemap>();
        if(collidedTilemap == tilemap)
        {
            Vector2 contactPoint = collision.ClosestPoint(transform.position);
            Vector3 worldPosition = contactPoint + GetDirection() * 0.1f;
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
