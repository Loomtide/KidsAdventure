using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KidsChef
{
    /// <summary>
    /// A topping on the tray. Dragging it spawns a copy that follows the pointer; dropping
    /// the copy on the plate sticks it to the waffles (with a small random tilt), anywhere
    /// else fades it away. The tray item itself never moves, so toppings are unlimited.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ChefToppingSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ChefGameManager manager;
        public RectTransform plateZone;
        public RectTransform decorLayer;
        public Vector2 placedSize = new Vector2(70f, 70f);
        public bool randomTilt = true;

        RectTransform _ghost;
        Canvas _canvas;

        void Awake() { _canvas = GetComponentInParent<Canvas>(); }

        public void OnBeginDrag(PointerEventData e)
        {
            if (manager == null || !manager.CanPlaceTopping) return;
            var src = GetComponent<Image>();
            var go = new GameObject("Topping_" + name, typeof(RectTransform));
            go.transform.SetParent(manager.dragLayer, false);
            var img = go.AddComponent<Image>();
            img.sprite = src.sprite;
            img.raycastTarget = false;
            _ghost = (RectTransform)go.transform;
            _ghost.anchorMin = _ghost.anchorMax = new Vector2(0.5f, 0.5f);
            _ghost.sizeDelta = placedSize;
            MoveGhost(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (_ghost != null) MoveGhost(e);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (_ghost == null) return;
            bool onPlate = plateZone != null && RectTransformUtility.RectangleContainsScreenPoint(
                plateZone, e.position, _canvas != null ? _canvas.worldCamera : null);
            if (onPlate && manager.CanPlaceTopping)
                Stick(_ghost);
            else
                manager.StartCoroutine(FadeAway(_ghost));
            _ghost = null;
        }

        void MoveGhost(PointerEventData e)
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_ghost.parent, e.position,
                _canvas != null ? _canvas.worldCamera : null, out local);
            _ghost.anchoredPosition = local;
        }

        void Stick(RectTransform ghost)
        {
            ghost.SetParent(decorLayer, true);
            ghost.localScale = Vector3.one;
            if (randomTilt) ghost.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-22f, 22f));
            manager.StartCoroutine(ChefGameManager.PopRect(ghost));
            manager.ToppingPlaced();
        }

        /// <summary>Debug hook: place one topping at the given offset from the plate centre.</summary>
        public void DebugPlace(Vector2 offset)
        {
            if (manager == null || !manager.CanPlaceTopping) return;
            var src = GetComponent<Image>();
            var go = new GameObject("Topping_" + name, typeof(RectTransform));
            go.transform.SetParent(decorLayer, false);
            var img = go.AddComponent<Image>();
            img.sprite = src.sprite;
            img.raycastTarget = false;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = placedSize;
            rt.anchoredPosition = plateZone.anchoredPosition + offset;
            if (randomTilt) rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-22f, 22f));
            manager.StartCoroutine(ChefGameManager.PopRect(rt));
            manager.ToppingPlaced();
        }

        static IEnumerator FadeAway(RectTransform ghost)
        {
            var img = ghost.GetComponent<Image>();
            float t = 0f, dur = 0.2f;
            while (t < dur && img != null)
            {
                t += Time.deltaTime;
                var c = img.color; c.a = 1f - t / dur; img.color = c;
                yield return null;
            }
            if (ghost != null) Object.Destroy(ghost.gameObject);
        }
    }
}
