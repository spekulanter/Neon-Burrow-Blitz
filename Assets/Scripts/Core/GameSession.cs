using UnityEngine;

public sealed class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }
    public static int PlayerCount { get; set; } = 1;

    public int playerCount = 1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerCount = Mathf.Clamp(PlayerCount, 1, 2);
    }

    public void SetPlayerCount(int count)
    {
        playerCount = Mathf.Clamp(count, 1, 2);
        PlayerCount = playerCount;
    }
}
