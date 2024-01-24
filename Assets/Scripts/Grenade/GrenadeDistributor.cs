using UnityEngine;
using FishNet.Object;

public class GrenadeDistributor : NetworkBehaviour
{
    public int grenadesToGive = 1;

    [Header("Audio Settings")]
    public AudioClip pickupSound;

    private AudioSource audioSource;

    private void Start()
    {
        if (IsServer)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.clip = pickupSound;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerController != null)
            {
                if (audioSource != null)
                {
                    audioSource.Play();
                }

                RechargeGrenadesClientRpc(playerController, grenadesToGive);

                // delete the object
                gameObject.SetActive(false);
                ServerManager.Despawn(gameObject);
            }
        }
    }

    [ObserversRpc]
    void RechargeGrenadesClientRpc(PlayerController playerController, int amount)
    {
        // client side
        playerController.RechargeGrenades(amount);
    }
}
