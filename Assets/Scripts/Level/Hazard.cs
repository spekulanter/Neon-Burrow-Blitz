using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int damage = 1;
    public float repeatDelay = 0.5f;
    float nextDamageTime;

    void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextDamageTime)
            return;

        if (other.TryGetComponent(out PlayerHealth player))
        {
            nextDamageTime = Time.time + repeatDelay;
            player.TakeDamage(damage, transform.position);
        }
    }
}
