using UnityEngine;

/// <summary>
/// Gentle ambient horizontal drift for a background cloud on a static-camera scene.
/// When the cloud passes the right edge it wraps back to the left, so the sky never empties.
/// Speeds are tiny by design — this is calm kid-game atmosphere, not motion that pulls focus.
/// </summary>
[DisallowMultipleComponent]
public class CloudDrift : MonoBehaviour
{
    [Tooltip("Horizontal drift speed in world units / second. Slower = reads as farther away.")]
    public float speed = 0.15f;

    [Tooltip("X position at which the cloud wraps back to the left edge.")]
    public float rightWrapX = 11f;

    [Tooltip("X position the cloud resets to after wrapping.")]
    public float leftResetX = -11f;

    [Tooltip("Small vertical bob amplitude (world units). 0 disables.")]
    public float bobAmplitude = 0.06f;

    [Tooltip("Vertical bob speed.")]
    public float bobSpeed = 0.6f;

    float _baseY;
    float _phase;

    void Start()
    {
        _baseY = transform.position.y;
        // Stagger bob phase by start X so clouds don't bob in lockstep.
        _phase = transform.position.x * 1.37f;
    }

    void Update()
    {
        Vector3 p = transform.position;
        p.x += speed * Time.deltaTime;
        if (p.x > rightWrapX) p.x = leftResetX;
        p.y = _baseY + Mathf.Sin(Time.time * bobSpeed + _phase) * bobAmplitude;
        transform.position = p;
    }
}
