using UnityEngine;
using UnityEngine.UI;

namespace KidsAdventure
{
    /// <summary>
    /// Twinkles a sparkle: alpha and scale pulse on independent random phases, with a
    /// slow lazy spin. Scatter a few over the home screen for ambient life.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class HomeSparkle : MonoBehaviour
    {
        public float twinkleSpeed = 2.2f;
        public float minAlpha = 0.15f;
        public float spinSpeed = 18f;

        Image _img;
        RectTransform _rt;
        Vector3 _baseScale;
        float _phase;

        void Awake()
        {
            _img = GetComponent<Image>();
            _rt = (RectTransform)transform;
            _baseScale = _rt.localScale;
            _phase = Random.value * 6.2831853f;
        }

        void Update()
        {
            float s = (Mathf.Sin(Time.time * twinkleSpeed + _phase) + 1f) * 0.5f; // 0..1
            var c = _img.color;
            c.a = Mathf.Lerp(minAlpha, 1f, s);
            _img.color = c;
            _rt.localScale = _baseScale * (0.7f + 0.3f * s);
            _rt.localRotation = Quaternion.Euler(0, 0, Time.time * spinSpeed + _phase * 40f);
        }
    }
}
