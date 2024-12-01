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


    void Update()
    {
        if (isLocalPlayer && Input.GetKeyDown(KeyCode.G))
        {
            Damage(10);
        }
    }


    public void Damage(int damage)
    {
        currentHealth -= damage;

        barController.SetValue((float)currentHealth / (float)maxHealth);
    }

}
