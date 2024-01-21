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

    public Dictionary<NetworkObject, GameObject> players = new Dictionary<NetworkObject, GameObject>();

    public (NetworkObject, GameObject) owner;

    [SyncVar] GameObject instanciedGameObject;

    private void Update()
    {
        foreach (var item in PlayerManager.Instance.networkObjects)
        {
            Debug.Log(item.IsServer);
            Debug.Log(item.IsOwner);
            if (!item.IsOwner) { continue; }
            if (!players.ContainsKey(item) && !owner.Item1 == item)
            {
                GameObject parent = IsServer ? parentOwner : parentClient;


                ServerSpawn(parent);
                Debug.Log(instanciedGameObject);
                SetClientOwner(item, instanciedGameObject);
                if (IsServer)
                {
                    owner.Item1 = item;
                    owner.Item2 = instanciedGameObject;
                }
                else
                {
                    players.Add(item, instanciedGameObject);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void ServerSpawn(GameObject parent)
    {
        GameObject go = Instantiate(prefab, parent.transform);
        ServerManager.Spawn(go);
        instanciedGameObject = go;
    }

    public void PressReady()
    {
        if(IsServer)
        {
            owner.Item2.GetComponent<PlayerCard>().isReady = !owner.Item2.GetComponent<PlayerCard>().isReady;
        }
        else
        {
            foreach (var item in players)
            {
                if(OwnerId == item.Key.OwnerId)
                {
                    item.Value.GetComponent<PlayerCard>().isReady = !item.Value.GetComponent<PlayerCard>().isReady;
                }
            }
        }
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
