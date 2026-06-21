using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public const string MainMenuScene = "MainMenu";
    public const string Level01Scene = "Level_01_CrystalBurrow";

    public void StartOnePlayer()
    {
        EnsureSession().SetPlayerCount(1);
        SceneManager.LoadScene(Level01Scene);
    }

    public void StartTwoPlayers()
    {
        EnsureSession().SetPlayerCount(2);
        SceneManager.LoadScene(Level01Scene);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuScene);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static GameSession EnsureSession()
    {
        if (GameSession.Instance != null)
            return GameSession.Instance;

        var go = new GameObject("GameSession");
        return go.AddComponent<GameSession>();
    }
}
