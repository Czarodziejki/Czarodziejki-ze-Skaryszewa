using Mirror;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildController : NetworkBehaviour
{ 
    private Camera cam;
    private Vector3 mousePos, blockPos;
    readonly float blockPlaceTime = 0f;
    bool modifyingBlock = false;
    private bool inRange = false;
    private float tileBuildRadius;
    [SyncVar(hook= nameof(OnClientInventoryUpdate))]
    public uint blocksInInventory;

    void Start()
    {
        if (!isLocalPlayer) return;
        cam = GetComponentInChildren<Camera>();
        tileBuildRadius = GameMapManager.Instance.tileBuildRadius;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        ProcessBlockPlacing();
    }

    [Client]
    private void OnClientInventoryUpdate(uint oldBlocks, uint newBlocks)
    {
        if (!isLocalPlayer) return;
        Debug.Log("Inventory: " + newBlocks);
    }

    [Client]
    private void ProcessBlockPlacing()
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        blockPos.y = Mathf.Round(mousePos.y - .5f);
        blockPos.x = Mathf.Round(mousePos.x - .5f);
        inRange = Vector3.Distance(transform.position, blockPos) <= tileBuildRadius;
        if (Input.GetKey(KeyCode.Mouse1) && !modifyingBlock && inRange)
        {
            modifyingBlock = true;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                StartCoroutine(DestroyBlock(blockPos));
            }
            else
            {
                StartCoroutine(PlaceBlock(blockPos));
            }
        }
    }

    [Client]
    IEnumerator PlaceBlock(Vector2 pos)
    {
        yield return new WaitForSeconds(blockPlaceTime);
        CmdRequestPlaceTile(new Vector3Int((int)pos.x, (int)pos.y, 0), TileType.Grass);
        modifyingBlock = false;
    }

    [Client]
    IEnumerator DestroyBlock(Vector2 pos)
    {
        yield return new WaitForSeconds(blockPlaceTime);
        CmdRequestDestroyTile(new Vector3Int((int)pos.x, (int)pos.y, 0));
        modifyingBlock = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = inRange ? Color.white : Color.red;
        Gizmos.DrawLine(blockPos, blockPos + new Vector3(1, 0, 0));
        Gizmos.DrawLine(blockPos, blockPos + new Vector3(0, 1, 0));
        Gizmos.DrawLine(blockPos + new Vector3(1, 0, 0), blockPos + new Vector3(1, 1, 0));
        Gizmos.DrawLine(blockPos + new Vector3(0, 1, 0), blockPos + new Vector3(1, 1, 0));
    }

    [Command]
    private void CmdRequestPlaceTile(Vector3Int position, TileType type)
    {
        bool inRange = OnServerIsInRange(position);
        bool hasBlocks = blocksInInventory > 0;
        if (inRange && hasBlocks && GameMapManager.Instance.TryBuildTile(position, type))
        {
            blocksInInventory--;
        }
    }

    [Command]
    private void CmdRequestDestroyTile(Vector3Int position)
    {
        bool inRange = OnServerIsInRange(position);
        if (inRange && GameMapManager.Instance.TryDestroyTile(position))
        {
            blocksInInventory++;
        }
    }

    [Server]
    private bool OnServerIsInRange(Vector3Int position)
    {
        return Vector3.Distance(transform.position, position) <= GameMapManager.Instance.tileBuildRadius;
    }
}
