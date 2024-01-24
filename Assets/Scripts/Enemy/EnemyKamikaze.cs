using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine.XR;
using UnityEngine.AI;
using FishNet;

public class EnemyKamikaze : NetworkBehaviour
{
    public float activationDistance = 2f;
    public float explosionRadius = 5f;
    public float speed = 4f;
    public float rotationSpeed = 5f;
    public int explosionDamage = 20;
    public MinMaxFloat pitchDistortionMovementSpeed;

    private bool hasExploded = false;
    public Transform nearestPlayer = null;

    public AudioClip movementSound;
    public AudioClip explosionSound;
    public GameObject explosionEffectPrefab;
    private AudioSource audioSource;

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
            Debug.Log("close to the player");
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
                Debug.Log("Player detected at position: " + targetsCollider.transform.position);

                if (targetsCollider.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
                {
                    Debug.Log("Player health update!");
                    UpdateHealthOnServer(playerHealth, -explosionDamage);
                }
            }
            if (targetsCollider.CompareTag("Enemy"))
            {
                if (targetsCollider.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
                {
                    Debug.Log("Enemy health update!");
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
        ServerManager.Despawn(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateHealthOnServer(PlayerHealth playerHealth, int amountToChange)
    {
        if (IsServer)
        {
            Debug.Log("set Life");
            playerHealth.UpdateHealth(playerHealth, amountToChange);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateHealthOnEnemy(EnemyHealth enemyHealth, int amountToChange)
    {
        if (IsServer)
        {
            Debug.Log("Setting Life");
            enemyHealth.UpdateHealth(amountToChange);
        }
    }
}
