using UnityEngine;
using FishNet.Object;
using TMPro;

public class WeaponUnlocker : NetworkBehaviour
{
    public TextMeshProUGUI weaponName;
    public PlayerWeapon.WeaponType weaponTypeToUnlock;

    [Header("Audio Settings")]
    public AudioClip pickupSound;

    private AudioSource audioSource;

    public float rotationSpeed = 150f;

    private void Start()
    {
        weaponName.text = weaponTypeToUnlock.ToString();
        if (IsServer)
        {
            //UpdateWeaponNameClientRpc(weaponName.text);
            audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.clip = pickupSound;
        }
    }

    private void Update()
    {
        // Faites tourner l'objet sur place vers la droite à chaque frame
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.CompareTag("Player"))
        {
            PlayerWeapon playerWeapon = other.GetComponent<PlayerWeapon>();

            if (playerWeapon != null)
            {
                if (audioSource != null)
                {
                    audioSource.Play();
                }

                if(!playerWeapon.IsWeaponUnlocked(weaponTypeToUnlock))
                {
                    UnlockWeaponClientRpc(playerWeapon, weaponTypeToUnlock);

                    gameObject.SetActive(false);
                    ServerManager.Despawn(gameObject);
                }
            }
        }
    }

    [ObserversRpc]
    void UnlockWeaponClientRpc(PlayerWeapon playerWeapon, PlayerWeapon.WeaponType weaponType)
    {
        // Côté client
        playerWeapon.UnlockWeapon(weaponType);
    }

    [ObserversRpc]
    void UpdateWeaponNameClientRpc(string newName)
    {
        // Côté client
        if (weaponName != null)
        {
            weaponName.text = newName;
        }
    }
}
