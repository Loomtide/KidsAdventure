using UnityEngine;
using UnityEngine.EventSystems;

namespace KidsChef
{
    /// <summary>
    /// Invisible raycast area over the bowl. While active, any scribbling/circular drag
    /// inside it counts as stirring: the whisk chases the pointer and the accumulated
    /// pointer travel is reported to the manager, which drives the batter cross-fade.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class ChefMixZone : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ChefGameManager manager;
        public RectTransform whisk;
        public float maxRadius = 150f;
        public bool active;

        RectTransform _rt;
        Canvas _canvas;
        bool _stirring;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData e) { _stirring = active; }

        public void OnDrag(PointerEventData e)
        {
            if (!active || !_stirring) return;
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            float delta = e.delta.magnitude / Mathf.Max(scale, 0.0001f);
            if (whisk != null)
            {
                Vector2 local;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rt, e.position, _canvas != null ? _canvas.worldCamera : null, out local);
                local = Vector2.ClampMagnitude(local, maxRadius);
                whisk.anchoredPosition = _rt.anchoredPosition + local + new Vector2(0f, 60f);
                whisk.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 14f) * 9f);
            }
            manager.OnStir(delta);
        }

        public void OnEndDrag(PointerEventData e) { _stirring = false; }
    }
}
