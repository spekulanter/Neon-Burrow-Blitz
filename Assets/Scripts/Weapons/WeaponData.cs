using UnityEngine;

[CreateAssetMenu(menuName = "Neon Burrow/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "Pulse Blaster";
    public int damage = 1;
    public float fireRate = 0.18f;
    public float projectileSpeed = 18f;
    public float projectileLifetime = 2f;
    public GameObject projectilePrefab;
    public GameObject muzzleFlashPrefab;
    public bool usesAmmo;
    public int projectileCount = 1;
    public float spreadAngle = 0f;
}
