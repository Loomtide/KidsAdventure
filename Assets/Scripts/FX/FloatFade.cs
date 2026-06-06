using UnityEngine;

/// <summary>Floating label (e.g. "+1"): pops in, drifts upward, then fades and self-destructs.</summary>
public class FloatFade : MonoBehaviour
{
    public float riseSpeed = 1.6f;
    public float life = 0.75f;
    public float popScale = 1.25f;

    SpriteRenderer _sr;
    float _t;
    Vector3 _targetScale;
    Color _baseColor;

    void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
        _targetScale = transform.localScale;
        _baseColor = _sr != null ? _sr.color : Color.white;
        transform.localScale = _targetScale * 0.4f;
    }

    void Update()
    {
        _t += Time.deltaTime;
        float k = _t / life;
        if (k >= 1f) { Destroy(gameObject); return; }

        transform.position += Vector3.up * riseSpeed * Time.deltaTime * (1f - k * 0.5f);

        // quick pop-in on the first 25% of life, settle to target
        float in01 = Mathf.Clamp01(_t / (life * 0.25f));
        float s = Mathf.LerpUnclamped(0.4f, 1f, EaseOutBack(in01));
        transform.localScale = _targetScale * s;

        if (_sr != null)
        {
            var c = _baseColor;
            c.a = k < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (k - 0.6f) / 0.4f);
            _sr.color = c;
        }
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
