using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform firePoint;
    public WeaponData pulseBlaster;
    public WeaponData sparkRocket;
    public WeaponData[] weapons;
    public int rocketAmmo;
    public bool sparkRocketUnlocked;
    public string ActiveWeaponName => CurrentWeapon != null ? CurrentWeapon.weaponName : "None";

    PlayerController2D player;
    Animator animator;
    int activeWeaponIndex;
    float nextFireTime;
    WeaponData CurrentWeapon
    {
        get
        {
            if (weapons != null && weapons.Length > 0)
                return weapons[Mathf.Clamp(activeWeaponIndex, 0, weapons.Length - 1)];
            return sparkRocketUnlocked && activeWeaponIndex == 1 ? sparkRocket : pulseBlaster;
        }
    }

    void Awake()
    {
        player = GetComponent<PlayerController2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null)
            return;

        if (player.input.SwitchWeaponPressed)
            CycleWeapon();

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
        SelectWeaponNamed("Spark Rocket");
    }

    public void AddRocketAmmo(int amount)
    {
        rocketAmmo += Mathf.Max(0, amount);
    }

    void TryFire()
    {
        WeaponData data = CurrentWeapon;
        if (data == null || data.projectilePrefab == null || Time.time < nextFireTime)
            return;

        if (data.usesAmmo)
        {
            if (rocketAmmo <= 0)
            {
                SelectFirstFreeWeapon();
                return;
            }
            rocketAmmo--;
        }

        nextFireTime = Time.time + data.fireRate;
        animator?.SetTrigger("Shoot");

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + Vector3.right * player.FacingDirection;
        if (data.muzzleFlashPrefab != null)
            Instantiate(data.muzzleFlashPrefab, spawnPosition, Quaternion.identity);

        int count = Mathf.Max(1, data.projectileCount);
        float startAngle = count == 1 ? 0f : -data.spreadAngle * 0.5f;
        float step = count == 1 ? 0f : data.spreadAngle / (count - 1);
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.right;
            direction.x *= player.FacingDirection;
            GameObject projectileObject = Instantiate(data.projectilePrefab, spawnPosition, Quaternion.identity);
            var projectile = projectileObject.GetComponent<Projectile>();
            if (projectile != null)
                projectile.Launch(direction, data.damage, data.projectileSpeed, data.projectileLifetime, gameObject);
        }
    }

    void CycleWeapon()
    {
        if (weapons == null || weapons.Length == 0)
        {
            if (sparkRocketUnlocked)
                activeWeaponIndex = activeWeaponIndex == 0 ? 1 : 0;
            return;
        }

        for (int i = 1; i <= weapons.Length; i++)
        {
            int next = (activeWeaponIndex + i) % weapons.Length;
            if (CanUseWeapon(weapons[next]))
            {
                activeWeaponIndex = next;
                return;
            }
        }
    }

    bool CanUseWeapon(WeaponData data)
    {
        if (data == null)
            return false;
        if (!data.usesAmmo)
            return true;
        return sparkRocketUnlocked;
    }

    void SelectFirstFreeWeapon()
    {
        if (weapons == null)
        {
            activeWeaponIndex = 0;
            return;
        }
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && !weapons[i].usesAmmo)
            {
                activeWeaponIndex = i;
                return;
            }
        }
    }

    void SelectWeaponNamed(string weaponName)
    {
        if (weapons == null)
            return;
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && weapons[i].weaponName == weaponName)
            {
                activeWeaponIndex = i;
                return;
            }
        }
    }
}
