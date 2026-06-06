using UnityEngine;

/// <summary>Gentle idle bob + breathing squash for a UI element (e.g. the Start-screen mascot).</summary>
public class UIBob : MonoBehaviour
{
    public float amplitude = 8f;
    public float speed = 2f;
    public float squash = 0.04f;

    RectTransform _rt;
    Vector2 _basePos;
    Vector3 _baseScale;

    void Awake()
    {
        _rt = (RectTransform)transform;
        _basePos = _rt.anchoredPosition;
        _baseScale = _rt.localScale;
    }

    void OnEnable()
    {
        if (_rt != null) { _basePos = _rt.anchoredPosition; _baseScale = _rt.localScale; }
    }

    void Update()
    {
        float t = Time.time * speed;
        _rt.anchoredPosition = _basePos + new Vector2(0f, Mathf.Sin(t) * amplitude);
        float w = 1f + Mathf.Sin(t) * squash;
        _rt.localScale = new Vector3(_baseScale.x * w, _baseScale.y * (2f - w), _baseScale.z);
    }
}
