using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine.AI;
using static EnemyKamikaze;

public class EnemyDistance : NetworkBehaviour
{
    public float shootingDistance = 10f;
    public float speed = 5f;
    public float rotationSpeed = 5f;
    public MinMaxFloat pitchDistortionMovementSpeed;

    private Transform nearestPlayer = null;

    public AudioClip movementSound;
    public AudioClip deathSound;
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
            bool alreadyShooting = CheckActivationDistance(nearestPlayer);
            if (!alreadyShooting)
            {
                ChaseNearestPlayer(nearestPlayer);
            }
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

    private bool CheckActivationDistance(Transform playerToAim)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerToAim.position);

        if (distanceToPlayer < shootingDistance)
        {
            Debug.Log("close to the player");
            ShootPlayer(playerToAim);
            return true;
        }
        
        return false;
    }

    private void ShootPlayer(Transform playerToShoot)
    {
        // Shoot logic here 
        // TODO : shoot to the player, serv side
    }

    private IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(0.1f);
        ServerManager.Despawn(gameObject);
    }
}
