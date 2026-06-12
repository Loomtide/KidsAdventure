using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// One-shot style polish so all three mini-games share the Kids Chef HUD language:
/// the Home button is the same pink round button with a home icon, top-left at (24,-24),
/// and it is visible on every screen including each game's own start screen.
/// CountTheFruits is a hand-authored scene, so its button is restyled in place here;
/// Shape Match is rebuilt by its own builder (whose home-button block was updated).
/// </summary>
public static class KidsPolishBuilder
{
    const string PinkBtn = "Assets/Art/chef/btn_round_pink.png";
    const string HomeIcon = "Assets/Art/ui/home_icon.png";

    [MenuItem("KidsAdventure/Polish Home Buttons (all games)")]
    public static void PolishAll()
    {
        PolishCountTheFruits();
        ShapeMatchSceneBuilder.Build();
        Debug.Log("[KidsPolishBuilder] polish complete");
    }

    public static void PolishCountTheFruits()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/CountTheFruits.unity", OpenSceneMode.Single);

        HomeButton hb = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            var found = root.GetComponentsInChildren<HomeButton>(true);
            if (found.Length > 0) { hb = found[0]; break; }
        }
        if (hb == null) { Debug.LogError("[KidsPolishBuilder] HomeButton not found in CountTheFruits"); return; }

        var canvas = hb.GetComponentInParent<Canvas>(true);
        var rt = (RectTransform)hb.transform;

        // always visible (it lived under the gameplay HUD before, so it vanished on the
        // start screen) and in the shared top-left spot
        rt.SetParent(canvas.transform, false);
        rt.SetAsLastSibling();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.sizeDelta = new Vector2(88, 88);
        rt.anchoredPosition = new Vector2(24, -24);
        hb.gameObject.SetActive(true);

        var img = hb.GetComponent<Image>();
        img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PinkBtn);
        img.type = Image.Type.Simple;
        img.color = Color.white;

        var icon = hb.transform.Find("Icon");
        if (icon != null)
        {
            var irt = (RectTransform)icon;
            irt.anchorMin = irt.anchorMax = irt.pivot = new Vector2(0.5f, 0.5f);
            irt.sizeDelta = new Vector2(46, 46);
            irt.anchoredPosition = new Vector2(0, 2);
            var iimg = icon.GetComponent<Image>();
            iimg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HomeIcon);
            iimg.raycastTarget = false;
        }

        // the count chip used to live where the Home button now sits — park it beside
        // the button, centre-aligned with it
        foreach (var root in scene.GetRootGameObjects())
        {
            var chip = root.transform.Find("CountChip") as RectTransform;
            if (chip != null)
            {
                chip.anchoredPosition = new Vector2(124, -20);
                EditorUtility.SetDirty(chip);
            }
        }

        // StartController hides everything in gameplayUI on the start screen — the Home
        // button must leave that group so it stays visible there too
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var sc in root.GetComponentsInChildren<StartController>(true))
            {
                if (sc.gameplayUI == null) continue;
                var kept = new System.Collections.Generic.List<GameObject>();
                foreach (var g in sc.gameplayUI)
                    if (g != null && g != hb.gameObject) kept.Add(g);
                sc.gameplayUI = kept.ToArray();
                EditorUtility.SetDirty(sc);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[KidsPolishBuilder] restyled CountTheFruits HomeButton");
    }
}
