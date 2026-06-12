using UnityEngine;

namespace KidsAdventure
{
    /// <summary>
    /// Floats a balloon slowly upward with a gentle horizontal sway; wraps back below
    /// the bottom of the screen once it leaves the top, so balloons drift by forever.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class HomeBalloon : MonoBehaviour
    {
        public float riseSpeed = 26f;     // px/sec
        public float swayAmplitude = 22f;
        public float swaySpeed = 0.9f;
        public float topY = 520f;         // wrap when above this (anchored Y)
        public float bottomY = -560f;     // respawn here

        RectTransform _rt;
        float _baseX;
        float _phase;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _baseX = _rt.anchoredPosition.x;
            _phase = Random.value * 6.2831853f;
        }

        void Update()
        {
            var p = _rt.anchoredPosition;
            p.y += riseSpeed * Time.deltaTime;
            p.x = _baseX + Mathf.Sin(Time.time * swaySpeed + _phase) * swayAmplitude;
            if (p.y > topY) p.y = bottomY;
            _rt.anchoredPosition = p;
            _rt.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * swaySpeed + _phase) * 6f);
        }
    }
}
