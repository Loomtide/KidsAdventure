using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KidsChef
{
    /// <summary>
    /// A chunky tap button (power / done / play again / home). Pops in on enable, optionally
    /// pulses, and invokes the matching manager action on tap. debugClick lets the editor
    /// verify it without simulated input.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class ChefButton : MonoBehaviour, IPointerClickHandler
    {
        public enum Action { Power, Done, PlayAgain, Home }

        public ChefGameManager manager;
        public Action action = Action.Power;
        public float pulse;

        [Header("Debug (editor verification)")]
        public bool debugClick;

        RectTransform _rt;
        Vector3 _baseScale;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _baseScale = _rt.localScale;
        }

        void OnEnable()
        {
            if (_rt != null) StartCoroutine(PopIn());
        }

        void Update()
        {
            if (debugClick) { debugClick = false; Click(); }
            if (_rt != null && pulse > 0f)
                _rt.localScale = _baseScale * (1f + pulse * Mathf.Sin(Time.time * 4f));
            else if (_rt != null)
                _rt.localScale = _baseScale;
        }

        public void OnPointerClick(PointerEventData e) => Click();

        public void Click()
        {
            if (manager == null) return;
            switch (action)
            {
                case Action.Power: manager.PowerPressed(); break;
                case Action.Done: manager.DonePressed(); break;
                case Action.PlayAgain: manager.PlayAgain(); break;
                case Action.Home: manager.GoHome(); break;
            }
        }

        IEnumerator PopIn()
        {
            float t = 0f, dur = 0.3f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float x = Mathf.Clamp01(t / dur);
                const float c1 = 1.7f, c3 = c1 + 1f;
                float e = 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
                _rt.localScale = _baseScale * Mathf.Max(0.05f, e);
                yield return null;
            }
            _rt.localScale = _baseScale;
        }
    }
}
