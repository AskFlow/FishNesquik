using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FishNet.Object;
using FishNet;

public class PlayerSpawnerScene : NetworkBehaviour
{
    [SerializeField] public GameObject prefab;
    [SerializeField] public List<GameObject> spawnPoint;
    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerRpc();
    }

    
    public void ServerRpc()
    {
        foreach (var item in InstanceFinder.ClientManager.Clients)
        {
            GameObject go = Instantiate(prefab, spawnPoint[0].transform.position, spawnPoint[0].transform.rotation);
            Spawn(go, item.Value);
        }
    }
}
