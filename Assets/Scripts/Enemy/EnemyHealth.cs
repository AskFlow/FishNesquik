using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class EnemyHealth : NetworkBehaviour
{
    [SyncVar] public int health = 100;

    private EnemyKamikaze kamikazeScript;


    private void Start()
    {
        kamikazeScript = GetComponent<EnemyKamikaze>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
            GetComponent<EnemyHealth>().enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealth(EnemyHealth script, int amountToChange)
    {
        script.health += amountToChange;

        // Is dead?
        if (script.health <= 0)
        {
            if (kamikazeScript != null)
            {
                kamikazeScript.DelayedDespawn();
            }
        }
    }
}
