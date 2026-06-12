using UnityEngine;

namespace ShapeMatch
{
    /// <summary>
    /// Owns the three answer tiles. The round manager hands it the correct shape plus two
    /// distractors; this lays them out in random slots and shows each tile's white shape.
    /// A correct tap locks the row, fires check + confetti + chime, and resolves the round
    /// (OnAnswered(true)). A wrong tap shakes + red ✕ + buzz but keeps the row tappable so
    /// the child can try again — the round only advances on a correct match.
    /// </summary>
    public class ShapeAnswerPanel : MonoBehaviour
    {
        [Header("Tiles")]
        public ShapeButton[] buttons;          // exactly 3
        [Tooltip("White tile sprite per ShapeKind, indexed Circle,Square,Triangle,Star.")]
        public Sprite[] whiteSprites = new Sprite[4];

        [Header("Celebration")]
        public RectTransform confettiLayer;
        public Sprite confettiSprite;

        [Header("Audio")]
        public AudioClip chimeClip;
        public AudioClip buzzClip;

        [Header("Debug (editor verification)")]
        public bool debugSubmitCorrect;
        public bool debugSubmitWrong;

        public ShapeKind CorrectKind { get; private set; }
        public bool Answered { get; private set; }
        public System.Action<bool> OnAnswered; // (wasCorrect) — only ever fires with true here

        void Awake()
        {
            if (buttons != null)
                foreach (var b in buttons) if (b != null) b.Init(this);
        }

        void Update()
        {
            if (debugSubmitCorrect && buttons != null)
            {
                debugSubmitCorrect = false;
                foreach (var b in buttons) if (b != null && b.Kind == CorrectKind) { Submit(b); break; }
            }
            if (debugSubmitWrong && buttons != null)
            {
                debugSubmitWrong = false;
                foreach (var b in buttons) if (b != null && b.Kind != CorrectKind) { Submit(b); break; }
            }
        }

        /// <summary>Pose a round: correct shape + the two distractors, shuffled across tiles.</summary>
        public void SetQuestion(ShapeKind correct, ShapeKind a, ShapeKind b)
        {
            CorrectKind = correct;
            Answered = false;

            var opts = new ShapeKind[] { correct, a, b };
            for (int i = opts.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (opts[i], opts[j]) = (opts[j], opts[i]);
            }
            for (int i = 0; i < buttons.Length && i < opts.Length; i++)
            {
                if (buttons[i] == null) continue;
                buttons[i].SetShape(opts[i], SpriteFor(opts[i]));
                buttons[i].ResetState();
            }
        }

        Sprite SpriteFor(ShapeKind k)
        {
            int i = (int)k;
            return (whiteSprites != null && i >= 0 && i < whiteSprites.Length) ? whiteSprites[i] : null;
        }

        public void Submit(ShapeButton b)
        {
            if (Answered || b == null) return;
            if (b.Kind == CorrectKind)
            {
                Answered = true;
                foreach (var bb in buttons) if (bb != null) bb.Lock();
                b.PlayCorrect();
                ShapeConfetti.Burst(confettiLayer, confettiSprite);
                ShapeSfx.Play(chimeClip);
                OnAnswered?.Invoke(true);
            }
            else
            {
                b.PlayWrong();
                ShapeSfx.Play(buzzClip);
                // stay in the round — no OnAnswered, tiles remain tappable
            }
        }
    }
}
