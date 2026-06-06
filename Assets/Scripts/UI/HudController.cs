using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the HUD: applies the rounded display font to every Text, binds the basket count
/// chip to CountManager, animates the mascot (idle bob + breathing squash + a Cheer on a
/// correct answer), and exposes SetRound / SetStars (with a star pop on each new fill).
/// </summary>
public class HudController : MonoBehaviour
{
    [Header("Font")]
    public Font displayFont;

    [Header("Count chip")]
    public Text countText;
    public string countPrefix = "×";

    [Header("Mascot")]
    public RectTransform mascot;
    public float bobAmplitude = 5f;
    public float bobSpeed = 2.2f;
    public float squashAmount = 0.035f;

    [Header("Progress")]
    public Image[] stars;
    public Text roundText;
    public Color starFilled = new Color(1f, 0.808f, 0.31f);
    public Color starEmpty = new Color(0.905f, 0.878f, 0.937f);

    Vector2 _mascotBasePos;
    Vector3 _mascotBaseScale;
    bool _cheering;

    void Awake()
    {
        if (displayFont != null)
            foreach (var t in GetComponentsInChildren<Text>(true)) t.font = displayFont;
        if (mascot != null)
        {
            _mascotBasePos = mascot.anchoredPosition;
            _mascotBaseScale = mascot.localScale;
        }
        SetStars(0);
    }

    void Start()
    {
        if (CountManager.Instance != null)
        {
            CountManager.Instance.OnCountChanged += SetCount;
            SetCount(CountManager.Instance.Count);
        }
    }

    void OnDestroy()
    {
        if (CountManager.Instance != null)
            CountManager.Instance.OnCountChanged -= SetCount;
    }

    void SetCount(int n)
    {
        if (countText != null) countText.text = countPrefix + n;
    }

    public void SetRound(int current, int total)
    {
        if (roundText != null) roundText.text = $"Round {current} / {total}";
    }

    public void SetStars(int filled)
    {
        if (stars == null) return;
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;
            stars[i].color = i < filled ? starFilled : starEmpty;
        }
        int idx = filled - 1;
        if (idx >= 0 && idx < stars.Length && stars[idx] != null)
            StartCoroutine(PopStar(stars[idx].rectTransform));
    }

    IEnumerator PopStar(RectTransform rt)
    {
        float t = 0f, dur = 0.34f;
        while (t < dur)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.one * (1f + 0.6f * Mathf.Sin((t / dur) * Mathf.PI));
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    public void Cheer()
    {
        if (mascot != null) StartCoroutine(CheerCo());
    }

    IEnumerator CheerCo()
    {
        _cheering = true;
        float t = 0f, dur = 0.65f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            float hop = Mathf.Sin(k * Mathf.PI) * 20f;
            float spin = Mathf.Sin(k * Mathf.PI * 3f) * 16f * (1f - k);
            float s = 1f + Mathf.Sin(k * Mathf.PI) * 0.22f;
            mascot.anchoredPosition = _mascotBasePos + new Vector2(0f, hop);
            mascot.localScale = new Vector3(_mascotBaseScale.x * s, _mascotBaseScale.y * s, _mascotBaseScale.z);
            mascot.localRotation = Quaternion.Euler(0f, 0f, spin);
            yield return null;
        }
        mascot.localRotation = Quaternion.identity;
        _cheering = false;
    }

    void Update()
    {
        if (mascot == null || _cheering) return;
        float t = Time.time;
        mascot.anchoredPosition = _mascotBasePos + new Vector2(0f, Mathf.Sin(t * bobSpeed) * bobAmplitude);
        float w = 1f + Mathf.Sin(t * bobSpeed) * squashAmount;
        mascot.localScale = new Vector3(_mascotBaseScale.x * w, _mascotBaseScale.y * (2f - w), _mascotBaseScale.z);
    }
}
