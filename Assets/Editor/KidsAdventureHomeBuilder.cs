using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using KidsAdventure;

/// <summary>
/// One-shot builder for the Kids Adventure home screen (Assets/Scenes/KidsAdventure.unity):
/// animated candy background (drifting clouds, floating balloons, twinkling sparkles),
/// rainbow logo, a 2x2 grid of mini-game tiles on green podiums (Count the Fruits,
/// Shape Match, 2x Coming Soon), a music toggle, and the looping home theme. Also
/// registers all three scenes in Build Settings so tiles and Home buttons can load scenes.
/// Re-runnable: rebuilds the scene from scratch each time. Editor-only, never ships.
/// </summary>
public static class KidsAdventureHomeBuilder
{
    const string ScenePath = "Assets/Scenes/KidsAdventure.unity";
    const string Art = "Assets/Art/";
    const string Home = "Assets/Art/home/";

    static readonly Color Ink = new Color(0.275f, 0.212f, 0.369f, 1f);   // #46365e
    static readonly Color Sky = new Color(0.659f, 0.902f, 1f, 1f);
    static readonly Color LockedTint = new Color(0.80f, 0.82f, 0.87f, 1f);

    static Font _fredoka, _nunito;

    // Flag-gated auto-build so the bridge can trigger this (drop the flag, recompile -> build).
    const string Flag = "Assets/.kabuild.flag";

    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnReload()
    {
        if (!System.IO.File.Exists(Flag)) return;
        EditorApplication.delayCall += () =>
        {
            try { Build(); }
            catch (System.Exception e) { Debug.LogError("[KidsAdventureHomeBuilder] " + e); }
            finally { try { System.IO.File.Delete(Flag); } catch { } AssetDatabase.Refresh(); }
        };
    }

    [MenuItem("KidsAdventure/Build Home Scene")]
    public static void Build()
    {
        _fredoka = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Fredoka.ttf");
        _nunito = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Nunito.ttf");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // --- camera ---
        var cam = Camera.main;
        if (cam == null)
        {
            var cg = new GameObject("Main Camera"); cg.tag = "MainCamera";
            cam = cg.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Sky;
        cam.transform.position = new Vector3(0, 0, -10);
        var dl = GameObject.Find("Directional Light");
        if (dl != null) Object.DestroyImmediate(dl);

        // --- event system ---
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        // --- canvas (Screen Space - Camera so bridge screenshots composite it) ---
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5f;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        var root = canvasGo.transform;

        // ============ BACKGROUND ============
        var bg = Empty("Background", root);
        Stretch(bg);
        var sky = Img("Sky", bg.transform, Load(Art + "bg/sky.png"));
        Stretch(sky.gameObject); sky.raycastTarget = false;
        var sun = Img("Sun", bg.transform, Load(Art + "bg/sun.png"));
        var srt = Place(sun, new Vector2(0.5f, 1f), new Vector2(480, -40), new Vector2(460, 460));
        sun.raycastTarget = false;
        var sunBob = sun.gameObject.AddComponent<HomeBob>(); sunBob.amplitude = 0f; sunBob.scalePulse = 0.06f; sunBob.speed = 0.9f;
        AddCloud(bg.transform, "Cloud1", new Vector2(-460, 240), 1.0f, 13f);
        AddCloud(bg.transform, "Cloud2", new Vector2(260, 280), 0.8f, -9f);
        AddCloud(bg.transform, "Cloud3", new Vector2(-60, 305), 0.62f, 7f);
        AddCloud(bg.transform, "Cloud4", new Vector2(520, 180), 0.55f, 10f);
        var hillFar = Img("HillFar", bg.transform, Load(Art + "bg/hill_far.png"));
        Place(hillFar, new Vector2(0.5f, 0f), new Vector2(0, 95), new Vector2(1500, 300));
        hillFar.rectTransform.pivot = new Vector2(0.5f, 0f); hillFar.raycastTarget = false;
        var hillNear = Img("HillNear", bg.transform, Load(Art + "bg/hill_near.png"));
        Place(hillNear, new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(1420, 340));
        hillNear.rectTransform.pivot = new Vector2(0.5f, 0f); hillNear.raycastTarget = false;
        var ground = Img("Ground", bg.transform, Load(Art + "bg/ground.png"));
        var grt = ground.rectTransform;
        grt.anchorMin = new Vector2(0, 0); grt.anchorMax = new Vector2(1, 0); grt.pivot = new Vector2(0.5f, 0f);
        grt.sizeDelta = new Vector2(0, 320); grt.anchoredPosition = new Vector2(0, 0);
        ground.raycastTarget = false;

        // floating balloons (wrap forever)
        AddBalloon(bg.transform, Home + "balloon_coral.png", -575, -180, 24f);
        AddBalloon(bg.transform, Home + "balloon_blue.png", 590, -420, 30f);
        AddBalloon(bg.transform, Home + "balloon_gold.png", -480, -560, 21f);

        // ============ LOGO ============
        var logo = Img("Logo", root, Load(Home + "logo.png"));
        Place(logo, new Vector2(0.5f, 1f), new Vector2(0, -82), new Vector2(745, 120));
        logo.raycastTarget = false;
        logo.gameObject.AddComponent<LogoWiggle>();

        // sparkles around logo + grass
        AddSparkle(root, new Vector2(-430, 290), 54);
        AddSparkle(root, new Vector2(415, 255), 44);
        AddSparkle(root, new Vector2(-560, 60), 36);
        AddSparkle(root, new Vector2(575, -40), 40);
        AddSparkle(root, new Vector2(-300, -320), 34);
        AddSparkle(root, new Vector2(330, -300), 38);

        // ============ AUDIO ============
        var audioGo = new GameObject("Audio");
        audioGo.transform.SetParent(root, false);
        var sfx = audioGo.AddComponent<AudioSource>();
        sfx.playOnAwake = false; sfx.spatialBlend = 0f;
        var musicGo = new GameObject("Music");
        musicGo.transform.SetParent(root, false);
        var music = musicGo.AddComponent<AudioSource>();
        music.clip = Aud("home_theme");
        music.loop = true; music.playOnAwake = true; music.volume = 0.42f; music.spatialBlend = 0f;

        // ============ GAME TILES (2x2 grid) ============
        var tiles = Empty("Tiles", root); Stretch(tiles);
        var tapClip = Aud("start");
        var lockedClip = Aud("buzz");

        // --- Count the Fruits ---
        var t1 = Tile(tiles.transform, "Tile_CountTheFruits", new Vector2(-262, 30), "Count the Fruits",
                      Home + "pill_coral.png", "CountTheFruits", false, sfx, tapClip, lockedClip, out var cluster1);
        IconImg(cluster1, "AppleL", Art + "fruit/apple.png", new Vector2(-74, -14), new Vector2(84, 84), 13f);
        IconImg(cluster1, "AppleR", Art + "fruit/apple.png", new Vector2(72, -16), new Vector2(80, 80), -11f);
        IconImg(cluster1, "Apple", Art + "fruit/apple.png", new Vector2(0, 8), new Vector2(112, 112), 0f);

        // --- Shape Match ---
        var t2 = Tile(tiles.transform, "Tile_ShapeMatch", new Vector2(262, 30), "Shape Match",
                      Home + "pill_blue.png", "ShapeMatch", false, sfx, tapClip, lockedClip, out var cluster2);
        IconImg(cluster2, "Circle", Art + "shapematch/circle_coral.png", new Vector2(-78, -12), new Vector2(78, 78), 0f);
        IconImg(cluster2, "Square", Art + "shapematch/square_blue.png", new Vector2(76, -14), new Vector2(76, 76), 8f);
        IconImg(cluster2, "Star", Art + "shapematch/star_gold.png", new Vector2(0, 14), new Vector2(106, 106), 0f);

        // --- Kids Chef ---
        var t3 = Tile(tiles.transform, "Tile_KidsChef", new Vector2(-262, -212), "Kids Chef",
                      Art + "chef/pill_gold.png", "KidsChef", false, sfx, tapClip, lockedClip, out var cluster3);
        IconImg(cluster3, "Waffle", Art + "chef/waffle_single.png", new Vector2(0, 4), new Vector2(104, 104), -6f);
        IconImg(cluster3, "Strawberry", Art + "chef/strawberry.png", new Vector2(-66, -22), new Vector2(64, 70), -10f);
        IconImg(cluster3, "Cream", Art + "chef/cream.png", new Vector2(64, -20), new Vector2(64, 61), 8f);
        // NEW badge (the newest game in the package)
        var badge = Img("NewBadge", t3.transform, Load(Home + "new_badge.png"));
        Place(badge, C(0.5f, 0.5f), new Vector2(132, 86), new Vector2(86, 86));
        badge.raycastTarget = false;
        var bb = badge.gameObject.AddComponent<HomeBob>(); bb.amplitude = 0f; bb.scalePulse = 0.12f; bb.speed = 2.6f;

        // --- Coming Soon ---
        var t4 = Tile(tiles.transform, "Tile_ComingSoon2", new Vector2(262, -212), "Coming Soon",
                      Home + "pill_grey.png", "", true, sfx, tapClip, lockedClip, out var cluster4);
        IconImg(cluster4, "Lock", Home + "lock.png", new Vector2(0, 0), new Vector2(92, 108), 0f);

        // ============ MUSIC TOGGLE (top-right) ============
        var musicBtn = Img("MusicButton", root, Load(Home + "btn_round_orange.png"));
        var mrt = musicBtn.rectTransform;
        mrt.anchorMin = mrt.anchorMax = new Vector2(1, 1); mrt.pivot = new Vector2(1, 1);
        mrt.sizeDelta = new Vector2(92, 92); mrt.anchoredPosition = new Vector2(-26, -26);
        var note = Img("Note", musicBtn.transform, Load(Home + "note_icon.png"));
        Place(note, C(0.5f, 0.5f), new Vector2(0, 2), new Vector2(48, 48));
        note.raycastTarget = false;
        var toggle = musicBtn.gameObject.AddComponent<HomeMusicToggle>();
        toggle.music = music; toggle.noteIcon = note;

        // ============ SAVE ============
        EditorSceneManager.MarkSceneDirty(scene);
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);

        // register the package's scenes (hub first = startup scene)
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/KidsAdventure.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/CountTheFruits.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/ShapeMatch.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/KidsChef.unity", true),
        };

        AssetDatabase.SaveAssets();
        Debug.Log("[KidsAdventureHomeBuilder] built + saved " + ScenePath);
    }

    // ---- tile factory: invisible hit-area root + pedestal + icon cluster + label pill ----
    static GameObject Tile(Transform parent, string name, Vector2 pos, string label, string pillPath,
                           string sceneName, bool locked, AudioSource sfx, AudioClip tap, AudioClip buzz,
                           out Transform iconCluster)
    {
        var rootImg = Img(name, parent, null);
        rootImg.color = new Color(1, 1, 1, 0f);           // invisible, but raycastable hit area
        rootImg.raycastTarget = true;
        Place(rootImg, C(0.5f, 0.5f), pos, new Vector2(360, 240));

        var btn = rootImg.gameObject.AddComponent<HomeTileButton>();
        btn.sceneName = sceneName; btn.locked = locked;
        btn.sfx = sfx; btn.tapClip = tap; btn.lockedClip = buzz;

        var pedestal = Img("Pedestal", rootImg.transform, Load(Home + "pedestal.png"));
        Place(pedestal, C(0.5f, 0.5f), new Vector2(0, -52), new Vector2(310, 105));
        pedestal.raycastTarget = false;
        if (locked) pedestal.color = LockedTint;

        var cluster = Empty("Icons", rootImg.transform);
        Place(cluster, C(0.5f, 0.5f), new Vector2(0, 36), new Vector2(280, 150));
        var bob = cluster.AddComponent<HomeBob>();
        bob.amplitude = locked ? 4f : 7f; bob.speed = locked ? 1.1f : 1.6f;
        iconCluster = cluster.transform;

        var pill = Img("Label", rootImg.transform, Load(pillPath));
        Place(pill, C(0.5f, 0.5f), new Vector2(0, -108), new Vector2(252, 64));
        pill.raycastTarget = false;
        var txt = Txt("Text", pill.transform, label, 30, Color.white, TextAnchor.MiddleCenter, _fredoka);
        Place(txt, C(0.5f, 0.5f), new Vector2(0, 2), new Vector2(240, 56));
        var sh = txt.gameObject.AddComponent<Shadow>(); sh.effectColor = new Color(Ink.r, Ink.g, Ink.b, 0.45f); sh.effectDistance = new Vector2(0, -2);

        return rootImg.gameObject;
    }

    static void IconImg(Transform cluster, string name, string spritePath, Vector2 pos, Vector2 size, float rotZ)
    {
        var img = Img(name, cluster, Load(spritePath));
        var rt = Place(img, C(0.5f, 0.5f), pos, size);
        rt.localRotation = Quaternion.Euler(0, 0, rotZ);
        img.raycastTarget = false;
    }

    static void AddCloud(Transform parent, string name, Vector2 pos, float scale, float driftSpeed)
    {
        var c = Img(name, parent, Load(Art + "bg/cloud.png"));
        Place(c, C(0.5f, 0.5f), pos, new Vector2(220 * scale, 127 * scale));
        c.raycastTarget = false;
        var d = c.gameObject.AddComponent<HomeDrift>(); d.speed = driftSpeed; d.range = 820f;
    }

    static void AddBalloon(Transform parent, string spritePath, float x, float startY, float rise)
    {
        var b = Img("Balloon", parent, Load(spritePath));
        Place(b, C(0.5f, 0.5f), new Vector2(x, startY), new Vector2(92, 138));
        b.raycastTarget = false;
        var fl = b.gameObject.AddComponent<HomeBalloon>();
        fl.riseSpeed = rise; fl.topY = 520f; fl.bottomY = -560f;
    }

    static void AddSparkle(Transform parent, Vector2 pos, float size)
    {
        var s = Img("Sparkle", parent, Load(Home + "sparkle.png"));
        Place(s, C(0.5f, 0.5f), pos, new Vector2(size, size));
        s.raycastTarget = false;
        s.gameObject.AddComponent<HomeSparkle>();
    }

    // ---- shared helpers (same idioms as ShapeMatchSceneBuilder) ----
    static Vector2 C(float x, float y) => new Vector2(x, y);
    static Sprite Load(string p) { var s = AssetDatabase.LoadAssetAtPath<Sprite>(p); if (s == null) Debug.LogError("missing sprite " + p); return s; }
    static AudioClip Aud(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/" + name + ".wav");

    static GameObject Empty(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static Image Img(string name, Transform parent, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        return img;
    }

    static Text Txt(string name, Transform parent, string text, int size, Color color, TextAnchor anchor, Font font)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = text; t.fontSize = size; t.color = color; t.alignment = anchor;
        t.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; rt.localScale = Vector3.one;
    }

    static RectTransform Place(Image img, Vector2 anchor, Vector2 pos, Vector2 size) => Place(img.gameObject, anchor, pos, size);
    static RectTransform Place(Text t, Vector2 anchor, Vector2 pos, Vector2 size) => Place(t.gameObject, anchor, pos, size);
    static RectTransform Place(GameObject go, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size; rt.anchoredPosition = pos; rt.localScale = Vector3.one;
        return rt;
    }
}
