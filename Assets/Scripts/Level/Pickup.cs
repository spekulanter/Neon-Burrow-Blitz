using UnityEngine;

public enum PickupType
{
    CrystalShard,
    BigCrystal,
    HealthCell,
    RocketAmmo,
    ExtraLife,
    SparkRocket
}

public class Pickup : MonoBehaviour
{
    public PickupType pickupType;
    public int amount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerHealth health))
            return;

        var weapon = other.GetComponent<WeaponController>();
        switch (pickupType)
        {
            case PickupType.CrystalShard:
                GameManager.Instance?.AddScore(10, true);
                break;
            case PickupType.BigCrystal:
                GameManager.Instance?.AddScore(100, true);
                break;
            case PickupType.HealthCell:
                health.Heal(amount);
                break;
            case PickupType.RocketAmmo:
                weapon?.AddRocketAmmo(5);
                break;
            case PickupType.ExtraLife:
                health.AddLife(1);
                break;
            case PickupType.SparkRocket:
                weapon?.UnlockSparkRocket(5);
                break;
        }

        Destroy(gameObject);
    }
}
