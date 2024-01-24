using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [SyncVar]
    public int health = 100;

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
    public void UpdateHealth(int amountToChange)
    {
        health += amountToChange;

        // Is dead?
        if (health <= 0)
        {
            if (kamikazeScript != null)
            {
                kamikazeScript.DelayedDespawn();
            }
        }
    }
}
