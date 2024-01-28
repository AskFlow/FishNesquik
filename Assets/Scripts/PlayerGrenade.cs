using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class PlayerGrenade : NetworkBehaviour
{
    [Header("Grenades")]
    public FragGrenade grenadePrefab;
    private int maxGrenadeCount = 3;
    private int currentGrenadeCount;

    public int getCurrentGrenadeCount()
    {
        return currentGrenadeCount;
    }

    private void Start()
    {
        // Initialise nb of grenades
        currentGrenadeCount = maxGrenadeCount;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ThrowGrenadeServerRpc()
    {
        // Server side
        if (currentGrenadeCount <= 0)
            return;

        if (grenadePrefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 6.0f;

            // create the grenade
            FragGrenade grenadeInstance = Instantiate(grenadePrefab, spawnPosition, Quaternion.identity);
            currentGrenadeCount--;

            // replicate it
            ServerManager.Spawn(grenadeInstance.gameObject);
        }
    }

    [Client]
    public void RechargeGrenades(int amount)
    {
        currentGrenadeCount = Mathf.Min(currentGrenadeCount + amount, maxGrenadeCount);
        // Debug.Log("Grenades rechargées. Nombre actuel de grenades : " + currentGrenadeCount);
    }
}
