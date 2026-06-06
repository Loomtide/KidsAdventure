using System;
using UnityEngine;

/// <summary>
/// Tracks how many fruit the player has tapped/counted this round and broadcasts changes.
/// The HUD count chip (hud-prompt slice) and the round-loop subscribe to <see cref="OnCountChanged"/>.
/// </summary>
public class CountManager : MonoBehaviour
{
    public static CountManager Instance { get; private set; }

    /// <summary>Fired whenever the live count changes. Argument is the new count.</summary>
    public event Action<int> OnCountChanged;

    [SerializeField] int count;
    public int Count => count;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Keep the game simulating when the editor/app is backgrounded (smooth demo + reliable testing).
        Application.runInBackground = true;
    }

    public void Add(int delta)
    {
        count += delta;
        OnCountChanged?.Invoke(count);
    }

    public void ResetCount()
    {
        count = 0;
        OnCountChanged?.Invoke(count);
    }
}
