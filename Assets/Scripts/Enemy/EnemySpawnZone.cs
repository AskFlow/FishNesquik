using System.Collections;
using UnityEngine;
using FishNet.Object;

public class EnemySpawnZone : NetworkBehaviour
{
    public GameObject enemyPrefab;
    public int numberOfEnemiesToSpawn = 3;
    [SerializeField]
    private float timeBeforeEachSpawn = 2f;
    [SerializeField]
    private GameObject spawnPosition;
    [SerializeField]
    private GameObject robotToHide;
    private bool hasBeenActivated = false;
    [HideInInspector]
    public GameObject spawnedObject;

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.CompareTag("Player") && hasBeenActivated == false)
        {
            if (enemyPrefab != null)
            {
                HideObjectOnServer();
                StartCoroutine(SpawnEnemiesWithDelay(enemyPrefab, robotToHide.transform.position, numberOfEnemiesToSpawn, timeBeforeEachSpawn));
                hasBeenActivated = true;
            }
            else
            {
                Debug.LogWarning("'enemyPrefab' est null.");
            }
        }
    }
    private IEnumerator SpawnEnemiesWithDelay(GameObject enemyPrefab, Vector3 spawnPosition, int numberOfEnemies, float delay)
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            SpawnEnemy(enemyPrefab, spawnPosition, this);
            yield return new WaitForSeconds(delay);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnEnemy(GameObject enemyPrefab, Vector3 spawnPosition, EnemySpawnZone script)
    {
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        ServerManager.Spawn(spawnedEnemy);
        SetSpawnedObject(spawnedEnemy, script);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HideObjectOnServer()
    {
        robotToHide.SetActive(false);

        RpcHideObjectOnClients();
    }

    [ObserversRpc]
    private void RpcHideObjectOnClients()
    {
        // Cette méthode sera appelée sur tous les clients pour désactiver l'objet
        if (robotToHide != null)
        {
            robotToHide.SetActive(false);
        }
    }

    [ObserversRpc]
    public void SetSpawnedObject(GameObject spawnedEnemy, EnemySpawnZone script)
    {
        script.spawnedObject = spawnedEnemy;
    }
}
