using System.Collections;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public class BuildController : MonoBehaviour
{
    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public Camera cam;
    private Vector3 mousePos, blockPos;

    public RuleTile grassTile;
    public Tilemap groundTileMap;

    public LayerMask layer;

    readonly float blockPlaceTime = 0f;

    bool placingBlock = false;

    private void FixedUpdate()
    {
        ProcessBlockPlacing();
    }

    private void ProcessBlockPlacing()
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        blockPos.y = Mathf.Round(mousePos.y - .5f);
        blockPos.x = Mathf.Round(mousePos.x - .5f);

        bool noBlock = groundTileMap.GetTile(new Vector3Int((int)blockPos.x, (int)blockPos.y, 0)) == null;
        bool insidePlayer = Physics2D.OverlapBox(new Vector2(blockPos.x + .5f, blockPos.y + .5f), new Vector2(.5f, .5f), 0, layer);

        if (Input.GetKey(KeyCode.Mouse1) && noBlock && !placingBlock && !insidePlayer)
        {
            placingBlock = true;
            StartCoroutine(PlaceBlock(groundTileMap, blockPos));
        }
    }

    IEnumerator PlaceBlock(Tilemap map, Vector2 pos)
    {
        yield return new WaitForSeconds(blockPlaceTime);

        map.SetTile(new Vector3Int((int)pos.x, (int)pos.y, 0), grassTile);

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
}
