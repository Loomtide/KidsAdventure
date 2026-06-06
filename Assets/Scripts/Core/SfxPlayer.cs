using UnityEngine;

/// <summary>
/// Tiny 2D one-shot SFX helper. A single pooled-ish AudioSource plays clips at a
/// configurable base volume. Other systems call SfxPlayer.Play(clip).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SfxPlayer : MonoBehaviour
{
    public static SfxPlayer Instance { get; private set; }

    [Range(0f, 1f)] public float volume = 0.8f;
    AudioSource _src;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.spatialBlend = 0f;
    }

    public static void Play(AudioClip clip, float pitch = 1f, float volumeScale = 1f)
    {
        if (clip == null || Instance == null) return;
        Instance._src.pitch = pitch;
        Instance._src.PlayOneShot(clip, Instance.volume * volumeScale);
    }
}
