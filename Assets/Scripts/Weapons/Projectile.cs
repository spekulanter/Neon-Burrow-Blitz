using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    public int damage = 1;
    public float speed = 18f;
    public float lifetime = 2f;
    public GameObject explosionPrefab;

    int direction = 1;
    GameObject owner;

    public void Launch(int facingDirection, int projectileDamage, float projectileSpeed, float projectileLifetime, GameObject projectileOwner)
    {
        direction = facingDirection >= 0 ? 1 : -1;
        damage = projectileDamage;
        speed = projectileSpeed;
        lifetime = projectileLifetime;
        owner = projectileOwner;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime, Space.World);
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
