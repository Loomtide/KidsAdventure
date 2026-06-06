using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Home/back button shown during gameplay and the reward screen. On click it abandons the
/// run and returns to the Start screen via StartController.GoHome().
/// </summary>
[RequireComponent(typeof(UnityEngine.UI.Image))]
public class HomeButton : MonoBehaviour, IPointerClickHandler
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
    }

    public void OnPointerClick(PointerEventData e) => Click();

    public void Click()
    {
        if (controller != null) controller.GoHome();
    }

    IEnumerator PopIn()
    {
        float t = 0f, dur = 0.26f;
        while (t < dur)
        {
            t += Time.deltaTime;
            _rt.localScale = _baseScale * (0.6f + 0.4f * EaseOutBack(t / dur));
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
