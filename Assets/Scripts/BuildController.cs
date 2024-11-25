using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildController : NetworkBehaviour
{ 
    public Camera cam;
    private Vector3 mousePos, blockPos;
    public GameMapManager mapManager;

    public LayerMask layer;

    readonly float blockPlaceTime = 0f;

    bool placingBlock = false;

    void Start()
    {
        if (!isLocalPlayer)
            return;
        cam = GetComponentInChildren<Camera>();
        Debug.Log("BuilControllerStart");
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
        if (Input.GetKey(KeyCode.Mouse1) && !placingBlock)
        {
            placingBlock = true;
            StartCoroutine(PlaceBlock(blockPos));
        }
    }

    [Client]
    IEnumerator PlaceBlock(Vector2 pos)
    {
        yield return new WaitForSeconds(blockPlaceTime);
        CmdRequestTileChange(new Vector3Int((int)pos.x, (int)pos.y, 0), TileID.Grass);
        placingBlock = false;
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
    private void CmdRequestTileChange(Vector3Int position, TileID tileID)
    {
        // Delegate tile placement to the server's GameMapManager
        mapManager.SetTile(position, tileID);
    }
}
