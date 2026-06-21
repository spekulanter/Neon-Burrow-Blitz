using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 2;
    public int scoreOnDeath = 50;
    int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= Mathf.Max(1, amount);
        if (currentHealth <= 0)
        {
            GameManager.Instance?.AddScore(scoreOnDeath);
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out PlayerHealth player))
            player.TakeDamage(1, transform.position);
    }
}
