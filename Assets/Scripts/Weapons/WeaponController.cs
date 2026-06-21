using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform firePoint;
    public WeaponData pulseBlaster;
    public WeaponData sparkRocket;
    public int rocketAmmo;
    public bool sparkRocketUnlocked;
    public string ActiveWeaponName => usingRocket ? "Spark Rocket" : "Pulse Blaster";

    PlayerController2D player;
    Animator animator;
    bool usingRocket;
    float nextFireTime;

    void Awake()
    {
        player = GetComponent<PlayerController2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null)
            return;

        if (player.input.SwitchWeaponPressed && sparkRocketUnlocked)
            usingRocket = !usingRocket;

        if (player.input.ShootPressed)
            TryFire();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.f2Key.wasPressedThisFrame)
            UnlockSparkRocket(10);
#endif
    }

    public void UnlockSparkRocket(int ammo)
    {
        sparkRocketUnlocked = true;
        rocketAmmo += ammo;
        usingRocket = true;
    }

    public void AddRocketAmmo(int amount)
    {
        rocketAmmo += Mathf.Max(0, amount);
    }

    void TryFire()
    {
        WeaponData data = usingRocket && sparkRocketUnlocked ? sparkRocket : pulseBlaster;
        if (data == null || data.projectilePrefab == null || Time.time < nextFireTime)
            return;

        if (data.usesAmmo)
        {
            if (rocketAmmo <= 0)
            {
                usingRocket = false;
                return;
            }
            rocketAmmo--;
        }

        nextFireTime = Time.time + data.fireRate;
        animator?.SetTrigger("Shoot");

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + Vector3.right * player.FacingDirection;
        GameObject projectileObject = Instantiate(data.projectilePrefab, spawnPosition, Quaternion.identity);
        var projectile = projectileObject.GetComponent<Projectile>();
        if (projectile != null)
            projectile.Launch(player.FacingDirection, data.damage, data.projectileSpeed, data.projectileLifetime, gameObject);
    }
}
