using FishNet.Object;
using UnityEngine;

public class ProjectileEnemy : NetworkBehaviour
{
    [SerializeField]
    private float speed = 50f;
    [SerializeField]
    private int damage = 10;
    [SerializeField]
    private float lifetime = 3f;
    [SerializeField]
    private ParticleSystem hitParticle;

    private Rigidbody rb;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);

        rb.drag = 0.2f;

        Invoke("Despawn", lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            ApplyDamage(other.gameObject, damage);

            // particules effect
            if (hitParticle != null)
            {
                Vector3 particlesPosition = other.gameObject.transform.position + new Vector3(0, 1f, 0);
                Instantiate(hitParticle, particlesPosition, Quaternion.identity);
            }
        }

        if (other.CompareTag("Enemy"))
        {
            Despawn();
        }

    }

    [Server]
    private void ApplyDamage(GameObject resultHit, int damage)
    {
        if (resultHit.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
        {
            playerHealth.UpdateHealth(playerHealth , -damage);
        }
    }

    private void Despawn()
    {
        if (gameObject != null && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            ServerManager.Despawn(gameObject);
        }
    }
}
