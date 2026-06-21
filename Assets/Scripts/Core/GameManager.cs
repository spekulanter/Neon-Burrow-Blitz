using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int score;
    public int crystalsCollected;
    public bool levelComplete;
    public float LevelTime { get; private set; }

    [SerializeField] LevelCompleteUI levelCompleteUI;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!levelComplete)
            LevelTime += Time.deltaTime;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard.f1Key.wasPressedThisFrame)
                FindFirstObjectByType<SceneLoader>()?.RestartLevel();
            if (keyboard.f5Key.wasPressedThisFrame)
                AddScore(1000);
        }
#endif
    }

    public void AddScore(int amount, bool crystal = false)
    {
        score += Mathf.Max(0, amount);
        if (crystal)
            crystalsCollected++;
    }

    public void CompleteLevel()
    {
        if (levelComplete)
            return;

        levelComplete = true;
        int bestScore = PlayerPrefs.GetInt("Level01_BestScore", 0);
        float bestTime = PlayerPrefs.GetFloat("Level01_BestTime", 0f);

        if (score > bestScore)
            PlayerPrefs.SetInt("Level01_BestScore", score);
        if (bestTime <= 0f || LevelTime < bestTime)
            PlayerPrefs.SetFloat("Level01_BestTime", LevelTime);

        PlayerPrefs.Save();
        if (levelCompleteUI == null)
            levelCompleteUI = FindFirstObjectByType<LevelCompleteUI>(FindObjectsInactive.Include);
        levelCompleteUI?.Show();
    }
}
