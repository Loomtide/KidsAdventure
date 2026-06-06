using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scatters N fruit instances at non-overlapping random positions inside a play-area
/// rectangle. Rejection-samples against already-placed fruit to keep spacing readable
/// for counting. The round-loop drives Spawn(n); spawnOnStart is a standalone-preview fallback.
/// </summary>
public class FruitSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject fruitPrefab;

    [Header("Preview")]
    [Tooltip("Spawn previewCount on Start when no GameManager drives the rounds.")]
    public bool spawnOnStart = true;
    public int previewCount = 6;

    [Header("Play area (world units)")]
    public Vector2 areaCenter = new Vector2(0f, 0.7f);
    public Vector2 areaSize = new Vector2(12f, 3.4f);

    [Header("Spacing")]
    [Tooltip("Minimum centre-to-centre distance between fruit.")]
    public float minSpacing = 1.7f;
    public int maxTriesPerFruit = 48;

    readonly List<GameObject> _spawned = new List<GameObject>();

    public IReadOnlyList<GameObject> Spawned => _spawned;

    void Start()
    {
        if (spawnOnStart && fruitPrefab != null) Spawn(previewCount);
    }

    /// <summary>Clear the field and scatter <paramref name="n"/> non-overlapping fruit.</summary>
    public void Spawn(int n)
    {
        Clear();
        var placed = new List<Vector2>(n);
        float halfX = areaSize.x * 0.5f;
        float halfY = areaSize.y * 0.5f;

        for (int i = 0; i < n; i++)
        {
            Vector2 p = areaCenter;
            for (int t = 0; t < maxTriesPerFruit; t++)
            {
                Vector2 cand = areaCenter + new Vector2(
                    Random.Range(-halfX, halfX),
                    Random.Range(-halfY, halfY));
                bool ok = true;
                foreach (var q in placed)
                {
                    if (Vector2.Distance(cand, q) < minSpacing) { ok = false; break; }
                }
                p = cand;
                if (ok) break;
            }
            placed.Add(p);
            var go = Instantiate(fruitPrefab, new Vector3(p.x, p.y, 0f), Quaternion.identity, transform);
            go.name = "Apple_" + i;
            float s = Random.Range(0.92f, 1.08f);
            go.transform.localScale = new Vector3(s, s, 1f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-7f, 7f));
            _spawned.Add(go);
        }
    }

    public void Clear()
    {
        foreach (var g in _spawned)
        {
            if (g == null) continue;
            if (Application.isPlaying) Destroy(g);
            else DestroyImmediate(g);
        }
        _spawned.Clear();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.55f, 0.5f);
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
#endif
}
