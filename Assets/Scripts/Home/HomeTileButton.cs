using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace KidsAdventure
{
    /// <summary>
    /// A mini-game tile on the Kids Adventure home screen. Tapping an unlocked tile plays a
    /// happy bounce + jingle, then loads that game's scene. Locked ("Coming Soon") tiles
    /// wobble and buzz instead. Real taps arrive via IPointerClickHandler; debugClick lets
    /// the editor bridge verify it without simulated input.
    /// </summary>
    public class HomeTileButton : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Scene to load; empty or locked = Coming Soon tile.")]
        public string sceneName;
        public bool locked;
        public AudioSource sfx;
        public AudioClip tapClip;
        public AudioClip lockedClip;

        [Header("Debug (editor verification)")]
        public bool debugClick;

        RectTransform _rt;
        Vector3 _baseScale;
        bool _busy;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _baseScale = _rt.localScale;
        }

        void Update()
        {
            if (debugClick) { debugClick = false; Click(); }
        }

        public void OnPointerClick(PointerEventData e) => Click();

        public void Click()
        {
            if (_busy) return;
            if (locked || string.IsNullOrEmpty(sceneName))
            {
                if (sfx != null && lockedClip != null) sfx.PlayOneShot(lockedClip, 0.5f);
                StartCoroutine(Wobble());
            }
            else
            {
                if (sfx != null && tapClip != null) sfx.PlayOneShot(tapClip, 0.9f);
                StartCoroutine(BounceAndLoad());
            }
        }

        IEnumerator BounceAndLoad()
        {
            _busy = true;
            float t = 0f, dur = 0.34f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = t / dur;
                // quick squash then overshoot pop
                float s = k < 0.35f ? Mathf.Lerp(1f, 0.86f, k / 0.35f)
                                    : Mathf.Lerp(0.86f, 1.12f, EaseOutBack((k - 0.35f) / 0.65f));
                _rt.localScale = _baseScale * s;
                yield return null;
            }
            SceneManager.LoadScene(sceneName);
        }

        IEnumerator Wobble()
        {
            _busy = true;
            float t = 0f, dur = 0.45f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float fade = 1f - t / dur;
                _rt.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 42f) * 7f * fade);
                yield return null;
            }
            _rt.localRotation = Quaternion.identity;
            _busy = false;
        }

        static float EaseOutBack(float x)
        {
            const float c1 = 1.7f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
    }
}
