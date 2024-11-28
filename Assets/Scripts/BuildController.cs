using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildController : NetworkBehaviour
{ 
    private Camera cam;
    private Vector3 mousePos, blockPos;
    readonly float blockPlaceTime = 0f;
    bool modifingBlock = false;

    public GameMapManager gameMapManager;

    void Start()
    {
        if (!isLocalPlayer)
            return;
        cam = GetComponentInChildren<Camera>();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;
        ProcessBlockPlacing();
    }

    [Client]
    private void ProcessBlockPlacing()
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        blockPos.y = Mathf.Round(mousePos.y - .5f);
        blockPos.x = Mathf.Round(mousePos.x - .5f);
        if (Input.GetKey(KeyCode.Mouse0) && !modifingBlock)
        {
            modifingBlock = true;
            StartCoroutine(PlaceBlock(blockPos));
        }
        else if (Input.GetKey(KeyCode.Mouse1) && !modifingBlock)
        {
            modifingBlock = true;
            StartCoroutine(DestroyBlock(blockPos));
        }
    }

    [Client]
    IEnumerator PlaceBlock(Vector2 pos)
    {
        yield return new WaitForSeconds(blockPlaceTime);
        CmdRequestPlaceTile(new Vector3Int((int)pos.x, (int)pos.y, 0), TileType.Grass);
        modifingBlock = false;
    }

    [Client]
    IEnumerator DestroyBlock(Vector2 pos)
    {
        yield return new WaitForSeconds(blockPlaceTime);
        CmdRequestDestroyTile(new Vector3Int((int)pos.x, (int)pos.y, 0));
        modifingBlock = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(blockPos, blockPos + new Vector3(1, 0, 0));
        Gizmos.DrawLine(blockPos, blockPos + new Vector3(0, 1, 0));
        Gizmos.DrawLine(blockPos + new Vector3(1, 0, 0), blockPos + new Vector3(1, 1, 0));
        Gizmos.DrawLine(blockPos + new Vector3(0, 1, 0), blockPos + new Vector3(1, 1, 0));
    }

    [Command]
    private void CmdRequestPlaceTile(Vector3Int position, TileType type)
    {
        gameMapManager.TryBuildTile(position, type);
    }

    [Command]
    private void CmdRequestDestroyTile(Vector3Int position)
    {
        gameMapManager.TryDestroyTile(position);
    }
}
