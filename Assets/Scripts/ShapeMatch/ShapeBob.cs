using UnityEngine;

namespace ShapeMatch
{
    /// <summary>
    /// Gentle idle bob for a UI element (target shape, mascot, title). Sine-eases the
    /// anchoredPosition around its starting point. Self-contained — no shared FX code.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ShapeBob : MonoBehaviour
    {
        public float amplitude = 8f;
        public float speed = 1.7f;
        [Tooltip("Optional gentle scale breathing on top of the bob.")]
        public float scalePulse = 0f;

        RectTransform _rt;
        Vector2 _basePos;
        Vector3 _baseScale;
        float _phase;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _basePos = _rt.anchoredPosition;
            _baseScale = _rt.localScale;
            _phase = Random.value * 6.2831853f;
        }

        void OnEnable()
        {
            if (_rt != null) { _rt.anchoredPosition = _basePos; _rt.localScale = _baseScale; }
        }

        void Update()
        {
            float s = Mathf.Sin(Time.time * speed + _phase);
            _rt.anchoredPosition = _basePos + new Vector2(0f, s * amplitude);
            if (scalePulse > 0f)
                _rt.localScale = _baseScale * (1f + scalePulse * 0.5f * (s + 1f) * 0.5f);
        }
    }
}
