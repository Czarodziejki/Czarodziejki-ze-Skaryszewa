using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameNetworkManager : NetworkRoomManager
{
    public GameObject mapPrefab;
    public GameMapManager gameMapManager;

    public GameObject[] playerPrefabVariants;
    public Texture2D[] playerTextures;

    private void SpawnMap()
    {
        GameObject mapInstance = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(mapInstance);
        gameMapManager = mapInstance.GetComponent<GameMapManager>();
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        base.OnRoomServerSceneChanged(sceneName);
        if(sceneName == GameplayScene)
            SpawnMap();
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        RoomPlayer roomPlayerComponent = roomPlayer.GetComponent<RoomPlayer>();
        int playerIndex = roomPlayerComponent.ColorID % playerPrefabVariants.Length;
        Transform startPos = GetStartPosition();
        return startPos != null
            ? Instantiate(playerPrefabVariants[playerIndex], startPos.position, startPos.rotation)
            : Instantiate(playerPrefabVariants[playerIndex], Vector3.zero, Quaternion.identity);
    }

    public override void OnGUI()
    {
        if (Utils.IsSceneActive(GameplayScene))
        {
            GUILayout.BeginArea(new Rect(Screen.width - 150f, 10f, 140f, 30f));
            if (NetworkServer.active)
            {
                if (GUILayout.Button("Return to Room"))
                    ServerChangeScene(RoomScene);
            }
            else
            {
                if (GUILayout.Button("Leave"))
                    StopClient();
            }
            GUILayout.EndArea();
        }

        if (Utils.IsSceneActive(RoomScene))
        {
            GUILayout.BeginArea(new Rect(Screen.width - 150f, 10f, 140f, 30f));
            if (GUILayout.Button("Return to main menu"))
            {
                if (NetworkServer.active && NetworkClient.isConnected)
                {
                    StopHost();
                }
                else if (NetworkClient.isConnected)
                {
                    StopClient();
                }
            }
            GUILayout.EndArea();
        }
    }
}
