using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Mirror;
using static UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty;

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
    private Tilemap tilemap;
    private Dictionary<TileType, TileBase> tileDictionary;
    private Vector2 tileSize = new Vector2(1.0f, 1.0f);

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        tileDictionary = new Dictionary<TileType, TileBase>
        {
            { TileType.Grass, grassTile },
            { TileType.Unbreakable, unbreakableTile }
        };

        tilemap = GetComponentInChildren<Tilemap>();
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
    public void TryDestroyTile(Vector3Int position)
    {
        if (GetTileType(position) == TileType.Unbreakable) return;
        tilemap.SetTile(position, null);
        RpcDestroyTile(position);
    }

    [Server]
    public void TryBuildTile(Vector3Int position, TileType type)
    {
        if (IsValidTileBuilPosition(position))
        {
            tilemap.SetTile(position, GetTile(type));
            RpcBuildTile(position, type);
        }
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
    private bool IsValidTileBuilPosition(Vector3Int position)
    {
        if (tilemap.GetTile(position) != null)
            return false;

        int layerMask = LayerMask.GetMask("Player");
        bool isOccupied = Physics2D.OverlapBox(tilemap.GetCellCenterWorld(position), tileSize, 0f, layerMask);
        return !isOccupied;
    }


}
