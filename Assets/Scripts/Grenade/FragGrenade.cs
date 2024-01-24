using FishNet.Managing.Server;
using FishNet.Object;
using System.Collections;
using UnityEngine;

public class FragGrenade : NetworkBehaviour
{
    public float timeBeforeExplosion = 2f;
    public string explosionEffectPrefabName = "ExplosionEffectPrefab";
    public string fragmentPrefabName = "FragmentPrefab";
    public int numberOfFragments = 5;
    public float fragmentSpawnRadius = 2f;
    public int damage = 20;
    public float explosionRadius = 5f;

    public GameObject explosionEffectPrefab;
    public GameObject fragmentsToSpawn;
    [HideInInspector]
    public GameObject spawnedObject;

    [Header("Audio Settings")]
    public AudioClip explosionSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = explosionSound;

        if (IsServer)
        {
            Invoke("ExplodeGrenade", timeBeforeExplosion);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ExplodeGrenade()
    {
        if (IsServer)
        {
            // spawn explosion effect
            GameObject go = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            ServerManager.Spawn(go);

            StartCoroutine(DelayedDespawn());
        }

        // Spawn fragments
        for (int i = 0; i < numberOfFragments; i++)
        {
            Vector3 randomDirection = UnityEngine.Random.onUnitSphere * fragmentSpawnRadius;
            randomDirection.y = 0f; // Keep the fragments on the same level as the grenade
            Vector3 fragmentPosition = transform.position + randomDirection;

            SpawnFragment(fragmentsToSpawn, fragmentPosition, this);
        }

        // Inflict damage in the explosion radius
        ApplyDamageInRadius();
    }

    public IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(0.1f);

        if (gameObject != null && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            ServerManager.Despawn(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnFragment(GameObject fragmentPrefabName, Vector3 fragmentPosition, FragGrenade script)
    {
        GameObject spawnedEnemy = Instantiate(fragmentPrefabName, fragmentPosition, Quaternion.identity);
        ServerManager.Spawn(spawnedEnemy);
        SetSpawnedObject(spawnedEnemy, script);
    }

    [ObserversRpc]
    public void SetSpawnedObject(GameObject spawnedFragment, FragGrenade script)
    {
        script.spawnedObject = spawnedFragment;
    }

    void ApplyDamageInRadius()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider targetsCollider in targets)
        {
            if (targetsCollider.CompareTag("Player"))
            {
                if (targetsCollider.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
                {
                    UpdateHealthOnServer(playerHealth, -damage);
                }
            }
            if (targetsCollider.CompareTag("Enemy"))
            {
                if (targetsCollider.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
                {
                    UpdateHealthOnEnemy(enemyHealth, -damage);
                }
            }
        }

        // Explosion sound
        audioSource.Stop();
        audioSource.PlayOneShot(explosionSound);

        if (IsServer)
        {
            // spawn explosion effect
            GameObject go = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            ServerManager.Spawn(go);

            StartCoroutine(DelayedDespawn());
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateHealthOnServer(PlayerHealth playerHealth, int amountToChange)
    {
        if (IsServer)
        {
            playerHealth.UpdateHealth(playerHealth, amountToChange);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateHealthOnEnemy(EnemyHealth enemyHealth, int amountToChange)
    {
        if (IsServer)
        {
            enemyHealth.UpdateHealth(amountToChange);
        }
    }
}
