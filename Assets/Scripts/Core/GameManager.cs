using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the round loop. The game waits on the Start screen until StartGame() is called
/// (by the Start button). For each of N rounds it resets the count, scatters that round's
/// fruit, poses the matching question, and updates the HUD. A correct answer awards a star,
/// cheers the mascot, and advances; after the last round it fires OnGameComplete + a fanfare.
/// The Home button calls StopGame() to abandon the run and return to the Start screen.
/// </summary>
public class GameManager : MonoBehaviour
{
    public FruitSpawner spawner;
    public AnswerPanel answerPanel;
    public CountManager countManager;
    public HudController hud;
    [Tooltip("Gameplay field hidden while the end summary is up (round banner, count chip, progress, answer buttons) so nothing draws over or bleeds through the reward card. HomeButton is intentionally NOT listed — it stays available on the reward screen.")]
    public GameObject[] hideOnComplete;

    [Tooltip("Fruit count per round — the difficulty ramp.")]
    public int[] roundCounts = { 3, 4, 5, 6, 7 };
    public float nextRoundDelay = 1.5f;
    [Tooltip("Start round 1 immediately on load (false = wait for the Start screen).")]
    public bool autoStart = false;

    [Header("Audio")]
    public AudioClip fanfareClip;
    [Tooltip("Cheerful jingle played when a playthrough begins (Play button / Play again).")]
    public AudioClip startClip;

    public int CurrentRound { get; private set; }
    public int Stars { get; private set; }
    public bool Complete { get; private set; }
    public bool Running { get; private set; }
    public System.Action<int, int> OnGameComplete; // (stars, totalRounds)

    void Start()
    {
        if (answerPanel != null) answerPanel.OnSolved += HandleCorrect;
        if (autoStart) StartGame();
    }

    void OnDestroy()
    {
        if (answerPanel != null) answerPanel.OnSolved -= HandleCorrect;
    }

    /// <summary>Begin a fresh playthrough at round 1 (Start screen's Play button / replay).</summary>
    public void StartGame()
    {
        StopAllCoroutines();
        Complete = false;
        Running = true;
        Stars = 0;
        SfxPlayer.Play(startClip);
        SetFieldActive(true);
        if (hud != null) hud.SetStars(0);
        StartRound(0);
    }

    /// <summary>Abandon the current run and clear the field (Home button).</summary>
    public void StopGame()
    {
        StopAllCoroutines();
        Running = false;
        Complete = false;
        if (spawner != null) spawner.Clear();
    }

    void StartRound(int index)
    {
        CurrentRound = index;
        int n = roundCounts[Mathf.Clamp(index, 0, roundCounts.Length - 1)];
        if (countManager != null) countManager.ResetCount();
        if (spawner != null) spawner.Spawn(n);
        if (answerPanel != null) answerPanel.SetQuestion(n);
        if (hud != null) hud.SetRound(index + 1, roundCounts.Length);
    }

    void HandleCorrect()
    {
        if (!Running) return;
        Stars++;
        if (hud != null) { hud.SetStars(Stars); hud.Cheer(); }
        StartCoroutine(NextRoundAfterDelay());
    }

    IEnumerator NextRoundAfterDelay()
    {
        yield return new WaitForSeconds(nextRoundDelay);
        int next = CurrentRound + 1;
        if (next < roundCounts.Length) StartRound(next);
        else EndGame();
    }

    void EndGame()
    {
        Complete = true;
        Running = false;
        // Clear the gameplay field so nothing draws over the end summary (world fruit would
        // otherwise punch through the Screen Space - Camera HUD; the chips/banner would bleed
        // through the scrim). Mirrors the Home/back field-clear, but keeps the Home button.
        if (spawner != null) spawner.Clear();
        SetFieldActive(false);
        SfxPlayer.Play(fanfareClip);
        OnGameComplete?.Invoke(Stars, roundCounts.Length);
    }

    void SetFieldActive(bool on)
    {
        if (hideOnComplete == null) return;
        foreach (var go in hideOnComplete)
            if (go != null) go.SetActive(on);
    }

    /// <summary>Restart from round 1 (used by the end-summary replay button).</summary>
    public void Restart() => StartGame();
}
