using UnityEngine;

namespace KidsAdventure
{
    /// <summary>
    /// Playful idle for the title logo: a gentle rocking rotation plus a soft scale
    /// breathe, like a sticker bobbing on the title screen.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class LogoWiggle : MonoBehaviour
    {
        public float rockDegrees = 1.6f;
        public float rockSpeed = 1.1f;
        public float breathe = 0.025f;
        public float breatheSpeed = 1.7f;

        RectTransform _rt;
        Vector3 _baseScale;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _baseScale = _rt.localScale;
        }

        void Update()
        {
            _rt.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * rockSpeed) * rockDegrees);
            _rt.localScale = _baseScale * (1f + Mathf.Sin(Time.time * breatheSpeed) * breathe);
        }
    }
}
