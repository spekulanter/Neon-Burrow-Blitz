using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Text scoreText;
    public Text timerText;
    public Text player1Text;
    public Text player2Text;

    void Update()
    {
        var manager = GameManager.Instance;
        if (manager != null)
        {
            if (scoreText != null)
                scoreText.text = $"Score {manager.score}";
            if (timerText != null)
                timerText.text = $"{manager.LevelTime:0.0}s";
        }

        var players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            var weapon = players[i].GetComponent<WeaponController>();
            string text = $"HP {players[i].CurrentHealth}/{players[i].maxHealth}  Lives {players[i].lives}";
            if (weapon != null)
                text += $"  {weapon.ActiveWeaponName}  Rkt {weapon.rocketAmmo}";

            if (i == 0 && player1Text != null)
                player1Text.text = "P1 " + text;
            if (i == 1 && player2Text != null)
                player2Text.text = "P2 " + text;
        }

        if (players.Length < 2 && player2Text != null)
            player2Text.text = "";
    }
}
