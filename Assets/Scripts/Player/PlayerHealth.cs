using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int lives = 3;
    public float invulnerabilityAfterHit = 1f;
    public int CurrentHealth { get; private set; }

    bool invulnerable;
    Animator animator;
    PlayerRespawn respawn;
    PlayerController2D controller;

    void Awake()
    {
        CurrentHealth = maxHealth;
        animator = GetComponent<Animator>();
        respawn = GetComponent<PlayerRespawn>();
        controller = GetComponent<PlayerController2D>();
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + Mathf.Max(0, amount));
    }

    public void AddLife(int amount)
    {
        lives += Mathf.Max(0, amount);
    }

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (invulnerable || amount <= 0)
            return;

        CurrentHealth -= amount;
        animator?.SetTrigger("Hurt");
        Vector2 direction = ((Vector2)transform.position - sourcePosition).normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.up;
        controller?.ApplyKnockback(new Vector2(direction.x * 7f, 7f));

        if (CurrentHealth <= 0)
            LoseLife();
        else
            StartCoroutine(InvulnerabilityRoutine());
    }

    void LoseLife()
    {
        lives--;
        if (lives < 0)
        {
            animator?.SetBool("IsDead", true);
            gameObject.SetActive(false);
            return;
        }

        CurrentHealth = maxHealth;
        respawn?.Respawn();
        StartCoroutine(InvulnerabilityRoutine());
    }

    IEnumerator InvulnerabilityRoutine()
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerabilityAfterHit);
        invulnerable = false;
    }
}
