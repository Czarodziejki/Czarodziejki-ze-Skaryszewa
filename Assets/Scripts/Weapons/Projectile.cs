using Mirror;
using Mirror.BouncyCastle.Asn1.Cmp;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;


public class Projectile : NetworkBehaviour
{
    public float lifetime = 15f;
    public float explosionParticleSpeedCoefficient = 0.2f;
    public int tilemapDamageRange = 1;

    public GameObject explosionParticleSystemPrefab;
    public GameObject trail;
    public GameObject pointLight;

    private float timeAlive = 0f;
    protected int damage = 0;

    private Tilemap tilemap;
    private new Rigidbody2D rigidbody;

    [SyncVar]
    private GameObject shootingPlayer;  // the player that shot the projectile
    [SyncVar]
    private float speed = 0f;

    bool collisionDetected = false;
    Vector2 detectedCollisionPoint;
    Vector2 explosionParticleOrigin;
    Quaternion explosionParticleDirection;

    private Vector2 Direction
    {
        get
        {
            return rigidbody.linearVelocity.normalized;
        }  
    }

    [Server]
    public void Initialize(Vector2 startDirection, GameObject shootingPlayer, float speed, int damage)
    {
        rigidbody.linearVelocity = startDirection.normalized * speed;
        transform.rotation = Quaternion.FromToRotation(new(1, 0, 0), startDirection);

        this.shootingPlayer = shootingPlayer;
        this.speed = speed;
        this.damage = damage;        
    }

    [ClientRpc]
    public void SetColors()
    {
        // Set sprite color
        var playerController = shootingPlayer.GetComponent<PlayerController>();
        GetComponent<Light2D>().color = playerController.secondaryColor; // Volumetric sprite light (instead of sprite renderer)
        pointLight.GetComponent<Light2D>().color = playerController.secondaryColor; // Point light illuminating the foreground

        // Set colors of the trails
        var trailMaterial = trail.GetComponent<TrailRenderer>().sharedMaterial;

        trailMaterial.SetColor("_Color1", playerController.secondaryColor);
        trailMaterial.SetColor("_Color2", playerController.ternaryColor);
        
        // Random texture animation offset
        float timeOffset = UnityEngine.Random.Range(0.0f, 1.0f);
        trailMaterial.SetFloat("_TimeOffset", timeOffset);
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
            NetworkServer.Destroy(gameObject);
        }
    }
    
    [Server]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == shootingPlayer)
            return;

        collisionDetected = true;
        detectedCollisionPoint = collision.ClosestPoint(transform.position);

        HandleCollision(collision);

        CalculateParticleExplosion();
        NetworkServer.Destroy(gameObject);
    }


    [Server]
    protected virtual void HandleCollision(Collider2D collision)
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

        var tileCollisionPos = CheckTileCollision(collision);
        if (tileCollisionPos != null)
        {
            for (int x = -tilemapDamageRange; x <= tilemapDamageRange; x++)
            {
                for (int y = -tilemapDamageRange; y <= tilemapDamageRange; y++)
                {
                    DamageTile(tileCollisionPos.Value + new Vector3Int(x, y, 0), damage);
                }
            }
        }
            
    }


    [Server]
    protected Vector3Int? CheckTileCollision(Collider2D collision)
    {
        Tilemap collidedTilemap = collision.GetComponentInParent<Tilemap>();
        if (collidedTilemap == null)
            return null;

        GameMapManager manager = GameMapManager.Instance;
        float tileSize = Math.Max(manager.tileSize.x, manager.tileSize.y);

        RaycastHit2D hit = Physics2D.CircleCast((Vector2)transform.position - Direction, 0.5f, Direction, tileSize * 2.0f, LayerMask.GetMask("Tilemap"));
        if (hit.collider == null)
            return null;

        Vector2 worldPosition = hit.point - hit.normal * (tileSize * 0.5f);
        Vector3Int tilePosition = collidedTilemap.WorldToCell(worldPosition);

        if (manager.GetTileType(tilePosition) == TileType.None)
        {
            worldPosition -= (Direction + hit.normal).normalized * (tileSize * 0.5f);
            tilePosition = collidedTilemap.WorldToCell(worldPosition);
        }

        return tilePosition;
    }

    [Server]
    protected void DamageTile(Vector3Int tilePos, int damage)
    {
        if (GameMapManager.Instance.DamageTile(tilePos, damage))
            shootingPlayer.GetComponent<BuildController>().blocksInInventory++;
    }


    [Server]
    void CalculateParticleExplosion()
    {
        Quaternion arcAdjustment = Quaternion.Euler(0, 0, -0.5f * explosionParticleSystemPrefab.GetComponent<ParticleSystem>().shape.arc);
        var origin = detectedCollisionPoint - Direction * 0.5f;
        int layerMask = LayerMask.GetMask("Tilemap", "Player");
        RaycastHit2D hit = Physics2D.Raycast(origin, Direction, 1.0f, layerMask);
        if (!hit)
        {
            explosionParticleOrigin = detectedCollisionPoint;
            explosionParticleDirection = arcAdjustment * Quaternion.FromToRotation(new(1, 0, 0), -Direction);
            SetParticleExplosionParams(explosionParticleOrigin, explosionParticleDirection);
            return;
        }    

        var reflectedDirection = Vector2.Reflect(Direction, hit.normal);
        explosionParticleDirection = arcAdjustment * Quaternion.FromToRotation(new(1, 0, 0), reflectedDirection);
        explosionParticleOrigin = hit.point + hit.normal * 0.25f;
        SetParticleExplosionParams(explosionParticleOrigin, explosionParticleDirection);
    }

    [ClientRpc]
    void SetParticleExplosionParams(Vector2 origin, Quaternion direction)
    {
        collisionDetected = true;
        explosionParticleOrigin = origin;
        explosionParticleDirection = direction;
    }

    public override void OnStopClient()
    {
        // Set parent of the trail to null so that it persists after the projectile is destroyed
        var trail = gameObject.GetComponentInChildren<TrailRenderer>();
        trail.transform.parent = null;
        // Adjust the parameters to create an absorption-like effect
        trail.time = 0.2f;
        trail.widthMultiplier *= 0.3f;
        trail.widthCurve = new AnimationCurve(new(0.0f, 1.0f), new(1.0f, 0.0f));

        // Emit explosion particles
        if (!collisionDetected)
            return;

        GameObject particleSystem = Instantiate(explosionParticleSystemPrefab, explosionParticleOrigin, explosionParticleDirection);

        var playerController = shootingPlayer.GetComponent<PlayerController>();
        var particleSettings = particleSystem.GetComponent<ParticleSystem>().main;
        particleSettings.startSpeedMultiplier = explosionParticleSpeedCoefficient * speed;

        var particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material.SetColor("_Color", playerController.secondaryColor);
        particleRenderer.trailMaterial.SetColor("_Color1", playerController.secondaryColor);
        particleRenderer.trailMaterial.SetColor("_Color2", playerController.ternaryColor);

        Destroy(particleSystem, particleSettings.startLifetimeMultiplier);
    }
}
