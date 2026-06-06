using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The "Great job!" end screen. Lives on an always-active object, subscribes to
/// GameManager.OnGameComplete, and shows a results card (stars earned + replay). The replay
/// button calls Replay() → hides the card and restarts the game.
/// </summary>
public class EndSummaryController : MonoBehaviour
{
    public GameManager game;
    public GameObject root;     // the panel container (starts inactive)
    public RectTransform card;  // pops in
    public Text titleText;
    public Text scoreText;
    public Image[] stars;
    public Color starFilled = new Color(1f, 0.808f, 0.31f);
    public Color starEmpty = new Color(0.905f, 0.878f, 0.937f);

    void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    void Start()
    {
        if (game != null) game.OnGameComplete += Show;
    }

    void OnDestroy()
    {
        if (game != null) game.OnGameComplete -= Show;
    }

    public void Show(int starsEarned, int total)
    {
        if (root != null) root.SetActive(true);
        if (titleText != null) titleText.text = starsEarned >= total ? "Great job!" : "Well done!";
        if (scoreText != null) scoreText.text = $"{starsEarned} / {total}";
        if (stars != null)
            for (int i = 0; i < stars.Length; i++)
                if (stars[i] != null) stars[i].color = i < starsEarned ? starFilled : starEmpty;
        if (card != null) StartCoroutine(PopCard());
    }

    IEnumerator PopCard()
    {
        card.localScale = Vector3.zero;
        float t = 0f, dur = 0.42f;
        while (t < dur)
        {
            t += Time.deltaTime;
            card.localScale = Vector3.one * EaseOutBack(t / dur);
            yield return null;
        }
        card.localScale = Vector3.one;
    }

    public void Replay()
    {
        if (root != null) root.SetActive(false);
        if (game != null) game.Restart();
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.7f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
