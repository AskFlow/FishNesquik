using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar]
    public int health = 100;
    public int maxHealth = 100;

    private Slider healthSlider; // Référence au Slider dans l'UI

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
            GetComponent<PlayerHealth>().enabled = false;

        healthSlider = FindObjectOfType<Slider>();
        healthSlider.maxValue = maxHealth;
        UpdateHealth(this, maxHealth);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealth(PlayerHealth script, int amountToChange)
    {
        script.health += amountToChange;
        if (script.health < 0)
        {
            script.health = 0;
        }
        if (script.health > maxHealth)
        {
            script.health = maxHealth;
        }
        UpdateSlider(); // slider
    }

    private void UpdateSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }
}
