using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerWeapon : NetworkBehaviour
{
    public enum WeaponType { Classic, Sniper, Pompe }

    [Header("Situation")]
    public WeaponType currentWeapon;


    public PlayerShoot playerShoot;

    [Header("Visuals")]
    public GameObject weaponObject;

    [Header("Projectiles")]
    [SerializeField] private GameObject projectileType;




    // Méthode pour changer d'arme
    public void SwitchWeapon()
    {
        UpdateShootParameters();
    }

    // Méthode pour mettre à jour les paramètres de tir en fonction de l'arme actuelle
    private void UpdateShootParameters()
    {

        switch (currentWeapon)
        {
            case WeaponType.Classic:
                SetShootParameters(false, 1f, 0.3f, 1, projectileType);
                break;
            case WeaponType.Sniper:
                SetShootParameters(true, 0f, 0f, 0, projectileType);
                break;
            case WeaponType.Pompe:
                SetShootParameters(false, 8f, 1f, 5, projectileType);
                break;
            default:
                break;
        }
    }

    public WeaponType getWeaponClass()
    {
        return currentWeapon;
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
