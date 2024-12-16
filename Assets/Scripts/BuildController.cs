using Mirror;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

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
    private GameObject inventoryUIElement;
    private TMP_Text inventoryCountUIElement;
    private InputAction buildAction, modifierAction;

    void Start()
    {
        if (!isLocalPlayer) return;
        cam = GetComponentInChildren<Camera>();
        tileBuildRadius = GameMapManager.Instance.tileBuildRadius;

        inventoryUIElement = GameObject.Find("InventoryUIElement");
        // InventoryUIElement is an empty object containg two objects: GrassBlockIcon (Image) and GrassBlockCount(TextMeshPro)
        inventoryCountUIElement = inventoryUIElement.GetComponentInChildren<TMP_Text>();
        inventoryCountUIElement.text = blocksInInventory.ToString();

        buildAction = InputSystem.actions.FindAction("Build");
        modifierAction = InputSystem.actions.FindAction("Modifier");
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        ProcessBlockPlacing();
    }

    [Client]
    private void OnClientInventoryUpdate(uint oldBlocks, uint newBlocks)
    {
        if (!isLocalPlayer) return;
        Debug.Log("Inventory: " + newBlocks);
        inventoryCountUIElement.text = newBlocks.ToString();
        if (newBlocks == 0)
        {
            // find a subobject GrassBlockIcon in InventoryUIElement and gray it out
            inventoryUIElement.GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
        else if (oldBlocks == 0)
        {
            // undo the graying out
            inventoryUIElement.GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f, 1f);
        }
    }

    [Client]
    private void ProcessBlockPlacing()
    {
        mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        blockPos.y = Mathf.Round(mousePos.y - .5f);
        blockPos.x = Mathf.Round(mousePos.x - .5f);
        inRange = Vector3.Distance(transform.position, blockPos) <= tileBuildRadius;
        if (buildAction.IsPressed() && !modifyingBlock && inRange)
        {
            modifyingBlock = true;
            if (modifierAction.IsPressed())
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
        Color color = inRange ? Color.white : Color.red;
        if (blocksInInventory == 0)
        {
            color.a = 0.1f;
        }
        Gizmos.color = color;
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
