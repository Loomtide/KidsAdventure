using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ShapeMatch
{
    /// <summary>
    /// Top-right progress: a 5-star track that fills one gold star per completed round.
    /// Off stars are tinted soft grey; the just-earned star pops as it fills.
    /// </summary>
    public class ShapeHud : MonoBehaviour
    {
        public Image[] stars = new Image[5];
        public Color goldColor = new Color(1f, 0.807f, 0.31f, 1f);   // #ffce4f
        public Color offColor = new Color(0.906f, 0.878f, 0.937f, 1f); // #e7e0ef

        int _filled = -1;

        public void SetProgress(int filled)
        {
            filled = Mathf.Clamp(filled, 0, stars != null ? stars.Length : 0);
            if (stars != null)
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    if (stars[i] == null) continue;
                    bool on = i < filled;
                    stars[i].color = on ? goldColor : offColor;
                    if (on && i >= _filled && _filled >= 0 && isActiveAndEnabled)
                        StartCoroutine(Pop(stars[i].transform));
                }
            }
            _filled = filled;
        }

        IEnumerator Pop(Transform t)
        {
            float e = 0f, dur = 0.3f;
            Vector3 baseS = Vector3.one;
            while (e < dur)
            {
                e += Time.deltaTime;
                float k = e / dur;
                t.localScale = baseS * (1f + 0.6f * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            t.localScale = baseS;
        }
    }
}
