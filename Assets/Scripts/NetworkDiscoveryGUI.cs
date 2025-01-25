using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkDiscoveryGUI : MonoBehaviour
{
    public NetworkDiscovery networkDiscovery;
    public GameObject serversPopup;
    public Transform serversList;
    public GameObject buttonPrefab;
    readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

    private bool shouldReloadServers = false;

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
            if (shouldReloadServers && serversPopup != null)
            {
                if (serversPopup.activeSelf)
                {
                    PopulateServers();
                    shouldReloadServers = false;
                }
            }
        }
    }

    public void PopulateServers()
    {
        foreach (Transform child in serversList)
        {
            Destroy(child.gameObject);
        }
        foreach (ServerResponse info in discoveredServers.Values)
        {
            GameObject serverButton = Instantiate(buttonPrefab, serversList);
            serverButton.GetComponentInChildren<TMP_Text>().text = info.EndPoint.Address.ToString();

            serverButton.GetComponent<Button>().onClick.AddListener(() => Connect(info));
        }
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
        PopulateServers();
        serversPopup.SetActive(true);
    }

    public void CloseServersPopup()
    {
        serversPopup.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
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
        shouldReloadServers = true;
    }
}
