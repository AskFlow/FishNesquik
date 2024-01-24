using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private bool isServerAuth;
    [SerializeField] private int damage = 10;
    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private string tagToHit;

    public Camera playerCamera;

    [Header("Shoot style")]
    private bool isRaycast = false;

    [Header("Grenades")]
    public FragGrenade grenadePrefab;
    public int maxGrenadeCount = 3;
    private int currentGrenadeCount;



    private void Start()
    {
        // Initialise nb of grenades
        currentGrenadeCount = maxGrenadeCount;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!isRaycast)
                ShootPhysic();
            else
            {
                if (isServerAuth)
                {

                    ServerShoot(playerCamera.transform.position, Camera.main.transform.forward);
                }
                else
                {
                    if (playerCamera == null)
                    {
                        playerCamera = gameObject.GetComponent<Camera>();
                    }
                    LocalShoot();
                }
            }

            
        }

        // throw grenade
        if (Input.GetKeyDown(KeyCode.G) && currentGrenadeCount > 0)
        {
            Debug.Log("G");
            ThrowGrenadeServerRpc();
        }
    }

    [ServerRpc]
    void ServerShoot(Vector3 camPosition, Vector3 shootForward, NetworkConnection sender = null)
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

    void LocalShoot()
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
    void ShootPhysic()
    {
        if (projectilePrefab != null)
        {
            // use camera rotation to have the direction
            Vector3 shootDirection = playerCamera.transform.forward;
            Vector3 spawnPosition = playerCamera.transform.position + shootDirection * 3;

            Quaternion spawnRotation = Quaternion.LookRotation(shootDirection);

            GameObject projectileInstance = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
            ServerManager.Spawn(projectileInstance.gameObject);
        }
    }




    [ServerRpc(RequireOwnership = false)]
    void ThrowGrenadeServerRpc()
    {
        Debug.Log("Throw grenade");
        // Server side
        if (currentGrenadeCount <= 0)
            return;

        if (grenadePrefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 6.0f;

            FragGrenade grenadeInstance = Instantiate(grenadePrefab, spawnPosition, Quaternion.identity);
            currentGrenadeCount--;

            // Replicates grenade
            ServerManager.Spawn(grenadeInstance.gameObject);
        }
    }

    [Client]
    public void RechargeGrenades(int amount)
    {
        currentGrenadeCount = Mathf.Min(currentGrenadeCount + amount, maxGrenadeCount);
        // Debug.Log("Grenades rechargées. Nombre actuel de grenades : " + currentGrenadeCount);
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
