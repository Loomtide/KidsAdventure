using UnityEngine;
using UnityEngine.UI;

namespace KidsChef
{
    /// <summary>
    /// The pulsing hint arrow. PointBetween places it part-way from a source item to its
    /// target (rotated to face the target) and it nudges back and forth along that
    /// direction until hidden. The art points right at rotation 0.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ChefArrowHint : MonoBehaviour
    {
        public float nudge = 14f;
        public float speed = 4f;

        RectTransform _rt;
        Image _img;
        Vector2 _anchor;
        Vector2 _dir = Vector2.right;
        bool _shown;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _img = GetComponent<Image>();
            _img.raycastTarget = false;
            _img.enabled = false;
        }

        void Update()
        {
            if (!_shown) return;
            float k = Mathf.Sin(Time.time * speed) * nudge;
            _rt.anchoredPosition = _anchor + _dir * k;
        }

        /// <summary>Show the arrow between two canvas-center-space points, facing the target.</summary>
        public void PointBetween(Vector2 from, Vector2 to, float lerp)
        {
            _anchor = Vector2.Lerp(from, to, lerp);
            _dir = (to - from).sqrMagnitude > 0.01f ? (to - from).normalized : Vector2.right;
            _rt.anchoredPosition = _anchor;
            _rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg);
            _rt.SetAsLastSibling();
            _img.enabled = true;
            _shown = true;
        }

        public void Hide()
        {
            _shown = false;
            if (_img != null) _img.enabled = false;
        }
    }
}
