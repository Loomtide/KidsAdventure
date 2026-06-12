using UnityEngine;
using UnityEngine.UI;

namespace KidsChef
{
    /// <summary>
    /// The chef-bear portrait. Idles with a gentle bob, swaps to happy/love expressions
    /// for a while when the player does something good, then settles back to neutral.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ChefMascot : MonoBehaviour
    {
        public Sprite neutral;
        public Sprite happy;
        public Sprite love;
        public float bobAmplitude = 5f;
        public float bobSpeed = 1.4f;

        Image _img;
        RectTransform _rt;
        Vector2 _base;
        float _revertAt = -1f;

        void Awake()
        {
            _img = GetComponent<Image>();
            _rt = (RectTransform)transform;
            _base = _rt.anchoredPosition;
        }

        void Update()
        {
            _rt.anchoredPosition = _base + new Vector2(0f, Mathf.Sin(Time.time * bobSpeed) * bobAmplitude);
            if (_revertAt > 0f && Time.time >= _revertAt)
            {
                _revertAt = -1f;
                _img.sprite = neutral;
            }
        }

        public void Happy(float hold) => Show(happy, hold);
        public void Love(float hold) => Show(love, hold);

        void Show(Sprite s, float hold)
        {
            if (s != null) _img.sprite = s;
            _revertAt = Time.time + hold;
        }
    }
}
