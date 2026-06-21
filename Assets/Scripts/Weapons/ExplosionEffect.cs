using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float lifetime = 0.25f;
    public float growSpeed = 6f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
    }
}
