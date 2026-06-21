using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform target;
    public float parallaxFactor = 0.25f;
    Vector3 previousTargetPosition;

    void Start()
    {
        if (target == null && Camera.main != null)
            target = Camera.main.transform;
        previousTargetPosition = target != null ? target.position : Vector3.zero;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 delta = target.position - previousTargetPosition;
        transform.position += new Vector3(delta.x * parallaxFactor, delta.y * parallaxFactor, 0f);
        previousTargetPosition = target.position;
    }
}
