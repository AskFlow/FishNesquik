using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [SyncVar]
    public int health = 100;

    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float timeBeforeDestroy = 3.0f;
    [SerializeField]
    private GameObject objToLoot;

    [SerializeField]
    private float probabilityOfLoot = 100f;


    private GameObject spawnedObject;

    private void Update()
    {
        if (health <= 0)
        {
            StartCoroutine(DeathCoroutine());
        }
    }


  

    public IEnumerator DeathCoroutine()
    {
        animator.SetBool("IsDead", true);
        if(TryGetComponent(out EnemyKamikaze stopMovementKamikaze))
        {
            stopMovementKamikaze.enabled = false;

        }
        if(TryGetComponent(out EnemyDistance stopMovementDistance))
        {
            stopMovementDistance.enabled = false;
        }
        if (TryGetComponent(out EnemyBoss stopMovementBoss))
        {
            stopMovementBoss.enabled = false;
        }
        Canvas healthUI = GetComponentInChildren<Canvas>();
        healthUI.enabled = false;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
        {
            yield return null;
        }
        yield return new WaitForSeconds(timeBeforeDestroy);

        if (IsServer)
        {
            if (gameObject != null && gameObject.activeSelf)
            {
             
                if (objToLoot != null && Random.Range(0.0f, 100.0f) <= probabilityOfLoot)
                {
                    SpawnedObject(objToLoot, this);
                }
                yield return new WaitForSeconds(0.1f);
                gameObject.SetActive(false);
                ServerManager.Despawn(gameObject);

            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealth(int amountToChange)
    {
        health += amountToChange;

    }


    [ServerRpc(RequireOwnership = false)]
    void SpawnedObject(GameObject obj, EnemyHealth script)
    {
        GameObject spawned = Instantiate(obj, gameObject.transform.position , Quaternion.identity);

        ServerManager.Spawn(spawned);
        SetSpawnedObject(spawned, script);
    }

    [ObserversRpc]
    void SetSpawnedObject(GameObject spawned, EnemyHealth script)
    {
        script.spawnedObject = spawned;
    }

}
