using Mirror;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;


public class CrackingController : NetworkBehaviour
{
    public TileBase[] crackLevels;

    public Tilemap cracksTilemap;

    [Server]
    public void SetCracksLevel(Vector3Int position, float cracksLevel)
    {
        var actTile = cracksTilemap.GetTile(position);
        var (newTile, index) = GetTile(cracksLevel);

        if (actTile != newTile)
        {
            Debug.LogError("New cracks");
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
        int index = (int)MathF.Ceiling(scaledDestLvl);
        //index = Math.Min(index, crackLevels.Length - 1);

        if (index == 0)
            return (null, -1);

        --index;
        Debug.LogError("Destruction index: " + index);
        return (crackLevels[index], index);
    }
}
