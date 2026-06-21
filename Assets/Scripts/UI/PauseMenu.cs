using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject panel;
    bool paused;
    SceneLoader loader;

    void Awake()
    {
        loader = FindFirstObjectByType<SceneLoader>();
        if (panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            SetPaused(!paused);
    }

    public void Resume() => SetPaused(false);
    public void RestartLevel() => loader?.RestartLevel();
    public void MainMenu() => loader?.LoadMainMenu();

    void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = paused ? 0f : 1f;
        if (panel != null)
            panel.SetActive(paused);
    }
}
