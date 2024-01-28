using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine.AI;
using FishNet.Example.ColliderRollbacks;

public class EnemyDistance : NetworkBehaviour
{
    [Header("Enemy Behavior")]
    public float shootingDistance = 10f;
    public float speed = 5f;
    public float rotationSpeed = 5f;

    [Header("Enemy Detection")]
    private Transform playerToAim = null;
    private Vector3 directionToPlayer;
    private Quaternion lookRotation;

    [Header("Audio Settings")]
    public AudioClip movementSound;
    public AudioClip deathSound;
    private AudioSource audioSource;
    public MinMaxFloat pitchDistortionMovementSpeed;

    [Header("Shoot")]
    public GameObject projectilePrefab;
    private bool isShooting = false;
    private bool shouldShoot = false;
    [SerializeField] private Transform aimStart;
    private bool coroutineHasStarted = false;

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
        // Initialize audio source
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = movementSound;
        audioSource.loop = true;
    }

    private void Update()
    {
        if (IsServer)
        {
            // Detect nearest player
            playerToAim = GetNearestPlayer();
            shouldShoot = CheckActivationDistance(playerToAim);

            // Update rotation logic
            directionToPlayer = playerToAim.position - transform.position;
            directionToPlayer.y = 0f;
            lookRotation = Quaternion.LookRotation(directionToPlayer.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            // Start shooting coroutine if should shoot
            if (shouldShoot)
            {
                if (!isShooting && !coroutineHasStarted)
                {
                    isShooting = true;
                    StartCoroutine(ShootCoroutine(1f));
                }
            }
            else
            {
                isShooting = false;
                // Move towards nearest player
                ChaseNearestPlayer(playerToAim);

                // Movement sound
                audioSource.pitch = Mathf.Lerp(pitchDistortionMovementSpeed.Min, pitchDistortionMovementSpeed.Max, speed);
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
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
                playerToAim = player.transform;
            }
        }

        return playerToAim;
    }

    private void ChaseNearestPlayer(Transform nearestPlayer)
    {
        if (nearestPlayer != null)
        {
            // Calculate direction to player
            directionToPlayer = nearestPlayer.position - transform.position;
            directionToPlayer.y = 0f;

            // Rotate towards player
            lookRotation = Quaternion.LookRotation(directionToPlayer.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            // Move towards player
            transform.Translate(directionToPlayer.normalized * speed * Time.deltaTime, Space.World);
        }
    }

    private bool CheckActivationDistance(Transform playerToAim)
    {
        if (playerToAim == null)
        {
            return false;
        }

        // Check distance to determine if enemy should shoot
        float distanceToPlayer = Vector3.Distance(transform.position, playerToAim.position);
        return distanceToPlayer < shootingDistance;
    }

    private IEnumerator ShootCoroutine(float timeBetweenShots)
    {
        coroutineHasStarted = true;
        while (isShooting)
        {
            // Shoot projectile towards aimStart position
            Vector3 spawnPosition = aimStart.position;
            Quaternion spawnRotation = Quaternion.LookRotation(playerToAim.position - aimStart.position);
            GameObject projectileInstance = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
            ServerManager.Spawn(projectileInstance.gameObject);

            // Wait for some time before shooting again
            yield return new WaitForSeconds(timeBetweenShots);
        }
        coroutineHasStarted = false;
    }

    // Delayed despawn coroutine
    public IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(0.1f);
        if (gameObject != null && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            ServerManager.Despawn(gameObject);
        }
    }
}
