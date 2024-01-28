using FishNet.Example.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar]
    public int health = 100;
    public int maxHealth = 100;

    [SyncVar]
    public bool isDead = false;

    private Slider healthSlider;

    public override void OnStartClient()
    {
        base.OnStartClient();

        //if (!base.IsOwner)
        //    GetComponent<PlayerHealth>().enabled = false;

        // Trouver le Slider dans le MainUI
        GameObject mainUI = GameObject.Find("MainUI");
        if (mainUI != null)
        {
            healthSlider = mainUI.GetComponentInChildren<Slider>();
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                UpdateHealth(this, maxHealth);
            }
            else
            {
                Debug.LogWarning("Slider not found in MainUI.");
            }
        }
        else
        {
            Debug.LogWarning("MainUI not found.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealth(PlayerHealth script, int amountToChange)
    {
        script.health += amountToChange;

        if (script.health <= 0)
        {
            script.health = 0;

            if (!script.isDead)
            {
                Die();
            }
        }

        if (script.health > maxHealth)
        {
            script.health = maxHealth;
        }

        UpdateSlider();
    }

    private void Die()
    {
        isDead = true;
        if (GetComponent<PlayerController>() != null)
        {
            GetComponent<PlayerController>().Die();
        }
    }


    private void UpdateSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }
}