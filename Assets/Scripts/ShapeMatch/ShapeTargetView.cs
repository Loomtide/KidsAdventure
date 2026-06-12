using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ShapeMatch
{
    /// <summary>
    /// The big target shape the child must match. Holds one glossy coloured sprite per
    /// ShapeKind and shows the round's shape with a little pop-in. Idle bob is handled by a
    /// separate ShapeBob on the same object.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ShapeTargetView : MonoBehaviour
    {
        [Tooltip("Glossy target sprite per ShapeKind, indexed Circle,Square,Triangle,Star.")]
        public Sprite[] targetSprites = new Sprite[4];

        Image _img;
        Coroutine _pop;
        Vector3 _baseScale = Vector3.one;

        void Awake()
        {
            _img = GetComponent<Image>();
            _baseScale = transform.localScale;
        }

        public void SetShape(ShapeKind k)
        {
            int i = (int)k;
            if (_img != null && targetSprites != null && i >= 0 && i < targetSprites.Length)
                _img.sprite = targetSprites[i];
            if (isActiveAndEnabled)
            {
                if (_pop != null) StopCoroutine(_pop);
                _pop = StartCoroutine(PopIn());
            }
        }

        IEnumerator PopIn()
        {
            float t = 0f, dur = 0.32f;
            while (t < dur)
            {
                t += Time.deltaTime;
                transform.localScale = _baseScale * (0.6f + 0.4f * EaseOutBack(t / dur));
                yield return null;
            }
            transform.localScale = _baseScale;
            _pop = null;
        }

        static float EaseOutBack(float x)
        {
            const float c1 = 1.7f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
    }
}
