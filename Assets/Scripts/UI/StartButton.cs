using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Big Play button on the Start screen: pops in, gently pulses, starts the game on click.</summary>
[RequireComponent(typeof(UnityEngine.UI.Image))]
public class StartButton : MonoBehaviour, IPointerClickHandler
{
    public StartController controller;

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
        // gentle idle pulse
        float s = 1f + 0.04f * Mathf.Sin(Time.time * 3f);
        if (_rt != null) _rt.localScale = _baseScale * s;
    }

    public void OnPointerClick(PointerEventData e) => Click();

    public void Click()
    {
        if (controller != null) controller.StartGame();
    }

    IEnumerator PopIn()
    {
        float t = 0f, dur = 0.34f;
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
