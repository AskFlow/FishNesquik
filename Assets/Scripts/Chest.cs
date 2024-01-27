using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class Chest : NetworkBehaviour
{
    [SerializeField]
    private GameObject objectToLootPrefab;
    [SerializeField]
    private KeyCode interactButton = KeyCode.E;

    
    private GameObject spawnedObject;
    private Vector3 addspawn = new Vector3(0.0f,0.0f, 2.0f);


    private bool isOpenable;

    private bool isNearChest;

    private int ownerPlayerId;



    public void Start()
    {
        if (objectToLootPrefab != null)
        {
            isOpenable = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(interactButton))
        {
            LootObject();
        }
    }

    public void LootObject() {
        if (isOpenable && IsOwner && isNearChest)
        {
            SpawnedObject(objectToLootPrefab, this);
            isOpenable = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnedObject(GameObject obj, Chest script)
    {
        GameObject spawned = Instantiate(obj, gameObject.transform.position + addspawn, Quaternion.identity);
        //GameObject spawned = Instantiate(obj, position);


        ServerManager.Spawn(spawned);
        SetSpawnedObject(spawned, script);
    }

    [ObserversRpc]
    void SetSpawnedObject(GameObject spawned, Chest script)
    {
        script.spawnedObject = spawned;
        script.ownerPlayerId = OwnerId;
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isNearChest = IsOwner && other.GetComponent<NetworkObject>().OwnerId == ownerPlayerId;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearChest = false;
        }
    }
}
