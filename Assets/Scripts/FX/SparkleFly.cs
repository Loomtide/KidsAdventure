using UnityEngine;

/// <summary>One sparkle particle: flies outward, spins, shrinks and fades, then self-destructs.</summary>
public class SparkleFly : MonoBehaviour
{
    public Vector2 velocity = Vector2.up;
    public float gravity = 2.2f;
    public float spin = 220f;
    public float life = 0.5f;

    SpriteRenderer _sr;
    float _t;
    Vector3 _baseScale;
    Color _baseColor;

    void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
        _baseScale = transform.localScale;
        _baseColor = _sr != null ? _sr.color : Color.white;
    }

    void Update()
    {
        _t += Time.deltaTime;
        float k = _t / life;
        if (k >= 1f) { Destroy(gameObject); return; }

        velocity.y -= gravity * Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, spin * Time.deltaTime);

        float s = Mathf.Lerp(1f, 0.1f, k * k);
        transform.localScale = _baseScale * s;
        if (_sr != null)
        {
            var c = _baseColor; c.a = Mathf.Lerp(1f, 0f, k);
            _sr.color = c;
        }
    }
}
