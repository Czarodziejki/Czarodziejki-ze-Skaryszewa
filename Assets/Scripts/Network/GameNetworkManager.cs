using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameNetworkManager : NetworkManager
{
    public GameObject mapPrefab;

    private GameObject mapInstance;
    private GameMapManager mapManager;

    public override void OnStartServer()
    {
        SpawnMap(); 
    }

    [Server]
    private void SpawnMap()
    {
        if (mapPrefab == null)
        {
            Debug.LogError("Map prefab is not assigned!");
            return;
        }

        // Spawn the map on the server
        mapInstance = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(mapInstance);

        // Cache the TilemapManager for shared access
        mapManager = mapInstance.GetComponent<GameMapManager>();

        Debug.Log("Map spawned and ready!");
    }


    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log($"Trying to instantiate player {conn.connectionId}");
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        Debug.Log($"Instatiating player {player.name}");
        player.GetComponent<BuildController>().mapManager = mapManager;
        mapManager.SyncTilemapToNewPlayer(conn);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
