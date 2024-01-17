using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine.XR;

public class EnemyKamikaze : NetworkBehaviour
{
    public float speed = 5f;
    public float activationDistance = 2f;
    public float explosionRadius = 5f;
    public int explosionDamage = 20;

    // verification bool
    private bool hasExploded = false;

    public Transform nearestPlayer = null;

    private void Update()
    {
        if (IsServer)
        {
            nearestPlayer = ChaseNearestPlayer();
            CheckActivationDistance(nearestPlayer);
        }
    }

    private Transform ChaseNearestPlayer()
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

        if (nearestPlayer != null)
        {
            Vector3 direction = nearestPlayer.position - transform.position;
            direction.Normalize();
            transform.Translate(direction * speed * Time.deltaTime);
        }

        return nearestPlayer;
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

    void Suicide()
    {
        Collider[] players = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider playerCollider in players)
        {
            if (playerCollider.CompareTag("Player"))
            {
                Debug.Log("Player detected at position: " + playerCollider.transform.position);

                if (playerCollider.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
                {
                    Debug.Log("Player health update!");
                    UpdateHealthOnServer(playerHealth, -explosionDamage);
                }
            }
        }

        if (IsServer)
        {
            StartCoroutine(DelayedDespawn());
        }
    }

    IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(0.1f);
        ServerManager.Despawn(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateHealthOnServer(PlayerHealth playerHealth, int amountToChange)
    {
        if (IsServer)
        {
            Debug.Log("set Life");
            playerHealth.UpdateHealth(playerHealth, amountToChange);
        }
    }

}
