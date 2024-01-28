using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FishNet.Object;
using FishNet;

public class PlayerSpawnerScene : NetworkBehaviour
{
    [SerializeField] public GameObject prefab;
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("test");
        ServerRpc();
    }

    
    public void ServerRpc()
    {
        foreach (var item in InstanceFinder.ClientManager.Clients)
        {
            GameObject go = Instantiate(prefab);
            Spawn(go, item.Value);
        }
    }
}
