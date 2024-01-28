using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerRevive : NetworkBehaviour
{
    [SyncVar] public bool canRevive = false;
    private bool isReviving = false;
    private float reviveTimer = 0f;
    private float reviveDuration = 2f;
    private PlayerController targetPlayer;

    public void TryRevive()
    {
        if (canRevive && targetPlayer != null)
        {
            if (isReviving)
            {
                reviveTimer += Time.deltaTime;

                if (reviveTimer >= reviveDuration)
                {
                    Debug.Log("REVIVED");

                    ReviveTargetPlayer();
                }
            }
        }
    }

    private void Update()
    {
        // start revive
        if (Input.GetKeyDown(KeyCode.E) && canRevive)
        {
            Debug.Log("reviving...");
            isReviving = true;
        }

        // stop revive
        if (Input.GetKeyUp(KeyCode.E) && canRevive)
        {
            Debug.Log("stop revive!");
            isReviving = false;
            reviveTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Debug.Log("PlayerController found.");
                playerController.canRevive = true;
            }
            else
            {
                Debug.LogWarning("PlayerController not found on parent object.");
            }
        }
    }



    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.transform.parent != null && other.transform.parent.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            canRevive = false;
            reviveTimer = 0f;
            targetPlayer = null;
        }
    }

    private void ReviveTargetPlayer()
    {
        Debug.Log("Revive!");

        targetPlayer.Revive();
        canRevive = false;
        reviveTimer = 0f;
        targetPlayer = null;
    }
}
