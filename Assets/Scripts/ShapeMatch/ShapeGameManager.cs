using System.Collections;
using UnityEngine;

namespace ShapeMatch
{
    /// <summary>
    /// Drives Shape Match: Start screen → 5 matching rounds → Reward screen, with a Home
    /// button that returns to Start at any time. Each round shows one target shape and three
    /// answer tiles (one matching). A correct match fills a star and advances; wrong taps stay
    /// in the round (gentle, no-fail). After 5 correct rounds the reward screen shows "Great
    /// job!" + 5 stars + Play Again + Home. Self-contained — shares no code with CountTheFruits.
    /// </summary>
    public class ShapeGameManager : MonoBehaviour
    {
        [Header("Gameplay refs")]
        public ShapeTargetView target;
        public ShapeAnswerPanel answerPanel;
        public ShapeHud hud;

        [Header("Screens (toggled by state)")]
        [Tooltip("Title + Play button. Shown only on the Start screen.")]
        public GameObject startScreen;
        [Tooltip("Banner + target + answer tiles + progress. Shown only during rounds.")]
        public GameObject gameplayRoot;
        [Tooltip("Great job! + 5 stars + Play Again. Shown only on the Reward screen.")]
        public GameObject rewardPanel;
        [Tooltip("Home button — visible during rounds and the reward screen, hidden on Start.")]
        public GameObject homeButton;

        [Header("Rules")]
        public int totalRounds = 5;
        public float nextRoundDelay = 0.9f;
        [Tooltip("Start the first round immediately instead of waiting on the Start screen.")]
        public bool autoStart = false;
        [Tooltip("If >= 0, shapes are picked from this fixed seed (deterministic verification).")]
        public int randomSeed = -1;

        [Header("Audio")]
        public AudioClip startClip;
        public AudioClip fanfareClip;

        public int Completed { get; private set; }
        public bool Running { get; private set; }
        public System.Action<int> OnGameComplete; // (rounds completed)

        System.Random _rng;

        void Start()
        {
            if (answerPanel != null) answerPanel.OnAnswered += HandleAnswer;
            if (autoStart) StartGame();
            else ShowStart();
        }

        void OnDestroy()
        {
            if (answerPanel != null) answerPanel.OnAnswered -= HandleAnswer;
        }

        // ---- screen states -------------------------------------------------
        void SetState(bool start, bool game, bool reward, bool home)
        {
            if (startScreen) startScreen.SetActive(start);
            if (gameplayRoot) gameplayRoot.SetActive(game);
            if (rewardPanel) rewardPanel.SetActive(reward);
            if (homeButton) homeButton.SetActive(home);
        }

        void ShowStart()
        {
            Running = false;
            StopAllCoroutines();
            // home stays visible: in the Kids Adventure package it returns to the hub
            SetState(true, false, false, true);
        }

        /// <summary>Begin a fresh playthrough (Play / Play Again).</summary>
        public void StartGame()
        {
            StopAllCoroutines();
            Running = true;
            Completed = 0;
            _rng = randomSeed >= 0 ? new System.Random(randomSeed) : null;
            SetState(false, true, false, true);
            if (hud != null) hud.SetProgress(0);
            ShapeSfx.Play(startClip);
            StartRound();
        }

        /// <summary>Abandon the run and go back to the Start screen (Home button).</summary>
        public void GoHome() => ShowStart();

        /// <summary>Replay from round 1 (reward screen Play Again).</summary>
        public void Restart() => StartGame();

        void StartRound()
        {
            ShapeKind correct = PickKind();
            ShapeKind d1 = PickOther(correct);
            ShapeKind d2 = PickOther(correct, d1);
            if (target != null) target.SetShape(correct);
            if (answerPanel != null) answerPanel.SetQuestion(correct, d1, d2);
        }

        void HandleAnswer(bool correct)
        {
            if (!Running || !correct) return; // only correct matches resolve a round
            Completed++;
            if (hud != null) hud.SetProgress(Completed);
            StartCoroutine(NextRoundAfterDelay());
        }

        IEnumerator NextRoundAfterDelay()
        {
            yield return new WaitForSeconds(nextRoundDelay);
            if (Completed < totalRounds) StartRound();
            else EndGame();
        }

        void EndGame()
        {
            Running = false;
            SetState(false, false, true, true);
            ShapeSfx.Play(fanfareClip);
            OnGameComplete?.Invoke(Completed);
        }

        // ---- shape selection ----------------------------------------------
        int Range(int maxExclusive) => _rng != null ? _rng.Next(maxExclusive) : Random.Range(0, maxExclusive);

        ShapeKind PickKind() => (ShapeKind)Range(ShapeKindExt.Count);

        ShapeKind PickOther(ShapeKind a)
        {
            ShapeKind k;
            do { k = (ShapeKind)Range(ShapeKindExt.Count); } while (k == a);
            return k;
        }

        ShapeKind PickOther(ShapeKind a, ShapeKind b)
        {
            ShapeKind k;
            do { k = (ShapeKind)Range(ShapeKindExt.Count); } while (k == a || k == b);
            return k;
        }
    }
}
