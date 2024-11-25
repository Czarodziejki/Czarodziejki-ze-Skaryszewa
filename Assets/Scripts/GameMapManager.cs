using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.WSA;
using System;
using UnityEngine.UIElements;

public enum TileID : int
{
    None = 0,
    Grass = 1,
    TilesSize
}

public class GameMapManager : NetworkBehaviour
{
    public Tilemap tilemap;

    public RuleTile grassTilePrefab;

    private LayerMask playerLayer;

    void Start()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap is not assigned!");
            return;
        }

        Vector3Int tilemapSize = tilemap.cellBounds.size;
    }

    [Server]
    public void SetTile(Vector3Int position, TileID tileID)
    {
        tilemap.SetTile(position, grassTilePrefab);

        // Optionally synchronize with all clients
        RpcSetTile(position, tileID);
    }

    [ClientRpc]
    private void RpcSetTile(Vector3Int position, TileID tileID)
    {
        // Set the tile on the local copy of the tilemap
        tilemap.SetTile(position, grassTilePrefab);
    }

    [TargetRpc]
    private void TargetSetTile(NetworkConnection conn, Vector3Int position, TileID tileID)
    {
        // Set the tile on the local copy of the tilemap
        if (!isServer && conn == NetworkClient.connection)
        {
            tilemap.SetTile(position, grassTilePrefab);
        }
    }

    [Server]
    private bool IsValidTileChange(Vector3Int position)
    {
        bool noBlock = tilemap.GetTile(position) == null;
        Vector3 worldPosition = tilemap.tileAnchor + tilemap.CellToWorld(position);
        Vector3 tileSize = tilemap.cellSize;
        bool insidePlayer = Physics2D.OverlapBox(new Vector2(worldPosition.x, worldPosition.y), new Vector2(tileSize.x, tileSize.y), 0, LayerMask.NameToLayer("Player"));
        return noBlock && !insidePlayer;
    }

    RuleTile GetTileByID(TileID tileID)
    {
        switch (tileID)
        {
            case TileID.None: return null;
            case TileID.Grass: return grassTilePrefab;
            default: return null;
        }
    }

    TileID GetTileID(TileBase tile)
    {
        if (tile == grassTilePrefab)
            return TileID.Grass;
        else
            return TileID.None;
    }

    [Server]
    public void SyncTilemapToNewPlayer(NetworkConnection conn)
    {
        for (int x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++)
        {
            for (int y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++)
            {
                for (int z = tilemap.cellBounds.min.z; z < tilemap.cellBounds.max.z; z++)
                {

                    TargetSetTile(conn, new Vector3Int(x, y, z), GetTileID(tilemap.GetTile(new Vector3Int(x, y, z))));
                }
            }
        }
    }
}
