using Mirror;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;


public class CrackingController : NetworkBehaviour
{
    public TileBase[] crackLevels;

    public Tilemap cracksTilemap;

    [Server]
    public void SetDestructionLevel(Vector3Int position, float destructionLevel)
    {
        var actTile = cracksTilemap.GetTile(position);
        var (newTile, index) = GetTile(destructionLevel);

        if (actTile != newTile)
        {
            cracksTilemap.SetTile(position, newTile);
            RpcSetTile(position, index);
        }
    }

    [Server]
    public void DeleteCracks(Vector3Int position)
    {
        if (cracksTilemap.GetTile(position) == null)
            return;

        cracksTilemap.SetTile(position, null);
        RpcSetTile(position, -1);
    }

    [ClientRpc]
    private void RpcSetTile(Vector3Int position, int index)
    {
        if (index < 0)
        {
            cracksTilemap.SetTile(position, null);
            return;
        }
            

        cracksTilemap.SetTile(position, crackLevels[index]);
    }

    private (TileBase, int) GetTile(float destructionLevel)
    {
        destructionLevel = Math.Clamp(destructionLevel, 0f, 1f);

        float scaledDestLvl = destructionLevel * (crackLevels.Length);
        int index = (int)MathF.Floor(scaledDestLvl);

        if (index <= 0)
            return (null, -1);

        index -= 1;
        return (crackLevels[index], index);
    }
}
