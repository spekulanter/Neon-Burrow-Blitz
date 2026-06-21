using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour
{
    public GameObject panel;
    public Text summaryText;
    SceneLoader loader;

    void Awake()
    {
        loader = FindFirstObjectByType<SceneLoader>();
        if (panel != null)
            panel.SetActive(false);
    }

    public void Show()
    {
        if (panel != null)
            panel.SetActive(true);

        var manager = GameManager.Instance;
        if (summaryText != null && manager != null)
        {
            int bestScore = PlayerPrefs.GetInt("Level01_BestScore", 0);
            float bestTime = PlayerPrefs.GetFloat("Level01_BestTime", 0f);
            summaryText.text = $"Score: {manager.score}\nCrystals: {manager.crystalsCollected}\nTime: {manager.LevelTime:0.0}s\nBest Score: {bestScore}\nBest Time: {bestTime:0.0}s";
        }
    }

    public void Restart() => loader?.RestartLevel();
    public void MainMenu() => loader?.LoadMainMenu();
}
