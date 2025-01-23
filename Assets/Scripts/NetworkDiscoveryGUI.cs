using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using UnityEngine;

public class NetworkDiscoveryGUI : MonoBehaviour
{
    public NetworkDiscovery networkDiscovery;
    readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

    private bool showServers = false;

    private void Awake()
    {
        networkDiscovery = GetComponent<NetworkDiscovery>();

        networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
#endif
    }

    public void OnGUI()
    {
        if (NetworkManager.singleton == null)
            return;

        if (!NetworkClient.isConnected && !NetworkServer.active && !NetworkClient.active)
        {
            drawTitle();
            if (showServers)
                drawServerListGUI();
            else
                drawButtonsGUI();
        }
    }

    private void drawTitle()
    {
        Rect textRect = new Rect(0, 0, Screen.width, Screen.height * 0.5f);
        GUI.Label(textRect, "Czarodziejki ze Skaryszewa", new GUIStyle(GUI.skin.label)
        {
            fontSize = 80,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        });
    }

    private void drawButtonsGUI()
    {
        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 48,
            alignment = TextAnchor.MiddleCenter
        };
        GUILayout.BeginArea(new Rect(Screen.width * 0.25f, Screen.height * 0.5f, Screen.width * 0.5f, Screen.height * 0.5f));
        GUILayout.BeginVertical();

        if (GUILayout.Button("Start Host", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
        {
            discoveredServers.Clear();
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
        }
        GUILayout.Space(50);
        if (GUILayout.Button("Find Servers", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
        {
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
            showServers = true;
        }
        GUILayout.Space(50);
        if (GUILayout.Button("Exit Game", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
        {
            Debug.Log("Exiting Game...");
            Application.Quit();
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void drawServerListGUI()
    {
        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 48,
            alignment = TextAnchor.MiddleCenter
        };
        GUILayout.BeginArea(new Rect(Screen.width * 0.25f, Screen.height * 0.5f, Screen.width * 0.5f, Screen.height * 0.5f));
        GUILayout.BeginScrollView(new Vector2(0, 0));
        foreach (ServerResponse info in discoveredServers.Values)
            if (GUILayout.Button(info.EndPoint.Address.ToString(), buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
                Connect(info);
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width - 150f, 10f, 140f, 30f));
        if (GUILayout.Button("Back"))
        {
            showServers = false;
        }
        GUILayout.EndArea();
    }
    public void HostGame()
    {
        discoveredServers.Clear();
        NetworkManager.singleton.StartHost();
        networkDiscovery.AdvertiseServer();
    }

    public void FindServers()
    {
        discoveredServers.Clear();
        networkDiscovery.StartDiscovery();
    }

    void Connect(ServerResponse info)
    {
        networkDiscovery.StopDiscovery();
        NetworkManager.singleton.StartClient(info.uri);
    }

    public void OnDiscoveredServer(ServerResponse info)
    {
        // Note that you can check the versioning to decide if you can connect to the server or not using this method
        discoveredServers[info.serverId] = info;
    }
}
