using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KidsChef
{
    /// <summary>
    /// Finale confetti: spawns tinted pieces above the screen that flutter down,
    /// sway, and spin. Pieces clean themselves up below the bottom edge.
    /// </summary>
    public class ChefConfetti : MonoBehaviour
    {
        public Sprite sprite;
        public RectTransform layer;

        static readonly Color[] Tints =
        {
            new Color(1f, 0.36f, 0.45f), new Color(1f, 0.62f, 0.25f), new Color(1f, 0.81f, 0.31f),
            new Color(0.37f, 0.84f, 0.54f), new Color(0.31f, 0.71f, 1f), new Color(0.63f, 0.43f, 0.91f),
        };

        public void Burst(int count)
        {
            StartCoroutine(Spawn(count));
        }

        IEnumerator Spawn(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Confetti", typeof(RectTransform));
                go.transform.SetParent(layer, false);
                var img = go.AddComponent<Image>();
                img.sprite = sprite;
                img.color = Tints[Random.Range(0, Tints.Length)];
                img.raycastTarget = false;
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(Random.Range(16f, 30f), Random.Range(12f, 22f));
                rt.anchoredPosition = new Vector2(Random.Range(-600f, 600f), Random.Range(380f, 480f));
                var piece = go.AddComponent<ChefConfettiPiece>();
                piece.fall = Random.Range(140f, 260f);
                piece.sway = Random.Range(30f, 80f);
                piece.spin = Random.Range(-220f, 220f);
                if (i % 4 == 3) yield return null; // stagger spawning a little
            }
        }
    }

    /// <summary>One falling confetti piece (runtime-only, added by ChefConfetti).</summary>
    public class ChefConfettiPiece : MonoBehaviour
    {
        public float fall = 180f;
        public float sway = 50f;
        public float spin = 120f;

        RectTransform _rt;
        float _phase;
        float _x0;

        void Start()
        {
            _rt = (RectTransform)transform;
            _phase = Random.Range(0f, Mathf.PI * 2f);
            _x0 = _rt.anchoredPosition.x;
        }

        void Update()
        {
            var p = _rt.anchoredPosition;
            p.y -= fall * Time.deltaTime;
            p.x = _x0 + Mathf.Sin(Time.time * 2.2f + _phase) * sway;
            _rt.anchoredPosition = p;
            _rt.localRotation = Quaternion.Euler(0f, 0f, spin * Time.time + _phase * 40f);
            if (p.y < -440f) Destroy(gameObject);
        }
    }
}
