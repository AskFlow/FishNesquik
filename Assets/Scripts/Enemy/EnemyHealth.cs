using FishNet.Managing.Timing;
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
    public bool isBoss = false;

    [SerializeField]
    private Door[] doors;

    private GameObject spawnedObject;

    private void Update()
    {
        if (health <= 0)
        {
            StartCoroutine(DeathCoroutine());

            if (isBoss)
            {
                GameManagerFake.Instance.Victory();
            }
        }
    }

    private void Start()
    {
        doors = FindObjectsOfType<Door>();
    }

    public IEnumerator DeathCoroutine()
    {
        if (animator)
        {
            animator.SetBool("IsDead", true);
            if (TryGetComponent(out EnemyKamikaze stopMovementKamikaze))
            {
                stopMovementKamikaze.enabled = false;
            }
            if (TryGetComponent(out EnemyDistance stopMovementDistance))
            {
                stopMovementDistance.isDead = true;
                stopMovementDistance.enabled = false;
            }
            if (TryGetComponent(out EnemyBoss stopMovementBoss))
            {
                stopMovementBoss.enabled = false;
            }
        }

        Canvas healthUI = GetComponentInChildren<Canvas>();
        healthUI.enabled = false;

        if (animator)
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
            {
                yield return null;
            }
            yield return new WaitForSeconds(timeBeforeDestroy);
        }

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
