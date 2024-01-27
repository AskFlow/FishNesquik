using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LobbyUI : NetworkBehaviour
{

    public GameObject prefab;
    public GameObject parentClient;
    public GameObject parentOwner;

    public Dictionary<NetworkConnection, NetworkObject> players = new Dictionary<NetworkConnection, NetworkObject>();

    public (NetworkConnection, GameObject) owner;



    public void Start()
    {
        if (IsServer)
        {
            PlayerManager oui = FindObjectOfType<PlayerManager>();
            oui.networkObjectAddedDelegate += OnPlayerAdded;
            oui.networkObjectRemovedDelegate += OnPlayerRemoved;
        }
    }

    [Server]
    public void OnPlayerAdded(NetworkConnection player)
    {
        NetworkObject parent;
        if(player.IsHost)
        {
            parent = parentOwner.GetComponent<NetworkObject>();
        }
        else
        {
            parent = parentClient.GetComponent<NetworkObject>();

        }
        NetworkObject go = Instantiate(prefab, parent.RuntimeParentTransform).GetComponent<NetworkObject>();
        Spawn(go,player);
        go.GiveOwnership(player);
        go.SetParent(parent);
        SetParent(go);
        players.Add(player, go);

    }

    [ServerRpc]
    public void OnPlayerRemoved(NetworkConnection player)
    {

    }


    [ObserversRpc(ExcludeServer =true)]
    public void SetParent(NetworkObject obj)
    {
        obj.SetParent(parentClient.GetComponent<NetworkObject>());

    }

    [ServerRpc(RequireOwnership =false)]
    public void PressReadyServerRpc(NetworkConnection conn)
    {
        players[conn].GetComponent<PlayerCard>().ToggleToggle();
    }

    [TargetRpc]
    public void PressReadyUserRpc(NetworkConnection conn)
    {

    }

    public void PressReady()
    {

        PressReadyServerRpc(Owner);


        //if(IsServer)
        //{
        //    owner.Item2.GetComponent<PlayerCard>().isReady = !owner.Item2.GetComponent<PlayerCard>().isReady;
        //}
        //else
        //{
        //    foreach (var item in players)
        //    {
        //        if(OwnerId == item.Key.OwnerId)
        //        {
        //            item.Value.GetComponent<PlayerCard>().isReady = !item.Value.GetComponent<PlayerCard>().isReady;
        //        }
        //    }
        //}
    }

    [ObserversRpc]
    public void SetClientOwner(NetworkObject item, GameObject go)
    {
        if (IsOwner ||IsServer)
        {
            go.GetComponent<PlayerCard>().isPlayer = true;
        }
    }
}
