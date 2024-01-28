using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    //[SyncVar] public List<NetworkObject> networkObjects = new List<NetworkObject>();


    public GameObject playerPrefab;

    [SyncObject]
    private readonly SyncList<NetworkConnection> _players = new SyncList<NetworkConnection>();
    
    public delegate void NetworkObjectAddedDelegate(NetworkConnection networkObject);
    public NetworkObjectAddedDelegate networkObjectAddedDelegate;
    public delegate void NetworkObjectRemovedDelegate(NetworkConnection networkObject);
    public NetworkObjectRemovedDelegate networkObjectRemovedDelegate;


    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(DebugList());
    }

    public IEnumerator DebugList()
    {
        while (true)
        {
            Debug.Log("Serveur update : ");
            int index = 0;
            foreach (var item in _players)
            {
                Debug.Log(item);
                ShowPlayerList(index);
                index++;
            
            }
            Debug.Log("=====");

            yield return new WaitForSeconds(1);
        }
    }

    [ObserversRpc]
    public void ShowPlayerList(int index)
    {
        Debug.Log(_players[index].ClientId);
    }

    public void SpawnPlayer()
    {
        SpawnPlayerServerRpc();
    }

    [ServerRpc]
    public void SpawnPlayerServerRpc()
    {
        foreach (var item in _players)
        {
            GameObject go = Instantiate(playerPrefab);
            Spawn(go, item);

        }
    }

    public void AddPlayer(NetworkConnection player)
    {
        _players.Add(player);
        networkObjectAddedDelegate(player);
    }

    public void RemovePlayer(NetworkConnection player)
    {
        _players.Remove(player);
        networkObjectRemovedDelegate(player);
    }
}
