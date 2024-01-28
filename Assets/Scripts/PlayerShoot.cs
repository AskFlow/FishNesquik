using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField] private bool isServerAuth;
    [SerializeField] private int damage = 10;
    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private string tagToHit;

    private GameObject projectilePrefab;
    private bool timeCanShoot = true;
    private float lastShootTime;

    public Camera playerCamera;


    [Header("Weapon parameters")]
    private bool isRaycast;
    [Range(0f, 10f)] private float sprayAngle;
    [Range(0f, 2f)] private float timeBetweenShots;
    [Range(1, 10)] private int numberOfBullets;


    // Setters for weapon parameters
    public void SetIsRaycast(bool value)
    {
        isRaycast = value;
    }

    public void SetSprayAngle(float value)
    {
        sprayAngle = value;
    }

    public void SetTimeBetweenShots(float value)
    {
        timeBetweenShots = value;
    }

    public void SetNumberOfBullets(int value)
    {
        numberOfBullets = value;
    }

    public void SetProjectilePrefab(GameObject prefab)
    {
        projectilePrefab = prefab;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

    }

    public int getDamage()
    {
        return damage;
    }

    void Start()
    {
        lastShootTime = Time.time;
    }

    public void TryShoot()
    {
        float timeSinceLastShoot = Time.time - lastShootTime;
        if (timeSinceLastShoot >= timeBetweenShots)
        {
            lastShootTime = Time.time;

            if (!isRaycast && timeCanShoot)
            {
                ShootPhysic(sprayAngle, timeBetweenShots, numberOfBullets);
            }
            else if (isRaycast && timeCanShoot)
            {
                if (isServerAuth)
                {
                    ServerShootRaycast(playerCamera.transform.position, Camera.main.transform.forward);
                }
                else
                {
                    LocalShootRaycast();
                }
            }
        }
    }


    [ServerRpc]
    void ServerShootRaycast(Vector3 camPosition, Vector3 shootForward, NetworkConnection sender = null)
    {
        // Last part is to have the shot be in front of our own player and not inside of him
        Vector3 shotPosition = camPosition + shootForward * 0.5f;
        if (Physics.Raycast(shotPosition, shootForward, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.transform.TryGetComponent(out EnemyHealth otherEnemy) && otherEnemy != this)
            {
                // We've hit another player that isn't the person shooting
                otherEnemy.health -= damage;
                HitConfirmation(sender, otherEnemy.Owner.ClientId, hit.point);
            }
        }
    }

    [TargetRpc]
    void HitConfirmation(NetworkConnection conn, int hitID, Vector3 hitPoint)
    {
        Debug.Log($"You've hit player {hitID} for {damage}. Calculations done by server");
        Instantiate(hitParticle, hitPoint, Quaternion.identity);
    }

    void LocalShootRaycast()
    {
        // Last part is to have the shot be in front of our own player and not inside of him
        Vector3 shotPosition = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        if (Physics.Raycast(shotPosition, playerCamera.transform.forward, out RaycastHit hit, Mathf.Infinity))
        {
            Debug.DrawLine(shotPosition, hit.point, Color.red, 2.0f); // Dessine une ligne rouge pendant 1 seconde
            Transform resultHit = hit.transform;
            if (resultHit.CompareTag(tagToHit) && resultHit != this.gameObject && IsServer)
            {
                Debug.Log("Attack");

                ApplyDamage(resultHit, damage);
                Instantiate(hitParticle, hit.point, Quaternion.identity);
                //Debug.Log($"You've hit player {otherPlayer.Owner.ClientId} for {damage}. Calculations done locally");
            }

        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ShootPhysic(float sprayAngle, float timeBetweenShots, int numberOfBullets)
    {
        StartCoroutine(ShootCoroutine(sprayAngle, timeBetweenShots, numberOfBullets));
    }

    IEnumerator ShootCoroutine(float sprayAngle, float timeBetweenShots, int numberOfBullets)
    {
        timeCanShoot = false;

        for (int i = 0; i < numberOfBullets; i++)
        {
            Vector3 shootDirection = playerCamera.transform.forward;
            Quaternion randomRotation = Quaternion.Euler(Random.Range(-sprayAngle, sprayAngle), Random.Range(-sprayAngle, sprayAngle), 0f);
            Vector3 finalDirection = randomRotation * shootDirection;
            Vector3 spawnPosition = playerCamera.transform.position + finalDirection * 3;
            Quaternion spawnRotation = Quaternion.LookRotation(finalDirection);

            // Create projectile
            GameObject projectileInstance = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
            ServerManager.Spawn(projectileInstance.gameObject);
        }

        // Wait until be able to shoot again
        yield return new WaitForSeconds(timeBetweenShots);

        timeCanShoot = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void ApplyDamage(Transform resultHit, int damage)
    {
        Debug.Log("Apply Damage");
        if (resultHit.CompareTag("Enemy"))
        {
            Debug.Log("TAG");
            if (resultHit.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
            {
                UpdateHealthOnEnemy(enemyHealth, -damage);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateHealthOnEnemy(EnemyHealth enemyHealth, int amountToChange)
    {
        Debug.Log("Update Health");

        if (IsServer)
        {
            enemyHealth.UpdateHealth(amountToChange);
        }
    }
}
