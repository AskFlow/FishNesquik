using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefabUI : NetworkBehaviour
{

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner) { return; }

        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        if (IsServer)
        {
            playerManager.AddPlayer(Owner);
        }
        else
        {
            AddPlayerToServerRpc(Owner);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void AddPlayerToServerRpc(NetworkConnection player)
    {
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        playerManager.AddPlayer(player);

    }


    public override void OnStopClient()
    {
        base.OnStopClient();
        FindObjectOfType<PlayerManager>().RemovePlayer(Owner);

    }

}

