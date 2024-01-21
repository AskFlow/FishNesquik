using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefabUI : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlayerManager.Instance.networkObjects.Add(GetComponent<NetworkObject>());

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        PlayerManager.Instance.networkObjects.Remove(GetComponent<NetworkObject>());
    }
}

