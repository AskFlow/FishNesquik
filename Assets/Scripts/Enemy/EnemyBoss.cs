using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine.AI;
using System;
using System.Collections.Generic;

public class EnemyBoss : NetworkBehaviour
{
    public float speed = 2f;
    public float rotationSpeed = 5f;
    public float wheelRotationSpeed = 100f;
    public MinMaxFloat pitchDistortionMovementSpeed;
    public AudioClip movementSound;
    public AudioClip deathSound;
    private AudioSource audioSource;
    private Transform nearestPlayer;
    private bool isPerformingAttack = false;
    private float minDistance = 5f;
    public bool mustRotatesWheels;

    private bool isPerformingCharge = false;
    private List<GameObject> playersHitByCharge = new List<GameObject>();


    public GameObject leftWheel;
    public GameObject rightWheel;

    public float minTimeBetweenAttacks = 1f;
    public float maxTimeBetweenAttacks = 5f;

    public int bulletAttackCount = 5;
    public float bulletAttackSpeed = 10f;
    public int chargeAttackDamage = 30;
    public float jumpAttackRadius = 5f;
    public float jumpAttackDamage = 20f;

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

        // Start the first attack
        Debug.Log("start");
        StartCoroutine(PerformRandomAttack(-1));
    }

    private void Update()
    {
        if (IsServer)
        {
            if (!isPerformingAttack)
            {
                nearestPlayer = GetNearestPlayer();
                ChaseNearestPlayer(nearestPlayer);
            }

            if (mustRotatesWheels)
            {
                RotateWheels(wheelRotationSpeed);

                // Movement sound
                audioSource.pitch = Mathf.Lerp(pitchDistortionMovementSpeed.Min, pitchDistortionMovementSpeed.Max, speed);

                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
            else
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
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
                nearestPlayer = player.transform;
            }
        }

        return nearestPlayer;
    }

    private void RotateWheels(float rotationSpeed)
    {
        leftWheel.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        rightWheel.transform.Rotate(Vector3.down * rotationSpeed * Time.deltaTime);
    }

    private void ChaseNearestPlayer(Transform nearestPlayer)
    {
        if (nearestPlayer != null)
        {
            // Direction to the player
            Vector3 directionToPlayer = nearestPlayer.position - transform.position;
            directionToPlayer.y = 0f;

            // should continue to chase?
            if (directionToPlayer.magnitude > minDistance)
            {
                // Rotation to look at the player
                if (directionToPlayer.magnitude > 0.1f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                }

                // Mouvement
                transform.Translate(directionToPlayer.normalized * speed * Time.deltaTime, Space.World);

                mustRotatesWheels = true;
            }
            else
            {
                // special attack?
                Debug.Log("Boss stopped chasing player.");
            }
        }
    }


    // ------------------------------------------------------------------------------------------
    // Attacks
    // ------------------------------------------------------------------------------------------

    private IEnumerator PerformRandomAttack(int attackType)
    {
        // Wait for a random time before performing the next attack
        yield return new WaitForSeconds(UnityEngine.Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks));

        mustRotatesWheels = false;
        isPerformingAttack = true;
        Debug.Log("Start Attack");

        if(attackType != 0 && attackType != 1 && attackType != 2)
        {
            // Randomly choose the next attack type
            attackType = UnityEngine.Random.Range(0, 3);
            Debug.Log("Attack chosen : " + attackType);
        }
        
        // Perform the chosen attack
        switch (attackType)
        {
            case 0:
                Debug.Log("Shooting Attack");
                StartCoroutine(BulletAttack());
                break;
            case 1:
                Debug.Log("Charge Attack");
                StartCoroutine(ChargeAttack());
                break;
            case 2:
                Debug.Log("Jumping Attack");
                StartCoroutine(JumpAttack());
                break;
        }
    }

    private void DealPlayerDamage(GameObject player, int damage)
    {
        // Vérifie si le joueur a le composant PlayerHealth
        if (player.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
        {
            Debug.Log("update health ! ");
            // Inflige des dégâts au joueur
            UpdateHealthOnServer(playerHealth, -damage);
        }
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


    // ------------------------------------------------------------------------------------------
    // Attacks Types
    // ------------------------------------------------------------------------------------------
    private IEnumerator BulletAttack()
    {
        for (int i = 0; i < bulletAttackCount; i++)
        {
            // Shoot a bullet towards the player
            // You need to implement the logic for shooting bullets
            // This is a placeholder for demonstration purposes
            yield return new WaitForSeconds(0.5f); // Adjust delay between bullets
        }

        // Continue with the next attack
        isPerformingAttack = false;
        StartCoroutine(PerformRandomAttack(-1));
    }


    private IEnumerator ChargeAttack()
    {
        // Record the target player's position
        Vector3 targetPosition = nearestPlayer.position;

        // Change the wheel rotation speed
        wheelRotationSpeed = wheelRotationSpeed * 10;
        mustRotatesWheels = true;

        // Wait for 2 seconds before starting the charge
        yield return new WaitForSeconds(2f);

        isPerformingCharge = true;
        playersHitByCharge.Clear();
        Debug.Log("CHAAARGE");

        // Charge towards the target player's position
        float chargeSpeed = 15f;
        float chargeDuration = 1f;

        float elapsedTime = 0f;

        while (elapsedTime < chargeDuration)
        {
            // Move the boss towards the target player's position
            transform.position = Vector3.Lerp(transform.position, targetPosition, (elapsedTime / chargeDuration) * chargeSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Reset the wheel rotation speed
        wheelRotationSpeed = wheelRotationSpeed / 10;

        // Continue with the next attack
        isPerformingAttack = false;
        StartCoroutine(PerformRandomAttack(-1));
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player? ");
        if (isPerformingCharge && other.CompareTag("Player") && !playersHitByCharge.Contains(other.gameObject))
        {
            Debug.Log("Hit player! ");
            // If the boss is charging and the player has not been hit yet
            // Add the player to the list of hit players
            playersHitByCharge.Add(other.gameObject);

            // Inflict damage to the player
            DealPlayerDamage(other.gameObject, chargeAttackDamage);
        }
    }



    private IEnumerator JumpAttack()
    {
        // Jump in the air and create a shockwave upon landing
        // You need to implement the logic for jumping and creating a shockwave
        yield return new WaitForSeconds(3f); // Adjust jump duration

        // Continue with the next attack
        isPerformingAttack = false;
        StartCoroutine(PerformRandomAttack(-1));
    }


    // ------------------------------------------------------------------------------------------
    // Utils
    // ------------------------------------------------------------------------------------------
    private IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(0.1f);
        ServerManager.Despawn(gameObject);
    }



}
