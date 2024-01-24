using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [SyncVar]
    public int health = 100;

    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float timeBeforeDestroy = 3.0f;


    private void Update()
    {
        if (health <= 0)
        {
            StartCoroutine(DeathCoroutine());
        }
    }


  

    public IEnumerator DeathCoroutine()
    {
        animator.SetBool("IsDead", true);
        if(TryGetComponent(out EnemyKamikaze stopMovementKamikaze))
        {
            stopMovementKamikaze.enabled = false;

        }
        if(TryGetComponent(out EnemyDistance stopMovementDistance))
        {
            stopMovementDistance.enabled = false;
        }
        if (TryGetComponent(out EnemyBoss stopMovementBoss))
        {
            stopMovementBoss.enabled = false;
        }
        Canvas healthUI = GetComponentInChildren<Canvas>();
        healthUI.enabled = false;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
        {
            yield return null;
        }
        yield return new WaitForSeconds(timeBeforeDestroy);

        if (IsServer)
        {
            if (gameObject != null && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                ServerManager.Despawn(gameObject);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealth(int amountToChange)
    {
        health += amountToChange;

    }
}
