using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    public int damage = 1;
    public float speed = 18f;
    public float lifetime = 2f;
    public GameObject explosionPrefab;

    Vector2 velocity = Vector2.right;
    GameObject owner;

    public void Launch(int facingDirection, int projectileDamage, float projectileSpeed, float projectileLifetime, GameObject projectileOwner)
    {
        Launch(Vector2.right * (facingDirection >= 0 ? 1f : -1f), projectileDamage, projectileSpeed, projectileLifetime, projectileOwner);
    }

    public void Launch(Vector2 direction, int projectileDamage, float projectileSpeed, float projectileLifetime, GameObject projectileOwner)
    {
        velocity = direction.normalized * projectileSpeed;
        damage = projectileDamage;
        lifetime = projectileLifetime;
        owner = projectileOwner;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(velocity * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.transform.root.gameObject == owner)
            return;

        if (other.TryGetComponent(out EnemyHealth enemy))
        {
            enemy.TakeDamage(damage);
            SpawnExplosion();
            Destroy(gameObject);
            return;
        }

        if (other.TryGetComponent(out PlayerHealth player))
        {
            player.TakeDamage(damage, transform.position);
            SpawnExplosion();
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            SpawnExplosion();
            Destroy(gameObject);
        }
    }

    void SpawnExplosion()
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
