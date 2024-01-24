using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar] public int health = 100;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
            GetComponent<PlayerHealth>().enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealth(PlayerHealth script, int amountToChange)
    {
        script.health += amountToChange;
    }
}
