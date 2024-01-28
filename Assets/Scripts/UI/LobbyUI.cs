using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class LobbyUI : NetworkBehaviour
{

    public GameObject prefab;
    public GameObject parentClient;
    public GameObject parentOwner;

    [SyncObject]
    public readonly SyncDictionary<NetworkConnection, NetworkObject> players = new SyncDictionary<NetworkConnection, NetworkObject>();
    [SyncVar]
    public  NetworkObject owner;

    private PlayerManager playerManager;

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public void Start()
    {
        if (IsServer)
        {
            playerManager = FindObjectOfType<PlayerManager>();
            playerManager.networkObjectAddedDelegate += OnPlayerAdded;
            playerManager.networkObjectRemovedDelegate += OnPlayerRemoved;
            StartCoroutine(CheckIfPlayerReady());
        }
    }


    public IEnumerator CheckIfPlayerReady()
    {
        while (true)
        {
            if(players.Count > 0) 
            {
                if(CheckIfAllReady())
                {
                    GameManager.Instance.StartGame();
                    break;

                }
            }
            yield return new WaitForSeconds(1);
            
        }
    }

    public bool CheckIfAllReady()
    {
        foreach (var item in players)
        {
            if (!item.Value.GetComponent<PlayerCard>().isReady) { return false; }
        }
        return true;
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

        if (player.IsHost)
        {
            owner = go;
        }
        else
        {
            players.Add(player, go);
        }

    }

    [ServerRpc]
    public void OnPlayerRemoved(NetworkConnection player)
    {
        if(players.TryGetValue(player, out NetworkObject parent))
        {
            parent.Despawn(DespawnType.Destroy);
            players.Remove(player);
        }
    }


    [ObserversRpc(ExcludeServer =true)]
    public void SetParent(NetworkObject obj)
    {
        obj.SetParent(parentClient.GetComponent<NetworkObject>());

    }

}
