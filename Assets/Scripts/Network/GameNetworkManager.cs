using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameNetworkManager : NetworkManager
{
    public GameObject mapPrefab;

    public GameMapManager gameMapManager;
    public override void OnStartServer()
    {
        SpawnMap();
    }


    private void SpawnMap()
    {
        GameObject mapInstance = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(mapInstance);
        gameMapManager = mapInstance.GetComponent<GameMapManager>();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log($"Trying to instantiate player {conn.connectionId}");
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        Debug.Log($"Instatiating player {player.name}");
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
