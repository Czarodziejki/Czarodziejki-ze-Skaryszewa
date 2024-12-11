using Mirror;
using UnityEngine;

public class HealthController : NetworkBehaviour
{
    public int currentHealth { get; private set; }
    public int maxHealth = 100;

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
    }


    [ClientRpc]
    void RpcSetHealth(int newHealth)
    {
        currentHealth = newHealth;

        barController.SetValue((float)currentHealth / (float)maxHealth);
    }

}
