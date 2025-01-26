using Mirror;
using Mirror.Discovery;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameNetworkManager : NetworkRoomManager
{
    public GameObject mapPrefab;
    public GameMapManager gameMapManager;

    public GameObject[] playerPrefabVariants;
    public Texture2D[] playerTextures;

    public List<NetworkConnectionToClient> alivePlayers = new List<NetworkConnectionToClient>();
    public List<NetworkConnectionToClient> deadPlayers = new List<NetworkConnectionToClient>();
    public Dictionary<NetworkConnectionToClient, Tuple<string, int>> playersProperties = new Dictionary<NetworkConnectionToClient, Tuple<string, int>>();

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
        playersProperties.Add(conn, new Tuple<string, int>(roomPlayerComponent.Name, roomPlayerComponent.ColorID));
        int playerIndex = roomPlayerComponent.ColorID % playerPrefabVariants.Length;
        Transform startPos = GetStartPosition();
        alivePlayers.Add(conn);
        return startPos != null
            ? Instantiate(playerPrefabVariants[roomPlayerComponent.ColorID], startPos.position, startPos.rotation)
            : Instantiate(playerPrefabVariants[roomPlayerComponent.ColorID], Vector3.zero, Quaternion.identity);
    }

    public override void OnGUI()
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
        buttonStyle.normal.textColor = Color.black;
        buttonStyle.hover.textColor = Color.black;
        buttonStyle.active.textColor = Color.black;
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.9607844f, 0.7294118f, 0.9215687f));
        buttonStyle.hover.background = MakeTex(2, 2, new Color(1f, 0.8820755f, 0.980345f));
        buttonStyle.active.background = MakeTex(2, 2, new Color(0.9811321f, 0.513706f, 0.9019072f));
        buttonStyle.alignment = TextAnchor.MiddleCenter;

        if (Utils.IsSceneActive(GameplayScene))
        {
            GUILayout.BeginArea(new Rect(Screen.width - 150f, 10f, 140f, 30f));
            if (NetworkServer.active)
            {
                if (GUILayout.Button("Return to Room", buttonStyle))
                    ServerChangeScene(RoomScene);
            }
            else
            {
                if (GUILayout.Button("Leave", buttonStyle))
                    StopClient();
            }
            GUILayout.EndArea();
        }

        if (Utils.IsSceneActive(RoomScene))
        {
            GUILayout.BeginArea(new Rect(Screen.width - 400f, 10f, 390f, 60f));
            buttonStyle.fontSize = 30;
            if (GUILayout.Button("Return to main menu", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                ReturnToMainMenu();
            }
            GUILayout.EndArea();
        }
    }

    public void ReturnToMainMenu()
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
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    [Server]
    public void OnPlayerKilled(NetworkConnectionToClient conn)
    {
        alivePlayers.Remove(conn);
        deadPlayers.Add(conn);

        CheckGameEnd();
    }

    [Server]
    private void CheckGameEnd()
    {
        if(alivePlayers.Count == 1)
        {
            ServerChangeScene(RoomScene);
            string[] orderedPlayersNames = new string[alivePlayers.Count + deadPlayers.Count];
            int[] orderedPlayersColorID = new int[alivePlayers.Count + deadPlayers.Count];
            orderedPlayersNames[0] = playersProperties[alivePlayers[0]].Item1;
            orderedPlayersColorID[0] = playersProperties[alivePlayers[0]].Item2;
            for (int i = 0; i < deadPlayers.Count; i++)
            {
                orderedPlayersNames[i + 1] = playersProperties[deadPlayers[i]].Item1;
                orderedPlayersColorID[i + 1] = playersProperties[deadPlayers[i]].Item2;
            }
            for (int i = deadPlayers.Count - 1; i >= 0; i--)
            {
                deadPlayers[i].identity.gameObject.GetComponent<RoomPlayer>().showResults = true;
                deadPlayers[i].identity.gameObject.GetComponent<RoomPlayer>().orderedPlayersNames = orderedPlayersNames;
                deadPlayers[i].identity.gameObject.GetComponent<RoomPlayer>().orderedPlayersColorID = orderedPlayersColorID;
            }
            alivePlayers[0].identity.gameObject.GetComponent<RoomPlayer>().showResults = true;
            alivePlayers[0].identity.gameObject.GetComponent<RoomPlayer>().orderedPlayersNames = orderedPlayersNames;
            alivePlayers[0].identity.gameObject.GetComponent<RoomPlayer>().orderedPlayersColorID = orderedPlayersColorID;
            alivePlayers.Clear();
            deadPlayers.Clear();
            playersProperties.Clear();
        }
    }
}
