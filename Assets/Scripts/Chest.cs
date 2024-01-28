using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class Chest : NetworkBehaviour
{
    [SerializeField]
    private GameObject objectToLootPrefab;

    private GameObject spawnedObject;
    private Vector3 addspawn = new Vector3(0.0f,0.0f, 2.0f);


    private bool isOpenable;


    public void Start()
    {
        if (objectToLootPrefab != null)
        {
            isOpenable = true;
        }
    }


    public void LootObject() {
        if (isOpenable)
        {

            SpawnedObject(objectToLootPrefab, this);
            isOpenable = false;

        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnedObject(GameObject obj, Chest script)
    {
        GameObject spawned = Instantiate(obj, gameObject.transform.position + addspawn, Quaternion.identity);


        ServerManager.Spawn(spawned);
        SetSpawnedObject(spawned, script);
    }

    [ObserversRpc]
    void SetSpawnedObject(GameObject spawned, Chest script)
    {
        script.spawnedObject = spawned;
    }

}
