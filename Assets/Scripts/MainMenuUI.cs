using FishNet.Managing;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private NetworkManager _networkManager;

    public GameObject _lobbyMenu;

    private LocalConnectionState _clientState = LocalConnectionState.Stopped;
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;

    void Start()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager not found, HUD will not function.");
            return;
        }
        else
        {
            _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        }
    }

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        _clientState = obj.ConnectionState;

    }


    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        _serverState = obj.ConnectionState;
    }

    public void OnClickHost()
    {
        if (_networkManager == null)
            return;

        if (_serverState != LocalConnectionState.Stopped)
            _networkManager.ServerManager.StopConnection(true);
        else
        {
            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
            _lobbyMenu.SetActive(true);

        }

    }

    public void OnClickClient()
    {
        if (_networkManager == null)
            return;

        if (_clientState != LocalConnectionState.Stopped)
            _networkManager.ClientManager.StopConnection();
        else
        {
            _networkManager.ClientManager.StartConnection();
            _lobbyMenu.SetActive(true);
        }

    }
}
