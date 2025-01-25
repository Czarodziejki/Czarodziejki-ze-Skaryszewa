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
            GUILayout.BeginArea(new Rect(Screen.width - 150f, 10f, 140f, 30f));
            if (GUILayout.Button("Return to main menu", buttonStyle))
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
}
