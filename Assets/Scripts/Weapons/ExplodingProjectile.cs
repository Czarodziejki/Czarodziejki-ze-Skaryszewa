using Mirror;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;


public class ExplodingProjectile : Projectile
{
    public int range;
    public float distanceMultiplier;


    [Server]
    protected override void HandleCollision(Collider2D collision)
    {
        foreach (var connection in NetworkServer.connections.Values)
        {
            int damage = CalculateDamage(connection.identity.transform.position);
            if (damage > 0)
            {
                var healthController = connection.identity.GetComponent<HealthController>();
                healthController.Damage(damage);
            }
        }

        var tileCollisionPos = CheckTileCollision(collision);
        if (tileCollisionPos == null)
            return;

        Tilemap collidedTilemap = collision.GetComponentInParent<Tilemap>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int actTilePos = tileCollisionPos.Value + new Vector3Int(x, y, 0);
                Vector3 worldTilePos = collidedTilemap.CellToWorld(actTilePos);

                int damage = CalculateDamage(worldTilePos);

                DamageTile(actTilePos, damage);
            }
        }
        
    }


    int CalculateDamage(Vector3 pos)
    {
        float sqrDist = (pos - gameObject.transform.position).sqrMagnitude;
        sqrDist *= distanceMultiplier;
        sqrDist = Math.Max(1.0f, sqrDist);

        return (int)(damage / sqrDist);
    }
}
