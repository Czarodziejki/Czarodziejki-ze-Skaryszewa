using Mirror;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System;

public class BuildController : NetworkBehaviour
{ 
    private Camera cam;
    private Vector3 mousePos, blockPos, lineStartPos;
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
        if (buildHeld) lineStartPos = UpdateMousePos();

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

        DrawBlockLine(lineStartPos, UpdateMousePos(), true);
        lineStartPos = mousePos;
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

        DrawBlockLine(lineStartPos, UpdateMousePos(), false);
        lineStartPos = mousePos;
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

        DrawBlockLine(lineStartPos, UpdateMousePos(), modifierHeld);
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

        lineStartPos = UpdateMousePos();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        UpdateMousePos();
        if (buildHeld)
        {
            DrawBlockLine(lineStartPos, mousePos, modifierHeld);
            lineStartPos = mousePos;
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

    //[Client]
    //private void ProcessBlockPlacing()
    //{
    //    UpdateMousePos();
    //    if (buildAction.IsPressed() && inRange)
    //    {
    //        if (modifierAction.IsPressed())
    //        {
    //            StartCoroutine(DestroyBlock(blockPos));
    //        }
    //        else
    //        {
    //            StartCoroutine(PlaceBlock(blockPos));
    //        }
    //    }
    //}

    [Client]
    private Vector3 UpdateMousePos()
    {
        mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        blockPos.y = Mathf.Round(mousePos.y - .5f);
        blockPos.x = Mathf.Round(mousePos.x - .5f);
        inRange = Vector3.Distance(transform.position, blockPos) <= tileBuildRadius;
        return mousePos;
    }

    [Client]
    private void DrawBlockLine(Vector3 start, Vector3 end, bool destroy = false)
    {
        // TODO: Implement Bresenham's line algorithm
        if (destroy)
        {
            DestroyBlock(end);
        }
        else
        {
            PlaceBlock(end);
        }
    }

    [Client]
    void PlaceBlock(Vector2 pos)
    {
        CmdRequestPlaceTile(new Vector3Int((int)pos.x, (int)pos.y, 0), TileType.Grass);
    }

    [Client]
    void DestroyBlock(Vector2 pos)
    {
        CmdRequestDestroyTile(new Vector3Int((int)pos.x, (int)pos.y, 0));
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
