using UnityEngine;

namespace ShapeMatch
{
    /// <summary>
    /// Tiny self-contained one-shot SFX helper for Shape Match (kept separate from
    /// CountTheFruits' SfxPlayer so the two games share no code). A single AudioSource
    /// plays clips at a base volume; callers use ShapeSfx.Play(clip).
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ShapeSfx : MonoBehaviour
    {
        public static ShapeSfx Instance { get; private set; }

        [Range(0f, 1f)] public float volume = 0.8f;
        AudioSource _src;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _src = GetComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.spatialBlend = 0f;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Play(AudioClip clip, float pitch = 1f, float volumeScale = 1f)
        {
            if (clip == null || Instance == null) return;
            Instance._src.pitch = pitch;
            Instance._src.PlayOneShot(clip, Instance.volume * volumeScale);
        }
    }
}
