using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameNetworkManager : NetworkRoomManager
{
    public GameObject[] avaiableMaps;

    private GameObject mapPrefab = null;
    public GameMapManager gameMapManager;

    public GameObject[] playerPrefabVariants;
    public Texture2D[] playerTextures;
    public string[] playerNames;

    public List<NetworkConnectionToClient> alivePlayers = new List<NetworkConnectionToClient>();
    public List<NetworkConnectionToClient> deadPlayers = new List<NetworkConnectionToClient>();
    public Dictionary<NetworkConnectionToClient, int> playersVariants = new Dictionary<NetworkConnectionToClient, int>();

    private bool[] variantAvaliable = Enumerable.Repeat(true, 4).ToArray();

    public void ReserveVariant(NetworkConnectionToClient conn, int variantId)
    {
        variantAvaliable[variantId] = false;
        playersVariants[conn] = variantId;
        if (variantAvaliable.Count(v => v == false) == roomSlots.Count)
        {
            foreach (var player in roomSlots)
                player.gameObject.GetComponent<RoomPlayer>().displayType = RoomPlayer.DisplayType.DisplayMapSelection;
        }
    }
    public void FreeVariant(NetworkConnectionToClient conn, int variantId)
    {
        variantAvaliable[variantId] = true;
        playersVariants.Remove(conn);
    }

    public bool IsVariantAvaliable(int variantId)
    {
        return variantAvaliable[variantId];
    }

    public void SelectMap(GameObject map)
    {
        mapPrefab = map;
    }


    public void StartGame()
    {
        allPlayersReady = true;
    }


    private void SpawnMap()
    {
        GameObject mapInstance = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(mapInstance);
        gameMapManager = mapInstance.GetComponent<GameMapManager>();
    }


    public override void OnServerSceneChanged(string sceneName)
    {
        if(sceneName == GameplayScene)
            SpawnMap();
        else if (sceneName == RoomScene)
        {
            foreach (var player in roomSlots)
                player.gameObject.GetComponent<RoomPlayer>().characterSelected = false;

            Array.Fill(variantAvaliable, true);

            playersVariants.Clear();
        }
        
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        RoomPlayer roomPlayerComponent = roomPlayer.GetComponent<RoomPlayer>();
        Transform startPos = GetStartPosition();
        alivePlayers.Add(conn);
        return startPos != null
            ? Instantiate(playerPrefabVariants[roomPlayerComponent.VariantID], startPos.position, startPos.rotation)
            : Instantiate(playerPrefabVariants[roomPlayerComponent.VariantID], Vector3.zero, Quaternion.identity);
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

    public bool IsHost()
    {
        return NetworkServer.active;
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

    public void ReturnToRoom()
    {
        foreach (var player in roomSlots)
            player.gameObject.GetComponent<RoomPlayer>().displayType = RoomPlayer.DisplayType.DisplayPlayerSelection;

        ServerChangeScene(RoomScene);
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

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        alivePlayers.Remove(conn);
        deadPlayers.Remove(conn);
        if(playersVariants.ContainsKey(conn))
        {
            variantAvaliable[playersVariants[conn]] = true;
            playersVariants.Remove(conn);
        }
        CheckGameEnd();
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
            int[] orderedPlayersVariants = new int[alivePlayers.Count + deadPlayers.Count];
            orderedPlayersVariants[0] = playersVariants[alivePlayers[0]];
            for (int i = 0; i < deadPlayers.Count; i++)
            {
                orderedPlayersVariants[i + 1] = playersVariants[deadPlayers[deadPlayers.Count - 1 - i]];
            }
            for (int i = deadPlayers.Count - 1; i >= 0; i--)
            {
                deadPlayers[i].identity.gameObject.GetComponent<RoomPlayer>().displayType = RoomPlayer.DisplayType.DisplayGameResults;
                deadPlayers[i].identity.gameObject.GetComponent<RoomPlayer>().orderedPlayersVariants = orderedPlayersVariants;
            }
            alivePlayers[0].identity.gameObject.GetComponent<RoomPlayer>().displayType = RoomPlayer.DisplayType.DisplayGameResults;
            alivePlayers[0].identity.gameObject.GetComponent<RoomPlayer>().orderedPlayersVariants = orderedPlayersVariants;
            alivePlayers.Clear();
            deadPlayers.Clear();
            playersVariants.Clear();
            Array.Fill(variantAvaliable, true);
        }
    }
}
