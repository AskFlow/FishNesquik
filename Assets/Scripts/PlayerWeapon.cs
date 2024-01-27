using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerWeapon : NetworkBehaviour
{
    public enum WeaponType { Classic, Sniper, Shotgun }

    [Header("Situation")]
    public WeaponType currentWeapon;

    public bool sniperUnlocked = false;
    public bool pompeUnlocked = false;
    public bool classicUnlocked = true;

    private PlayerShoot playerShoot;

    [Header("Visuals")]
    public GameObject weaponObject;
    public List<Color> weaponColors;

    [Header("Projectiles")]
    [SerializeField] private GameObject projectileClassic;
    [SerializeField] private GameObject projectileSniper;
    [SerializeField] private GameObject projectilePompe;

    void Start()
    {
        playerShoot = GetComponent<PlayerShoot>();
        SwitchWeapon(WeaponType.Classic);
    }

    public void SwitchWeapon(WeaponType newWeapon)
    {
        if (CanSwitchToWeapon(newWeapon))
        {
            currentWeapon = newWeapon;
            Debug.Log("Switched to " + currentWeapon.ToString());

            UpdateShootParameters();
            UpdateWeaponColor();
        }
        else
        {
            Debug.Log("Cannot switch to " + newWeapon.ToString() + ". Weapon is locked.");
        }
    }

    private void UpdateShootParameters()
    {
        switch (currentWeapon)
        {
            case WeaponType.Classic:
                SetShootParameters(false, 1f, 0.3f, 1, projectileClassic);
                Debug.Log("PARAMETER SET");
                break;
            case WeaponType.Sniper:
                SetShootParameters(true, 0f, 0f, 0, projectileSniper);
                break;
            case WeaponType.Shotgun:
                SetShootParameters(false, 8f, 1f, 5, projectilePompe);
                break;
            default:
                break;
        }
    }

    private void UpdateWeaponColor()
    {
        if (weaponObject != null && weaponColors.Count > 0 && (int)currentWeapon < weaponColors.Count)
        {
            Renderer weaponRenderer = weaponObject.GetComponent<Renderer>();
            if (weaponRenderer != null)
            {
                weaponRenderer.material.color = weaponColors[(int)currentWeapon];
            }
        }
    }

    public bool IsWeaponUnlocked(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Sniper:
                return sniperUnlocked;
            case WeaponType.Shotgun:
                return pompeUnlocked;
            case WeaponType.Classic:
                return classicUnlocked;
            default:
                return false;
        }
    }

    public void UnlockWeapon(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Sniper:
                sniperUnlocked = true;
                break;
            case WeaponType.Shotgun:
                pompeUnlocked = true;
                break;
            case WeaponType.Classic:
                classicUnlocked = true;
                break;
            default:
                break;
        }
    }

    private bool CanSwitchToWeapon(WeaponType weapon)
    {
        return IsWeaponUnlocked(weapon);
    }

    private void SetShootParameters(bool raycast, float sprayAngle, float timeBetweenShots, int numberOfBullets, GameObject projectile)
    {
        playerShoot.SetIsRaycast(raycast);
        playerShoot.SetSprayAngle(sprayAngle);
        playerShoot.SetTimeBetweenShots(timeBetweenShots);
        playerShoot.SetNumberOfBullets(numberOfBullets);
        playerShoot.SetProjectilePrefab(projectile);
    }
}
