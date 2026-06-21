using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public float smoothTime = 0.15f;
    public float minX;
    public float maxX = 200f;
    public float minY = 0f;
    public float maxY = 30f;
    public float baseSize = 6.5f;
    public float maxSize = 9f;

    Camera cam;
    Vector3 velocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        var players = FindObjectsByType<PlayerController2D>(FindObjectsSortMode.None);
        if (players.Length == 0)
            return;

        Vector3 center = Vector3.zero;
        foreach (var player in players)
            center += player.transform.position;
        center /= players.Length;

        float distance = 0f;
        if (players.Length > 1)
            distance = Vector3.Distance(players[0].transform.position, players[1].transform.position);

        Vector3 target = new Vector3(Mathf.Clamp(center.x, minX, maxX), Mathf.Clamp(center.y + 1.5f, minY, maxY), transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);

        if (cam != null)
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, Mathf.Clamp(baseSize + distance * 0.15f, baseSize, maxSize), Time.deltaTime * 4f);
    }
}
