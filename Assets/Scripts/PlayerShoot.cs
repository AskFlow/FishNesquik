using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    [SyncVar] public int health = 100;
    [SerializeField] private bool isServerAuth;
    [SerializeField] private int damage = 10;
    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private string tagToHit;

    public Camera playerCamera;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (isServerAuth)
                ServerShoot(playerCamera.transform.position, Camera.main.transform.forward);
            else {
                if (playerCamera == null) 
                { 
                    playerCamera = gameObject.GetComponent<Camera>();
                }
                LocalShoot();
            }
        }
    }

    [ServerRpc]
    void ServerShoot(Vector3 camPosition, Vector3 shootForward, NetworkConnection sender = null)
    {
        // Last part is to have the shot be in front of our own player and not inside of him
        Vector3 shotPosition = camPosition + shootForward * 0.5f;
        if (Physics.Raycast(shotPosition, shootForward, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.transform.TryGetComponent(out PlayerShoot otherPlayer) && otherPlayer != this)
            {
                // We've hit another player that isn't the person shooting
                otherPlayer.health -= damage;
                HitConfirmation(sender, otherPlayer.Owner.ClientId, hit.point);
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
            if (resultHit.parent.CompareTag(tagToHit) && resultHit != this.gameObject)
            {
                resultHit.parent.TryGetComponent(out PlayerShoot health);
                health.DamagePlayer(damage);
                Instantiate(hitParticle, hit.point, Quaternion.identity);
                //Debug.Log($"You've hit player {otherPlayer.Owner.ClientId} for {damage}. Calculations done locally");
            }

        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DamagePlayer(int damage)
    {
        health -= damage;
    }
}
