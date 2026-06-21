using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 4f;
    Vector2 velocity;

    public void Launch(Vector2 direction, float speed)
    {
        velocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(velocity * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerHealth player))
        {
            player.TakeDamage(damage, transform.position);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
