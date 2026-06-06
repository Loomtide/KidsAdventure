using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads pointer/touch presses (Input System) and routes them to the Fruit under the
/// press point via a 2D physics overlap. Pointer.current covers mouse, touch and pen,
/// so it works in-editor, on device, and for real players.
/// </summary>
public class TapInput : MonoBehaviour
{
    public Camera cam;

    [Header("Debug (editor verification)")]
    [Tooltip("Set >0 at runtime to auto-tap that many uncounted fruit, one per frame.")]
    public int debugTapBurst;
    public bool debugLog = false;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (debugTapBurst > 0) { debugTapBurst--; TapNextFruit(); }

        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;

        Vector2 screenPos = pointer.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(screenPos);
        var hit = Physics2D.OverlapPoint(world);
        if (debugLog)
            Debug.Log($"[TapInput] press screen={screenPos} world=({world.x:F2},{world.y:F2}) hit={(hit ? hit.name : "null")}");
        if (hit == null) return;

        var fruit = hit.GetComponentInParent<Fruit>();
        if (fruit != null) fruit.Tap();
    }

    void TapNextFruit()
    {
        foreach (var f in FindObjectsByType<Fruit>(FindObjectsSortMode.None))
        {
            if (!f.Counted) { f.Tap(); return; }
        }
    }
}
