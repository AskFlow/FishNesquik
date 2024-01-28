using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class Door : NetworkBehaviour
{
    [SerializeField]
    public List<EnemySpawnZone> enemyInZone;

    [SerializeField]
    private bool isDoorAlreadyOpened;

    private Vector3 positionInitiale;
    private Vector3 positionFinale;

    private void Start()
    {
        positionInitiale = transform.position;
        positionFinale = transform.position;
        positionFinale.y += 8.0f;
    }


    private void Update()
    {
        
        if (CheckAllEnemyDied() && !isDoorAlreadyOpened)
        {
            Debug.Log("oyui");
            ServerOpenDoor();
        }
    }

    // Méthode côté serveur
    [ServerRpc(RequireOwnership = false)]
    void ServerOpenDoor()
    {
        // Coroutine pour ouvrir la porte côté serveur
        StartCoroutine(ServerOpenDoorCoroutine());
    }

    IEnumerator ServerOpenDoorCoroutine()
    {
        Debug.Log("non");
        isDoorAlreadyOpened = true;
        float elapsedTime = 0f;
        float lerpDuration = 5.0f;
        while (elapsedTime < lerpDuration)
        {
            float t = elapsedTime / lerpDuration;
            // Appel de la méthode Rpc pour synchroniser la position avec les clients
            RpcSyncDoorPosition(Vector3.Lerp(positionInitiale, positionFinale, t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Appel de la méthode Rpc pour finaliser la position de la porte
        RpcSyncDoorPosition(positionFinale);
    }

    // Méthode Rpc pour synchroniser la position de la porte avec les clients
    [ObserversRpc]
    void RpcSyncDoorPosition(Vector3 position)
    {
        transform.position = position;
    }

    private bool CheckAllEnemyDied()
    {

        foreach (EnemySpawnZone item in enemyInZone)
        {
            if(!item.finish)
            {
                return false;
            }
        }
        return true;
    }
}