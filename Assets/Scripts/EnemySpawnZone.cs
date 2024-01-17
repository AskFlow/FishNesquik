using System.Collections;
using UnityEngine;
using FishNet.Object;

public class EnemySpawnZone : NetworkBehaviour
{
    public GameObject enemyPrefab1;
    public GameObject enemyPrefab2;
    public int numberOfEnemies1ToSpawn = 3;
    public int numberOfEnemies2ToSpawn = 2;
    public Vector3 spawnPosition = Vector3.zero;
    private bool hasBeenActivated = false;

    public GameObject spawnedObject;

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.CompareTag("Player") && hasBeenActivated == false)
        {
            StartCoroutine(SpawnEnemiesWithDelay(enemyPrefab1, spawnPosition, numberOfEnemies1ToSpawn, 2f));
            StartCoroutine(SpawnEnemiesWithDelay(enemyPrefab2, spawnPosition, numberOfEnemies2ToSpawn, 2f));
            hasBeenActivated = true;
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

    [ObserversRpc]
    public void SetSpawnedObject(GameObject spawnedEnemy, EnemySpawnZone script)
    {
        script.spawnedObject = spawnedEnemy;
    }
}
