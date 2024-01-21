using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine.AI;

public class EnemyBoss : NetworkBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 5f;
    public float wheelRotationSpeed = 100f;
    public MinMaxFloat pitchDistortionMovementSpeed;
    public AudioClip movementSound;
    public AudioClip deathSound;
    private AudioSource audioSource;
    private Transform nearestPlayer;
    private bool isPerformingAttack = false;

    public GameObject leftWheel;
    public GameObject rightWheel;

    public float minTimeBetweenAttacks = 1f;
    public float maxTimeBetweenAttacks = 5f;

    public int bulletAttackCount = 5;
    public float bulletAttackSpeed = 10f;
    public float chargeAttackDamage = 30f;
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
        StartCoroutine(PerformRandomAttack());
    }

    private void Update()
    {
        if (IsServer)
        {
            nearestPlayer = GetNearestPlayer();
            ChaseNearestPlayer(nearestPlayer);
        }

        RotateWheels(wheelRotationSpeed);

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

    private void RotateWheels(float rotationSpeed)
    {
        leftWheel.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        rightWheel.transform.Rotate(Vector3.down * rotationSpeed * Time.deltaTime);
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

    private IEnumerator PerformRandomAttack()
    {
        Debug.Log("start coroutine");
        isPerformingAttack = true;

        // Wait for a random time before performing the next attack
        yield return new WaitForSeconds(Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks));
        Debug.Log("finish coroutine");
        // Randomly choose the next attack type
        int randomAttackType = Random.Range(0, 3);
        Debug.Log("Attack chosen : " + randomAttackType);
        // Perform the chosen attack
        switch (randomAttackType)
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
        StartCoroutine(PerformRandomAttack());
    }

    private IEnumerator ChargeAttack()
    {

        // Enregistre la position du joueur cible
        Vector3 targetPosition = nearestPlayer.position;

        // Change la vitesse de rotation des roues
        wheelRotationSpeed = wheelRotationSpeed * 3;

        // Attend 2 secondes avant de commencer la charge
        yield return new WaitForSeconds(2f);

        // Charge vers la position du joueur cible
        float chargeSpeed = 15f; // Vitesse de la charge (ajuster selon les besoins)
        float chargeDuration = 1f; // Durée de la charge (ajuster selon les besoins)

        float elapsedTime = 0f;

        while (elapsedTime < chargeDuration)
        {
            // Déplace le boss vers la position du joueur cible
            transform.position = Vector3.Lerp(transform.position, targetPosition, (elapsedTime / chargeDuration) * chargeSpeed * Time.deltaTime);

            // Incrémente le temps écoulé
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Remet la vitesse de rotation par défaut
        wheelRotationSpeed = wheelRotationSpeed / 3;

        // Continue avec la prochaine attaque
        isPerformingAttack = false;
        StartCoroutine(PerformRandomAttack());
    }


    private IEnumerator JumpAttack()
    {
        // Jump in the air and create a shockwave upon landing
        // You need to implement the logic for jumping and creating a shockwave
        yield return new WaitForSeconds(3f); // Adjust jump duration

        // Continue with the next attack
        isPerformingAttack = false;
        StartCoroutine(PerformRandomAttack());
    }

    private IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(0.1f);
        ServerManager.Despawn(gameObject);
    }
}
