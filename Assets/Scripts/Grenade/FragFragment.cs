using FishNet.Object;
using System.Collections;
using UnityEngine;

public class FragFragment : NetworkBehaviour
{
    public float timeBeforeExplosion = 0.5f;
    public string explosionEffectPrefabName = "ExplosionEffectPrefab";
    public int damage = 50;
    public float explosionRadius = 5f;
    public GameObject explosionEffectPrefab;

    [Header("Audio Settings")]
    public AudioClip explosionSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = explosionSound;

        if (IsServer)
        {
            Invoke("ApplyDamageInRadius", timeBeforeExplosion);
        }
    }

    [ServerRpc(RequireOwnership = false)]
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
