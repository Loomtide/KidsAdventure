using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ShapeMatch
{
    /// <summary>
    /// A chunky menu button (Play / Home / Play Again). Pops in, gently pulses, and invokes
    /// the matching ShapeGameManager action on tap. Real taps arrive via IPointerClickHandler;
    /// debugClick lets the editor verify it without simulated input.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class ShapeMenuButton : MonoBehaviour, IPointerClickHandler
    {
        public enum Action { Play, Home, PlayAgain }

        public ShapeGameManager manager;
        public Action action = Action.Play;
        public float pulse = 0.04f;

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
                _rt.localScale = _baseScale * (1f + pulse * Mathf.Sin(Time.time * 3f));
        }

        public void OnPointerClick(PointerEventData e) => Click();

        public void Click()
        {
            if (manager == null) return;
            switch (action)
            {
                case Action.Play: manager.StartGame(); break;
                case Action.Home:
                    // In the Kids Adventure package, Home returns to the hub scene;
                    // standalone it falls back to this game's own Start screen.
                    if (Application.CanStreamedLevelBeLoaded("KidsAdventure"))
                        UnityEngine.SceneManagement.SceneManager.LoadScene("KidsAdventure");
                    else
                        manager.GoHome();
                    break;
                case Action.PlayAgain: manager.Restart(); break;
            }
        }

        IEnumerator PopIn()
        {
            float t = 0f, dur = 0.32f;
            while (t < dur)
            {
                t += Time.deltaTime;
                _rt.localScale = _baseScale * (0.5f + 0.5f * EaseOutBack(t / dur));
                yield return null;
            }
            _rt.localScale = _baseScale;
        }

        static float EaseOutBack(float x)
        {
            const float c1 = 1.7f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
    }
}
