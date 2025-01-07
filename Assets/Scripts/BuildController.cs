using Mirror;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System;

public class BuildController : NetworkBehaviour
{ 
    private Camera cam;
    private Vector3 mousePos, blockPos, startBlockPos;
    private bool inRange = false;
    private float tileBuildRadius;
    [SyncVar(hook= nameof(OnClientInventoryUpdate))]
    public uint blocksInInventory;
    private GameObject inventoryUIElement;
    private TMP_Text inventoryCountUIElement;
    private InputAction buildAction, modifierAction;
    private bool buildHeld, modifierHeld;

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
        buildAction.performed += OnBuildPress;
        buildAction.canceled += OnBuildRelease;
        buildHeld = buildAction.IsPressed();
        if (buildHeld) startBlockPos = UpdateBlockPos();

        modifierAction = InputSystem.actions.FindAction("Modifier");
        modifierAction.performed += OnModifierPress;
        modifierAction.canceled += OnModifierRelease;
        modifierHeld = modifierAction.IsPressed();
    }

    [Client]
    private void OnModifierRelease(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (!modifierHeld)
        {
            Debug.Log("OnModifierRelease called while modifierHeld is already false");
            return;
        }
        modifierHeld = false;

        if (!buildHeld) return;

        DrawBlockLine(startBlockPos, UpdateBlockPos(), true);
        startBlockPos = blockPos;
    }

    [Client]
    private void OnModifierPress(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (modifierHeld)
        {
            Debug.Log("OnModifierPress called while modifierHeld is already true");
            return;
        }
        modifierHeld = true;

        if (!buildHeld) return;

        DrawBlockLine(startBlockPos, UpdateBlockPos(), false);
        startBlockPos = blockPos;
    }

    [Client]
    private void OnBuildRelease(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (!buildHeld)
        {
            Debug.Log("OnBuildRelease called while buildHeld is already false");
            return;
        }
        buildHeld = false;

        DrawBlockLine(startBlockPos, UpdateBlockPos(), modifierHeld);
    }

    [Client]
    private void OnBuildPress(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (buildHeld)
        {
            Debug.Log("OnBuildPress called while buildHeld is already true");
            return;
        }
        buildHeld = true;

        startBlockPos = UpdateBlockPos();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        UpdateBlockPos();
        if (buildHeld)
        {
            DrawBlockLine(startBlockPos, blockPos, modifierHeld);
            startBlockPos = blockPos;
        }
    }

    [Client]
    private void OnClientInventoryUpdate(uint oldBlocks, uint newBlocks)
    {
        if (!isLocalPlayer) return;
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
    private Vector3 UpdateBlockPos()
    {
        mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        blockPos.y = Mathf.Round(mousePos.y - .5f);
        blockPos.x = Mathf.Round(mousePos.x - .5f);
        inRange = Vector3.Distance(transform.position, blockPos) <= tileBuildRadius;
        return blockPos;
    }

    [Client]
    private void DrawBlockLine(Vector3 start, Vector3 end, bool destroy = false)
    {
        int x0 = (int)start.x;
        int y0 = (int)start.y;
        int x1 = (int)end.x;
        int y1 = (int)end.y;

        if (x0 == x1 && y0 == y1)
        {
            ModifyBlock(x0, y0, destroy);
            return;
        }

        if (Math.Abs(y1 - y0) < Math.Abs(x1 - x0))
        {
            if (x0 > x1)
            {
                DrawBlockLineLow(x1, y1, x0, y0, destroy);
            }
            else
            {
                DrawBlockLineLow(x0, y0, x1, y1, destroy);
            }
        }
        else
        {
            if (y0 > y1)
            {
                DrawBlockLineHigh(x1, y1, x0, y0, destroy);
            }
            else
            {
                DrawBlockLineHigh(x0, y0, x1, y1, destroy);
            }
        }
    }

    [Client]
    private void DrawBlockLineLow(int x0, int y0, int x1, int y1, bool destroy = false)
    {
        int dx = x1 - x0;
        int dy = y1 - y0;
        int yi = 1;

        if (dy < 0)
        {
            yi = -1;
            dy = -dy;
        }

        int D = 2 * dy - dx;
        int y = y0;

        for (int x = x0; x <= x1; x++)
        {
            ModifyBlock(x, y, destroy);
            if (D > 0)
            {
                y += yi;
                D -= 2 * dx;
            }
            D += 2 * dy;
        }
    }

    [Client]
    private void DrawBlockLineHigh(int x0, int y0, int x1, int y1, bool destroy = false)
    {
        int dx = x1 - x0;
        int dy = y1 - y0;
        int xi = 1;

        if (dx < 0)
        {
            xi = -1;
            dx = -dx;
        }

        int D = 2 * dx - dy;
        int x = x0;

        for (int y = y0; y <= y1; y++)
        {
            ModifyBlock(x, y, destroy);
            if (D > 0)
            {
                x += xi;
                D -= 2 * dy;
            }
            D += 2 * dx;
        }
    }

    [Client]
    void ModifyBlock(int x, int y, bool destroy = false)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);
        if (destroy)
        {
            CmdRequestDestroyTile(pos);
        }
        else if (GameMapManager.Instance.OnClientIsValidBuildPosition(pos))
        {
            CmdRequestPlaceTile(pos, TileType.Breakable);
        }
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
