using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Text bestScoreText;
    public Text bestTimeText;
    SceneLoader loader;

    void Awake()
    {
        loader = FindFirstObjectByType<SceneLoader>();
    }

    void Start()
    {
        int bestScore = PlayerPrefs.GetInt("Level01_BestScore", 0);
        float bestTime = PlayerPrefs.GetFloat("Level01_BestTime", 0f);
        if (bestScoreText != null)
            bestScoreText.text = $"Best Score: {bestScore}";
        if (bestTimeText != null)
            bestTimeText.text = bestTime > 0f ? $"Best Time: {bestTime:0.0}s" : "Best Time: --";
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            StartOnePlayer();
        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            StartTwoPlayers();
        if (keyboard.escapeKey.wasPressedThisFrame)
            Quit();
    }

    public void StartOnePlayer() => loader?.StartOnePlayer();
    public void StartTwoPlayers() => loader?.StartTwoPlayers();
    public void Quit() => loader?.Quit();
}
