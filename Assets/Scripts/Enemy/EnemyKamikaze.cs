using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine.XR;
using UnityEngine.AI;
using FishNet;

public class EnemyKamikaze : NetworkBehaviour
{
    [Header("Enemy Behavior")]
    public float activationDistance = 2f;
    public float speed = 4f;
    public float rotationSpeed = 5f;

    [Header("Player Detection")]
    private bool hasExploded = false;
    private Transform nearestPlayer = null;

    [Header("Audio Settings")]
    public AudioClip movementSound;
    public AudioClip explosionSound;
    private AudioSource audioSource;
    public MinMaxFloat pitchDistortionMovementSpeed;

    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public int explosionDamage = 20;
    public GameObject explosionEffectPrefab;


    [System.Serializable]
    public struct MinMaxFloat
    {
        public float Min;
        public float Max;

        public MinMaxFloat(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = movementSound;
        audioSource.loop = true;
    }

    private void Update()
    {
        if (IsServer)
        {
            nearestPlayer = GetNearestPlayer();
            ChaseNearestPlayer(nearestPlayer);
            CheckActivationDistance(nearestPlayer);
        }

        // Movement sound
        audioSource.pitch = Mathf.Lerp(pitchDistortionMovementSpeed.Min, pitchDistortionMovementSpeed.Max, speed);

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private Transform GetNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                nearestPlayer = player.transform;
            }
        }

        return nearestPlayer;
    }

    private void ChaseNearestPlayer(Transform nearestPlayer)
    {
        if (nearestPlayer != null)
        {
            // Movement to the player
            Vector3 directionToPlayer = nearestPlayer.position - transform.position;
            directionToPlayer.y = 0f;

            // Rotation to look at the player
            if (directionToPlayer.magnitude > 0.1f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }

            // Movement
            transform.Translate(directionToPlayer.normalized * speed * Time.deltaTime, Space.World);
        }
    }

    private void CheckActivationDistance(Transform playerToAim)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerToAim.position);

        if (distanceToPlayer < activationDistance && hasExploded == false)
        {
            hasExploded = true;
            Suicide();
        }
    }

    private void Suicide()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider targetsCollider in targets)
        {
            if (targetsCollider.CompareTag("Player"))
            {
                if (targetsCollider.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
                {
                    UpdateHealthOnServer(playerHealth, -explosionDamage);
                }
            }
            if (targetsCollider.CompareTag("Enemy"))
            {
                if (targetsCollider.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
                {
                    UpdateHealthOnEnemy(enemyHealth, -explosionDamage);
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
