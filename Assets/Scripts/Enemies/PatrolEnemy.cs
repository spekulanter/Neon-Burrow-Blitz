using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolEnemy : MonoBehaviour
{
    public float speed = 2f;
    public Transform groundProbe;
    public Transform wallProbe;
    public LayerMask groundMask;

    Rigidbody2D rb;
    int direction = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        bool wall = wallProbe != null && Physics2D.OverlapCircle(wallProbe.position, 0.12f, groundMask);
        bool ledge = groundProbe != null && !Physics2D.OverlapCircle(groundProbe.position, 0.12f, groundMask);
        if (wall || ledge)
            Flip();
    }

    void Flip()
    {
        direction *= -1;
        var scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }
}
