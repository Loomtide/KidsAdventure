using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KidsChef
{
    /// <summary>
    /// A draggable cooking item (ingredient, spray bottle, batter bowl). Follows the pointer
    /// while dragged; on release over its target rect it asks the manager to consume it
    /// (shrink-out on accept), otherwise it springs back home. ForceConsume lets the
    /// manager's debug hook complete the step without simulated input.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class ChefIngredient : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ChefGameManager manager;
        public string id;
        public RectTransform target;
        public bool consumed;

        RectTransform _rt;
        CanvasGroup _cg;
        Canvas _canvas;
        Vector2 _home;
        Vector3 _baseScale;

        public RectTransform Rect => _rt != null ? _rt : (RectTransform)transform;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _cg = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
            _home = _rt.anchoredPosition;
            _baseScale = _rt.localScale;
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (consumed) return;
            _cg.blocksRaycasts = false;
            _rt.localScale = _baseScale * 1.1f;
            _rt.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData e)
        {
            if (consumed) return;
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            _rt.anchoredPosition += e.delta / Mathf.Max(scale, 0.0001f);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (consumed) return;
            _cg.blocksRaycasts = true;
            _rt.localScale = _baseScale;
            bool overTarget = target != null && RectTransformUtility.RectangleContainsScreenPoint(
                target, e.position, _canvas != null ? _canvas.worldCamera : null);
            if (overTarget && manager != null && manager.TryConsume(this))
            {
                consumed = true;
                StartCoroutine(ShrinkOut());
            }
            else
            {
                StartCoroutine(SpringHome());
            }
        }

        /// <summary>Debug hook: jump onto the target and consume (no pointer involved).</summary>
        public void ForceConsume()
        {
            if (consumed || manager == null) return;
            if (!manager.TryConsume(this)) return;
            consumed = true;
            if (target != null) _rt.anchoredPosition = target.anchoredPosition;
            StartCoroutine(ShrinkOut());
        }

        IEnumerator ShrinkOut()
        {
            float t = 0f, dur = 0.22f;
            Vector3 s0 = _rt.localScale;
            while (t < dur)
            {
                t += Time.deltaTime;
                _rt.localScale = Vector3.Lerp(s0, Vector3.zero, t / dur);
                yield return null;
            }
            gameObject.SetActive(false);
        }

        IEnumerator SpringHome()
        {
            Vector2 from = _rt.anchoredPosition;
            float t = 0f, dur = 0.25f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float x = Mathf.Clamp01(t / dur);
                x = 1f - (1f - x) * (1f - x); // ease-out
                _rt.anchoredPosition = Vector2.Lerp(from, _home, x);
                yield return null;
            }
            _rt.anchoredPosition = _home;
        }
    }
}
