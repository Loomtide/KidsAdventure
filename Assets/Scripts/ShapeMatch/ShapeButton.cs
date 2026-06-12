using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ShapeMatch
{
    /// <summary>
    /// One big answer tile showing a white shape on a coloured button. Pops in on a new
    /// question, idle-bounces, and on tap reports to the panel. Correct → squish + green
    /// check. Wrong → shake + red ✕ that fades, and the tile STAYS tappable so the child
    /// can try again (gentle, no-fail design for ages 3-5). Real taps arrive via
    /// IPointerClickHandler; Press() also lets the editor verify without simulated input.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ShapeButton : MonoBehaviour, IPointerClickHandler
    {
        [Header("Refs")]
        public Image shapeImage;     // the white shape drawn on the tile face
        public Image checkBadge;     // green check (correct)
        public Image xBadge;         // red cross (wrong)

        [Header("Idle bob")]
        public float bobAmp = 5f;
        public float bobSpeed = 2.2f;

        public ShapeKind Kind { get; private set; }

        ShapeAnswerPanel _panel;
        RectTransform _rt;
        Vector2 _basePos;
        Vector3 _baseScale;
        float _phase;
        bool _interactable = true;
        Coroutine _fx, _popIn, _xFade;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _basePos = _rt.anchoredPosition;
            _baseScale = _rt.localScale;
            _phase = Random.value * 6.2831853f;
            if (checkBadge) checkBadge.gameObject.SetActive(false);
            if (xBadge) xBadge.gameObject.SetActive(false);
        }

        public void Init(ShapeAnswerPanel panel) => _panel = panel;

        /// <summary>Assign which shape this tile represents (and its white sprite).</summary>
        public void SetShape(ShapeKind kind, Sprite white)
        {
            Kind = kind;
            if (shapeImage != null) shapeImage.sprite = white;
        }

        public void ResetState()
        {
            _interactable = true;
            if (_fx != null) { StopCoroutine(_fx); _fx = null; }
            if (_xFade != null) { StopCoroutine(_xFade); _xFade = null; }
            _rt.anchoredPosition = _basePos;
            if (checkBadge) checkBadge.gameObject.SetActive(false);
            if (xBadge) xBadge.gameObject.SetActive(false);
            if (_popIn != null) StopCoroutine(_popIn);
            _popIn = StartCoroutine(PopIn());
        }

        public void Lock() => _interactable = false;

        public void OnPointerClick(PointerEventData e) => Press();

        public void Press()
        {
            if (!_interactable || _panel == null) return;
            _panel.Submit(this);
        }

        void Update()
        {
            if (_fx == null && _popIn == null)
                _rt.anchoredPosition = _basePos + new Vector2(0f, Mathf.Sin(Time.time * bobSpeed + _phase) * bobAmp);
        }

        IEnumerator PopIn()
        {
            float t = 0f, dur = 0.28f;
            while (t < dur)
            {
                t += Time.deltaTime;
                _rt.localScale = _baseScale * (0.7f + 0.3f * EaseOutBack(t / dur));
                yield return null;
            }
            _rt.localScale = _baseScale;
            _popIn = null;
        }

        public void PlayCorrect()
        {
            _interactable = false;
            if (_fx != null) StopCoroutine(_fx);
            _fx = StartCoroutine(CorrectCo());
        }

        public void PlayWrong()
        {
            if (_fx != null) StopCoroutine(_fx);
            _fx = StartCoroutine(WrongCo());
        }

        IEnumerator CorrectCo()
        {
            if (checkBadge) { checkBadge.gameObject.SetActive(true); checkBadge.transform.localScale = Vector3.zero; }
            float t = 0f, dur = 0.34f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = t / dur;
                float s = 1f + 0.18f * Mathf.Sin(k * Mathf.PI);
                _rt.localScale = new Vector3(_baseScale.x * s, _baseScale.y * (2f - s), _baseScale.z);
                if (checkBadge) checkBadge.transform.localScale = Vector3.one * EaseOutBack(Mathf.Clamp01(k * 1.4f));
                yield return null;
            }
            _rt.localScale = _baseScale;
            _fx = null;
        }

        IEnumerator WrongCo()
        {
            // pop the red ✕ in, then fade it out so the tile is clearly tappable again
            if (xBadge)
            {
                if (_xFade != null) StopCoroutine(_xFade);
                xBadge.gameObject.SetActive(true);
                xBadge.transform.localScale = Vector3.zero;
                var c = xBadge.color; c.a = 1f; xBadge.color = c;
                _xFade = StartCoroutine(FadeBadge());
            }
            float t = 0f, dur = 0.36f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = t / dur;
                _rt.anchoredPosition = _basePos + new Vector2(Mathf.Sin(k * Mathf.PI * 6f) * 14f * (1f - k), 0f);
                if (xBadge && k < 0.5f) xBadge.transform.localScale = Vector3.one * EaseOutBack(Mathf.Clamp01(k * 2.4f));
                yield return null;
            }
            _rt.anchoredPosition = _basePos;
            _fx = null;
        }

        IEnumerator FadeBadge()
        {
            yield return new WaitForSeconds(0.55f);
            float t = 0f, dur = 0.3f;
            var c = xBadge.color;
            while (t < dur)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / dur);
                xBadge.color = c;
                yield return null;
            }
            xBadge.gameObject.SetActive(false);
            c.a = 1f; xBadge.color = c;
            _xFade = null;
        }

        static float EaseOutBack(float x)
        {
            const float c1 = 1.7f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
    }
}
