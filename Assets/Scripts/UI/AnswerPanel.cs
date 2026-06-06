using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the three answer buttons: builds a question (correct value + two near distractors),
/// evaluates a tapped value, and fires correct/wrong feedback (check + confetti + chime, or
/// shake + buzz). The round-loop slice calls SetQuestion(n) and listens to OnSolved.
/// </summary>
public class AnswerPanel : MonoBehaviour
{
    public AnswerButton[] buttons;
    public FruitSpawner spawner;
    public RectTransform confettiLayer;
    public Sprite confettiSprite;
    public AudioClip chimeClip;
    public AudioClip buzzClip;
    public bool autoSetupOnStart = true;

    [Header("Debug (editor verification)")]
    public int debugSubmit = -1;
    public bool debugSubmitCorrect;

    public int CorrectValue { get; private set; }
    public bool Solved { get; private set; }
    public System.Action OnSolved;

    void Start()
    {
        if (autoSetupOnStart)
            SetQuestion(spawner != null ? spawner.previewCount : 6);
    }

    void Update()
    {
        if (debugSubmit >= 0 && buttons != null && debugSubmit < buttons.Length)
        {
            int i = debugSubmit;
            debugSubmit = -1;
            Submit(buttons[i]);
        }
        if (debugSubmitCorrect && buttons != null)
        {
            debugSubmitCorrect = false;
            foreach (var b in buttons)
                if (b != null && b.value == CorrectValue) { Submit(b); break; }
        }
    }

    public void SetQuestion(int correct)
    {
        CorrectValue = correct;
        Solved = false;
        int[] opts = BuildOptions(correct);
        for (int i = 0; i < buttons.Length && i < opts.Length; i++)
        {
            buttons[i].Init(this, opts[i]);
            buttons[i].ResetState();
        }
    }

    int[] BuildOptions(int correct)
    {
        var set = new List<int> { correct };
        void TryAdd(int v) { if (v >= 1 && !set.Contains(v)) set.Add(v); }
        TryAdd(correct - 1);
        TryAdd(correct + 1);
        TryAdd(correct + 2);
        TryAdd(correct - 2);
        while (set.Count < 3) TryAdd(correct + set.Count + 1);
        var pick = set.GetRange(0, 3);
        for (int i = pick.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pick[i], pick[j]) = (pick[j], pick[i]);
        }
        return pick.ToArray();
    }

    public void Submit(AnswerButton b)
    {
        if (Solved) return;
        if (b.value == CorrectValue)
        {
            Solved = true;
            b.PlayCorrect();
            foreach (var bb in buttons) bb.Lock();
            BurstConfetti();
            SfxPlayer.Play(chimeClip);
            OnSolved?.Invoke();
        }
        else
        {
            b.PlayWrong();
            SfxPlayer.Play(buzzClip);
        }
    }

    static readonly string[] Palette = { "ff5d72", "ffce4f", "5fe0bd", "b79cff", "ff9aa0", "59c1ff", "ff8a3d" };

    void BurstConfetti()
    {
        if (confettiSprite == null || confettiLayer == null) return;
        for (int i = 0; i < 44; i++)
        {
            var go = new GameObject("Confetti");
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.sprite = confettiSprite;
            img.raycastTarget = false;
            ColorUtility.TryParseHtmlString("#" + Palette[Random.Range(0, Palette.Length)], out var col);
            img.color = col;
            var rt = (RectTransform)go.transform;
            rt.SetParent(confettiLayer, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(Random.Range(26f, 44f), Random.Range(34f, 54f));
            rt.anchoredPosition = new Vector2(Random.Range(-260f, 260f), Random.Range(-30f, 90f));
            var p = go.AddComponent<ConfettiPiece>();
            p.velocity = new Vector2(Random.Range(-380f, 380f), Random.Range(280f, 580f));
            p.gravity = 720f;
        }
    }
}
