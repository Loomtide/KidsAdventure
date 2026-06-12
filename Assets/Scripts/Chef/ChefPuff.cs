using UnityEngine;
using UnityEngine.UI;

namespace KidsChef
{
    /// <summary>
    /// A one-shot rising, growing, fading puff (steam, spray mist, sparkles).
    /// Destroys itself when its life is over.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ChefPuff : MonoBehaviour
    {
        public float rise = 90f;
        public float grow = 0.6f;
        public float life = 1.4f;
        public float drift;

        Image _img;
        RectTransform _rt;
        Vector2 _start;
        float _t0;

        void Start()
        {
            _img = GetComponent<Image>();
            _rt = (RectTransform)transform;
            _start = _rt.anchoredPosition;
            _t0 = Time.time;
            if (drift == 0f) drift = Random.Range(-22f, 22f);
        }

        void Update()
        {
            float t = (Time.time - _t0) / life;
            if (t >= 1f) { Destroy(gameObject); return; }
            _rt.anchoredPosition = _start + new Vector2(drift * t, rise * t);
            _rt.localScale = Vector3.one * (1f + grow * t);
            var c = _img.color; c.a = 1f - t * t; _img.color = c;
        }
    }
}
