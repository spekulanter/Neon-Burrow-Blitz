using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public Transform player1Spawn;
    public Transform player2Spawn;
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    bool spawned;

    void Awake()
    {
        SpawnPlayers();
    }

    void Start()
    {
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        if (spawned)
            return;

        int playerCount = GameSession.Instance != null ? GameSession.Instance.playerCount : GameSession.PlayerCount;
        if (player1Prefab != null && player1Spawn != null)
            PrepareSpawnedPlayer(Instantiate(player1Prefab, player1Spawn.position, Quaternion.identity));
        if (playerCount >= 2 && player2Prefab != null && player2Spawn != null)
            PrepareSpawnedPlayer(Instantiate(player2Prefab, player2Spawn.position, Quaternion.identity));

        spawned = true;
    }

    static void PrepareSpawnedPlayer(GameObject player)
    {
        if (player == null)
            return;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.WakeUp();
        }
    }
}
