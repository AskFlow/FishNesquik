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
            PlayerGrenade playerGrenade = other.GetComponent<PlayerGrenade>();

            if (playerGrenade != null)
            {
                if (audioSource != null)
                {
                    audioSource.Play();
                }

                RechargeGrenadesClientRpc(playerGrenade, grenadesToGive);

                // delete the object
                gameObject.SetActive(false);
                ServerManager.Despawn(gameObject);
            }
        }
    }

    [ObserversRpc]
    void RechargeGrenadesClientRpc(PlayerGrenade playerGrenade, int amount)
    {
        // client side
        playerGrenade.RechargeGrenades(amount);
    }
}
