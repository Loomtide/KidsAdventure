using System.Collections;
using UnityEngine;

/// <summary>
/// A single tappable fruit. Pops in with a staggered spawn animation; on tap it does a
/// squash-stretch pop, throws a gold sparkle burst and a "+1" float, stamps a green "counted"
/// check, plays a pop SFX, and adds one to the live count. Tapping again does nothing.
/// </summary>
[DisallowMultipleComponent]
public class Fruit : MonoBehaviour
{
    [Header("Refs")]
    public Transform visual;

    [Header("FX sprites")]
    public Sprite sparkleSprite;
    public Sprite plusOneSprite;
    public Sprite checkSprite;

    [Header("SFX")]
    public AudioClip popClip;

    [Header("Tuning")]
    public int sparkleCount = 7;
    public float popDuration = 0.34f;

    public bool Counted { get; private set; }
    Vector3 _baseScale = Vector3.one;
    bool _spawning;

    void Awake()
    {
        if (visual == null) visual = transform.Find("Visual");
        if (visual != null) _baseScale = visual.localScale;
    }

    void Start()
    {
        if (visual != null) StartCoroutine(SpawnPop());
    }

    IEnumerator SpawnPop()
    {
        _spawning = true;
        visual.localScale = Vector3.zero;
        yield return new WaitForSeconds(Random.Range(0f, 0.22f));
        float t = 0f, dur = 0.32f;
        while (t < dur)
        {
            t += Time.deltaTime;
            visual.localScale = _baseScale * EaseOutBack(t / dur);
            yield return null;
        }
        visual.localScale = _baseScale;
        _spawning = false;
    }

    public bool MarkCounted()
    {
        if (Counted) return false;
        Counted = true;
        return true;
    }

    /// <summary>Tap this fruit. Returns true if it counted (first tap only).</summary>
    public bool Tap()
    {
        if (!MarkCounted()) return false;

        if (visual != null) { StopAllCoroutines(); _spawning = false; StartCoroutine(PopRoutine()); }
        SpawnSparkles();
        SpawnPlusOne();
        StampCheck();
        SfxPlayer.Play(popClip, Random.Range(0.94f, 1.08f));
        if (CountManager.Instance != null) CountManager.Instance.Add(1);
        return true;
    }

    IEnumerator PopRoutine()
    {
        float up = popDuration * 0.30f;
        float down = popDuration - up;
        float t = 0f;
        while (t < up)
        {
            t += Time.deltaTime;
            float k = EaseOut(t / up);
            visual.localScale = Vector3.LerpUnclamped(_baseScale,
                Vector3.Scale(_baseScale, new Vector3(1.26f, 1.16f, 1f)), k);
            yield return null;
        }
        t = 0f;
        Vector3 from = Vector3.Scale(_baseScale, new Vector3(1.26f, 1.16f, 1f));
        while (t < down)
        {
            t += Time.deltaTime;
            float k = EaseOutBack(t / down);
            visual.localScale = Vector3.LerpUnclamped(from, _baseScale, k);
            yield return null;
        }
        visual.localScale = _baseScale;
    }

    void SpawnSparkles()
    {
        if (sparkleSprite == null) return;
        for (int i = 0; i < sparkleCount; i++)
        {
            float ang = (360f / sparkleCount) * i + Random.Range(-12f, 12f);
            var rad = ang * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            var go = MakeSpriteGo("Sparkle", sparkleSprite, 25,
                transform.position + (Vector3)(dir * 0.15f));
            go.transform.localScale = Vector3.one * Random.Range(0.7f, 1.05f);
            var fly = go.AddComponent<SparkleFly>();
            fly.velocity = dir * Random.Range(2.2f, 3.6f) + Vector2.up * 0.6f;
            fly.life = Random.Range(0.42f, 0.6f);
        }
    }

    void SpawnPlusOne()
    {
        if (plusOneSprite == null) return;
        var go = MakeSpriteGo("PlusOne", plusOneSprite, 30,
            transform.position + new Vector3(0f, 0.55f, 0f));
        go.transform.localScale = Vector3.one;
        go.AddComponent<FloatFade>();
    }

    void StampCheck()
    {
        if (checkSprite == null) return;
        var go = MakeSpriteGo("CountedCheck", checkSprite, 16,
            transform.position + new Vector3(0.42f, 0.42f, 0f));
        go.transform.SetParent(transform, true);
        go.transform.localScale = Vector3.one * 0.42f;
        go.AddComponent<PopIn>();
    }

    /// <summary>Stamp a big red cross over this fruit — wrong-answer feedback.</summary>
    public void ShowWrong(Sprite cross)
    {
        if (cross == null) return;
        var go = MakeSpriteGo("WrongCross", cross, 20, transform.position);
        go.transform.SetParent(transform, true);
        go.transform.localScale = Vector3.one * 1.5f;
        go.AddComponent<PopIn>();
    }

    GameObject MakeSpriteGo(string name, Sprite sprite, int order, Vector3 worldPos)
    {
        var go = new GameObject(name);
        go.transform.position = worldPos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return go;
    }

    static float EaseOut(float x) => 1f - Mathf.Pow(1f - x, 3f);
    static float EaseOutBack(float x)
    {
        const float c1 = 1.7f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
