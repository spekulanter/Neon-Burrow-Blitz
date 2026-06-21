using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Sprite inactiveSprite;
    public Sprite activeSprite;
    public Transform respawnPoint;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && inactiveSprite != null)
            spriteRenderer.sprite = inactiveSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerRespawn respawn))
            return;

        Vector3 point = respawnPoint != null ? respawnPoint.position : transform.position + Vector3.up;
        respawn.SetCheckpoint(point);
        if (spriteRenderer != null && activeSprite != null)
            spriteRenderer.sprite = activeSprite;
    }
}
