using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class HealthLoot : NetworkBehaviour
{
    public int healtAmountToGive = 20;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(other.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
            {
                playerHealth.UpdateHealth(playerHealth, healtAmountToGive);
                DeSpawnedObject(gameObject);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DeSpawnedObject(GameObject obj)
    {
        ServerManager.Despawn(obj);
    }


}
