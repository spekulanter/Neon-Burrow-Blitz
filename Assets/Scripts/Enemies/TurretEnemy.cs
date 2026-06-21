using UnityEngine;

public class TurretEnemy : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float range = 12f;
    public float fireInterval = 1.5f;
    public float projectileSpeed = 9f;

    float nextFireTime;

    void Update()
    {
        if (Time.time < nextFireTime || projectilePrefab == null)
            return;

        PlayerController2D target = FindNearestPlayer();
        if (target == null)
            return;

        nextFireTime = Time.time + fireInterval;
        Vector3 spawn = firePoint != null ? firePoint.position : transform.position;
        GameObject projectileObject = Instantiate(projectilePrefab, spawn, Quaternion.identity);
        var projectile = projectileObject.GetComponent<EnemyProjectile>();
        if (projectile != null)
            projectile.Launch((target.transform.position - spawn).normalized, projectileSpeed);
    }

    PlayerController2D FindNearestPlayer()
    {
        PlayerController2D nearest = null;
        float best = range * range;
        foreach (var player in FindObjectsByType<PlayerController2D>(FindObjectsSortMode.None))
        {
            float distance = (player.transform.position - transform.position).sqrMagnitude;
            if (distance < best)
            {
                best = distance;
                nearest = player;
            }
        }
        return nearest;
    }
}
