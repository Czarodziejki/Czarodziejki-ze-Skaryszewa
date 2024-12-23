using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using System;


public enum TileType : int
{
    None,
    Grass,
    Unbreakable
}

public class GameMapManager : NetworkBehaviour
{
    public static GameMapManager Instance;
    public TileBase grassTile, unbreakableTile;
    public int grassMaxHealth = 20;

    public Tilemap tilemap;

    private Dictionary<TileType, TileBase> tileDictionary;
    private Dictionary<TileType, int> tileMaxHealth;

    // Stores only damaged tiles
    private Dictionary<Vector3Int, int> tilesHealthPoints;

    private Vector2 tileSize = new Vector2(1.0f, 1.0f);
    public float tileBuildRadius;
    private CrackingController crackingController;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;

        tileDictionary = new Dictionary<TileType, TileBase>
        {
            { TileType.Grass, grassTile },
            { TileType.Unbreakable, unbreakableTile }
        };

        tileMaxHealth = new Dictionary<TileType, int>
        {
            { TileType.Grass, grassMaxHealth }
        };

        tilesHealthPoints = new Dictionary<Vector3Int, int>();
        crackingController = GetComponent<CrackingController>();
    }

    private TileBase GetTile(TileType tileType)
    {
        return tileDictionary.TryGetValue(tileType, out TileBase tile) ? tile : null;
    }

    public TileType GetTileType(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);
        foreach (var pair in tileDictionary)
        {
            if (pair.Value == tile)
                return pair.Key;
        }
        return TileType.None;
    }

    [Server]
    public bool TryDestroyTile(Vector3Int position)
    {
        if (GetTileType(position) != TileType.Grass) return false;
        tilemap.SetTile(position, null);
        tilesHealthPoints.Remove(position);
        crackingController.DeleteCracks(position);
        RpcDestroyTile(position);
        return true;
    }

    [Server]
    public bool TryBuildTile(Vector3Int position, TileType type)
    {
        if (IsValidTileBuildPosition(position))
        {
            tilemap.SetTile(position, GetTile(type));
            RpcBuildTile(position, type);
            return true;
        }
        return false;
    }

    [Server]
    public bool DamageTile(Vector3Int position, int damage)
    {
        TileType type = GetTileType(position);

        if (!tilesHealthPoints.ContainsKey(position))
        {
            if (!tileMaxHealth.ContainsKey(type))
                return false;   // Tile cannot be damaged

            int newHealth = math.max(tileMaxHealth[type] - damage, 0);

            if (newHealth > 0)
            {
                tilesHealthPoints.Add(position, newHealth);
                int maxHealth = tileMaxHealth[type];
                crackingController.SetCracksLevel(position, (float)(maxHealth - newHealth) / (float)maxHealth);
                return false;
            }
                
            return TryDestroyTile(position);
        }

        int prevHealth = tilesHealthPoints[position];
        int actHealth = math.max(prevHealth - damage, 0);

        if (actHealth > 0)
        {
            tilesHealthPoints[position] = actHealth;
            int maxHealth = tileMaxHealth[type];
            crackingController.SetCracksLevel(position, (float)(maxHealth - actHealth) / (float)maxHealth);
            return false;
        }
            
        return TryDestroyTile(position);
    }

    [ClientRpc]
    private void RpcDestroyTile(Vector3Int position)
    {
        tilemap.SetTile(position, null);
    }

    [ClientRpc]
    private void RpcBuildTile(Vector3Int position, TileType type)
    {
        tilemap.SetTile(position, GetTile(type));
    }


    [Server]
    private bool IsValidTileBuildPosition(Vector3Int position)
    {
        if (tilemap.GetTile(position) != null)
            return false;

        int layerMask = LayerMask.GetMask("Player");
        bool isOccupied = Physics2D.OverlapBox(tilemap.GetCellCenterWorld(position), tileSize, 0f, layerMask);
        return !isOccupied;
    }

    
}
