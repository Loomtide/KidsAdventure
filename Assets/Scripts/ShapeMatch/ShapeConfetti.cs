using UnityEngine;
using UnityEngine.UI;

namespace ShapeMatch
{
    /// <summary>One confetti rectangle: ballistic arc + spin + fade, then self-destruct.</summary>
    public class ShapeConfetti : MonoBehaviour
    {
        public Vector2 velocity;
        public float gravity = 720f;
        public float life = 1.4f;

        RectTransform _rt;
        Image _img;
        float _t;
        float _spin;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _img = GetComponent<Image>();
            _spin = Random.Range(-360f, 360f);
        }

        void Update()
        {
            _t += Time.deltaTime;
            velocity.y -= gravity * Time.deltaTime;
            _rt.anchoredPosition += velocity * Time.deltaTime;
            _rt.Rotate(0f, 0f, _spin * Time.deltaTime);
            if (_img != null && _t > life * 0.6f)
            {
                var c = _img.color;
                c.a = Mathf.Lerp(1f, 0f, (_t - life * 0.6f) / (life * 0.4f));
                _img.color = c;
            }
            if (_t >= life) Destroy(gameObject);
        }

        /// <summary>Fire a confetti burst into <paramref name="layer"/>.</summary>
        public static void Burst(RectTransform layer, Sprite sprite, int count = 40)
        {
            if (layer == null || sprite == null) return;
            string[] palette = { "ff5d72", "ffce4f", "5fe0bd", "59c1ff", "b79cff", "ff9aa0", "ff8a3d" };
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Confetti");
                var img = go.AddComponent<Image>();
                img.sprite = sprite;
                img.raycastTarget = false;
                ColorUtility.TryParseHtmlString("#" + palette[Random.Range(0, palette.Length)], out var col);
                img.color = col;
                var rt = (RectTransform)go.transform;
                rt.SetParent(layer, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.one;
                rt.sizeDelta = new Vector2(Random.Range(22f, 40f), Random.Range(30f, 50f));
                rt.anchoredPosition = new Vector2(Random.Range(-220f, 220f), Random.Range(-40f, 80f));
                var p = go.AddComponent<ShapeConfetti>();
                p.velocity = new Vector2(Random.Range(-360f, 360f), Random.Range(300f, 600f));
                p.gravity = 720f;
            }
        }
    }
}
