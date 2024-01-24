using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;

public class EnemyHealthUI : NetworkBehaviour
{
    private EnemyHealth enemyHealth;
    public Slider healthSlider;

    private void Start()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        if (healthSlider == null)
        {
            Debug.LogError("Health Slider is not assigned to the enemy.");
        }
        else
        {
            healthSlider.maxValue = enemyHealth.health;
        }
    }

    private void Update()
    {
        if (IsClient && enemyHealth != null && healthSlider != null)
        {
            Debug.Log("Update life");
            // Mettez à jour la barre de santé côté client
            healthSlider.value = enemyHealth.health;
        }
    }
}
