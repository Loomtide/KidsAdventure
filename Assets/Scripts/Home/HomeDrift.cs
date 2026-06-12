using UnityEngine;

namespace KidsAdventure
{
    /// <summary>
    /// Slowly drifts a UI element horizontally and wraps it around — background clouds.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class HomeDrift : MonoBehaviour
    {
        public float speed = 12f;   // px/sec (sign = direction)
        public float range = 760f;  // wrap span around the start position

        RectTransform _rt;
        float _startX;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _startX = _rt.anchoredPosition.x;
        }

        void Update()
        {
            var p = _rt.anchoredPosition;
            p.x += speed * Time.deltaTime;
            float half = range * 0.5f;
            if (p.x > _startX + half) p.x = _startX - half;
            else if (p.x < _startX - half) p.x = _startX + half;
            _rt.anchoredPosition = p;
        }
    }
}
