using Mirror;
using UnityEngine;


public class HealthController : NetworkBehaviour
{
    public int currentHealth { get; private set; }
    public int maxHealth = 100;

    public GameObject spectatorObject;

    private BarController barController;


    private void Start()
    {
        currentHealth = maxHealth;
        barController = GetComponentInChildren<BarController>();
    }


    [Server]
    public void Damage(int damage)
    {
        currentHealth -= damage;

        RpcSetHealth(currentHealth);

        if (currentHealth <= 0)
            KillPlayer();
    }


    [ClientRpc]
    void RpcSetHealth(int newHealth)
    {
        currentHealth = newHealth;

        barController.SetValue((float)currentHealth / (float)maxHealth);
    }


    [Server]
    private void KillPlayer()
    {
        var networkIdentity = gameObject.GetComponent<NetworkIdentity>();
        var transform = gameObject.GetComponent<Transform>();
        GameObject spectator = Instantiate(spectatorObject, transform.position, Quaternion.identity);
        NetworkServer.Spawn(spectator);

        NetworkServer.ReplacePlayerForConnection(networkIdentity.connectionToClient, spectator, ReplacePlayerOptions.KeepActive);

        var spectatorID = spectator.GetComponent<NetworkIdentity>().netId;
        RpcSetCamera(spectatorID);
        TargetShowDeathScreen(spectator.GetComponent<NetworkIdentity>().connectionToClient, spectatorID);
        RpcDeactivateGameObject(networkIdentity.netId);

        gameObject.SetActive(false);

        GameNetworkManager manager = NetworkManager.singleton as GameNetworkManager;
        manager.OnPlayerKilled(spectator.GetComponent<NetworkIdentity>().connectionToClient);
    }


    [ClientRpc]
    void RpcSetCamera(uint spectattorID)
    {
        if (NetworkClient.spawned.TryGetValue(spectattorID, out NetworkIdentity identity))
        {
            GameObject spectator = identity.gameObject;
            spectator.GetComponent<SpectatorController>().SetupLocalPlayerCamera();
        }
        else
            Debug.LogError("Spectator not found");
    }


    [ClientRpc]
    public void RpcDeactivateGameObject(uint objectID)
    {
        if (NetworkClient.spawned.TryGetValue(objectID, out NetworkIdentity identity))
            identity.gameObject.SetActive(false);
    }


    [TargetRpc]
    private void TargetShowDeathScreen(NetworkConnectionToClient target, uint spectattorID)
    {
        if (NetworkClient.spawned.TryGetValue(spectattorID, out NetworkIdentity identity))
        {
            GameObject spectator = identity.gameObject;
            spectator.GetComponent<SpectatorController>().ShowDeathMessage();
        }
    }
}
