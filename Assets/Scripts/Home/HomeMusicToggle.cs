using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KidsAdventure
{
    /// <summary>
    /// Round music button (top-right). Tap to mute/unmute the home theme; the note icon
    /// dims while muted. debugClick allows bridge verification.
    /// </summary>
    public class HomeMusicToggle : MonoBehaviour, IPointerClickHandler
    {
        public AudioSource music;
        public Image noteIcon;

        [Header("Debug (editor verification)")]
        public bool debugClick;

        RectTransform _rt;
        Vector3 _baseScale;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _baseScale = _rt.localScale;
        }

        void Update()
        {
            if (debugClick) { debugClick = false; Click(); }
            // tiny idle pulse
            _rt.localScale = _baseScale * (1f + 0.025f * Mathf.Sin(Time.time * 2.4f));
        }

        public void OnPointerClick(PointerEventData e) => Click();

        public void Click()
        {
            if (music == null) return;
            music.mute = !music.mute;
            if (noteIcon != null)
            {
                var c = noteIcon.color;
                c.a = music.mute ? 0.35f : 1f;
                noteIcon.color = c;
            }
        }
    }
}
