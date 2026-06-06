using UnityEngine;

/// <summary>Scales an object from zero to its current scale with a springy overshoot, then rests.</summary>
public class PopIn : MonoBehaviour
{
    public float duration = 0.3f;
    Vector3 _target;
    float _t;

    void Awake()
    {
        _target = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        _t += Time.deltaTime;
        float k = Mathf.Clamp01(_t / duration);
        transform.localScale = _target * EaseOutBack(k);
        if (k >= 1f) enabled = false;
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.9f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
