using UnityEngine;
using UnityEngine.UI;

/// <summary>A single UI confetti rectangle: arcs under gravity, spins, fades, self-destructs.</summary>
[RequireComponent(typeof(Image))]
public class ConfettiPiece : MonoBehaviour
{
    public Vector2 velocity;
    public float gravity = 620f;
    public float spin = 320f;
    public float life = 1.8f;

    RectTransform _rt;
    Image _img;
    float _t;
    Color _base;

    void Awake()
    {
        _rt = (RectTransform)transform;
        _img = GetComponent<Image>();
        _base = _img.color;
        spin *= Random.Range(-1f, 1f);
    }

    void Update()
    {
        _t += Time.deltaTime;
        if (_t >= life) { Destroy(gameObject); return; }
        velocity.y -= gravity * Time.deltaTime;
        _rt.anchoredPosition += velocity * Time.deltaTime;
        _rt.Rotate(0f, 0f, spin * Time.deltaTime);
        var c = _base;
        c.a = _t < life * 0.65f ? 1f : Mathf.Lerp(1f, 0f, (_t - life * 0.65f) / (life * 0.35f));
        _img.color = c;
    }
}
