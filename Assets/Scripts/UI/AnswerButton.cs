using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// One big number button. Pops in on a new question, idle-bounces, squishes + reveals a check
/// on correct, shakes on wrong. Real clicks arrive via IPointerClickHandler (EventSystem);
/// Press() also lets the editor verify it without simulated input.
/// </summary>
[RequireComponent(typeof(Image))]
public class AnswerButton : MonoBehaviour, IPointerClickHandler
{
    public int value;
    public Text label;
    public Image checkBadge;
    public float bobAmp = 4f;
    public float bobSpeed = 2.4f;

    AnswerPanel _panel;
    RectTransform _rt;
    Vector2 _basePos;
    Vector3 _baseScale;
    float _phase;
    bool _interactable = true;
    Coroutine _fx;
    Coroutine _popIn;

    void Awake()
    {
        _rt = (RectTransform)transform;
        _basePos = _rt.anchoredPosition;
        _baseScale = _rt.localScale;
        _phase = Random.value * 6.28f;
        if (checkBadge) checkBadge.gameObject.SetActive(false);
    }

    public void Init(AnswerPanel panel, int v)
    {
        _panel = panel;
        value = v;
        if (label) label.text = v.ToString();
    }

    public void ResetState()
    {
        _interactable = true;
        if (_fx != null) { StopCoroutine(_fx); _fx = null; }
        _rt.anchoredPosition = _basePos;
        if (checkBadge) checkBadge.gameObject.SetActive(false);
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
        float t = 0f, dur = 0.36f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            _rt.anchoredPosition = _basePos + new Vector2(Mathf.Sin(k * Mathf.PI * 6f) * 14f * (1f - k), 0f);
            yield return null;
        }
        _rt.anchoredPosition = _basePos;
        _fx = null;
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.7f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
