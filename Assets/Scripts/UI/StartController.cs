using UnityEngine;

/// <summary>
/// Owns the Start state: shows the Start screen and hides the gameplay UI until the player
/// taps Play. StartGame() reveals gameplay and kicks off round 1; GoHome() abandons the run
/// and returns to the Start screen. The Home button is NOT part of the gameplay group — it
/// stays visible on every screen (in the Kids Adventure package it returns to the hub).
/// </summary>
public class StartController : MonoBehaviour
{
    public GameManager game;
    public GameObject startScreen;
    public GameObject endSummary;
    [Tooltip("Gameplay UI shown only while playing/rewarding (count chip, banner, progress, answer buttons).")]
    public GameObject[] gameplayUI;

    void Awake()
    {
        ShowStart();
    }

    public void ShowStart()
    {
        if (endSummary != null) endSummary.SetActive(false);
        if (startScreen != null) startScreen.SetActive(true);
        SetGameplay(false);
    }

    public void StartGame()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (endSummary != null) endSummary.SetActive(false);
        SetGameplay(true);
        if (game != null) game.StartGame();
    }

    /// <summary>Home button: stop the run, hide gameplay + reward, show the Start screen.</summary>
    public void GoHome()
    {
        if (game != null) game.StopGame();
        ShowStart();
    }

    void SetGameplay(bool on)
    {
        if (gameplayUI == null) return;
        foreach (var g in gameplayUI)
            if (g != null) g.SetActive(on);
    }
}
