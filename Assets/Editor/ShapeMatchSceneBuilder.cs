using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ShapeMatch;

/// <summary>
/// One-shot builder for the Shape Match scene (Assets/Scenes/ShapeMatch.unity). Constructs
/// the whole UI — parallax background, Start screen, active-round HUD/target/answers, and the
/// Reward screen — and wires every script reference + sprite, then saves. Re-runnable: it
/// rebuilds the scene from scratch each time. Lives in Assets/Editor so it is editor-only and
/// never ships. Does NOT touch CountTheFruits.
/// </summary>
public static class ShapeMatchSceneBuilder
{
    const string ScenePath = "Assets/Scenes/ShapeMatch.unity";
    const string Art = "Assets/Art/";
    const string SM = "Assets/Art/shapematch/";

    static readonly Color Ink = new Color(0.275f, 0.212f, 0.369f, 1f);   // #46365e
    static readonly Color Coral = new Color(1f, 0.365f, 0.447f, 1f);     // #ff5d72
    static readonly Color Gold = new Color(1f, 0.807f, 0.31f, 1f);       // #ffce4f
    static readonly Color Off = new Color(0.906f, 0.878f, 0.937f, 1f);   // #e7e0ef
    static readonly Color Sky = new Color(0.659f, 0.902f, 1f, 1f);

    static Font _fredoka, _nunito;

    // Flag-gated auto-build so the bridge can trigger this (drop the flag, recompile → build).
    const string Flag = "Assets/.smbuild.flag";

    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnReload()
    {
        if (!System.IO.File.Exists(Flag)) return;
        EditorApplication.delayCall += () =>
        {
            try { Build(); }
            catch (System.Exception e) { Debug.LogError("[ShapeMatchSceneBuilder] " + e); }
            finally { try { System.IO.File.Delete(Flag); } catch { } AssetDatabase.Refresh(); }
        };
    }

    [MenuItem("ShapeMatch/Build Scene")]
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
        // drop the default directional light — pure 2D UI
        var dl = GameObject.Find("Directional Light");
        if (dl != null) Object.DestroyImmediate(dl);

        // --- event system (StandaloneInputModule; project uses Both input handling) ---
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        // --- canvas (Screen Space - Camera so the bridge screenshots composite it) ---
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
        Place(Img("Sun", bg.transform, Load(Art + "bg/sun.png")), C(0.5f, 1f), new Vector2(420, -60), new Vector2(520, 520)).GetComponent<Image>().raycastTarget = false;
        AddCloud(bg.transform, "Cloud1", new Vector2(-440, 250), 1.0f, 14f);
        AddCloud(bg.transform, "Cloud2", new Vector2(250, 270), 0.85f, -10f);
        AddCloud(bg.transform, "Cloud3", new Vector2(-90, 300), 0.7f, 8f);
        var hillFar = Img("HillFar", bg.transform, Load(Art + "bg/hill_far.png"));
        Place(hillFar, new Vector2(0.5f, 0f), new Vector2(0, 95), new Vector2(1500, 300)); hillFar.rectTransform.pivot = new Vector2(0.5f, 0f); hillFar.raycastTarget = false;
        var hillNear = Img("HillNear", bg.transform, Load(Art + "bg/hill_near.png"));
        Place(hillNear, new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(1420, 340)); hillNear.rectTransform.pivot = new Vector2(0.5f, 0f); hillNear.raycastTarget = false;
        var ground = Img("Ground", bg.transform, Load(Art + "bg/ground.png"));
        var grt = ground.rectTransform; grt.anchorMin = new Vector2(0, 0); grt.anchorMax = new Vector2(1, 0); grt.pivot = new Vector2(0.5f, 0f);
        grt.sizeDelta = new Vector2(0, 150); grt.anchoredPosition = new Vector2(0, 0); ground.raycastTarget = false;

        // ============ GAMEPLAY ROOT ============
        var gameplay = Empty("GameplayRoot", root); Stretch(gameplay);

        // banner + mascot + prompt
        var banner = Img("Banner", gameplay.transform, Load(Art + "hud/card_white.png"));
        banner.type = Image.Type.Sliced; banner.raycastTarget = false;
        Place(banner, C(0.5f, 0.5f), new Vector2(0, 250), new Vector2(660, 104));
        var mascot = Img("Mascot", banner.transform, Load(Art + "hud/mascot.png"));
        Place(mascot, C(0.5f, 0.5f), new Vector2(-262, 4), new Vector2(82, 82)); mascot.raycastTarget = false;
        mascot.gameObject.AddComponent<ShapeBob>().amplitude = 4f;
        var q = Txt("Prompt", banner.transform, "Find the matching shape!", 40, Ink, TextAnchor.MiddleCenter, _fredoka);
        Place(q, C(0.5f, 0.5f), new Vector2(36, 0), new Vector2(520, 92));

        // target shadow + shape
        var tshadow = Img("TargetShadow", gameplay.transform, Load(Art + "fruit/fruit_shadow.png"));
        Place(tshadow, C(0.5f, 0.5f), new Vector2(0, -50), new Vector2(240, 64)); tshadow.color = new Color(0.27f, 0.16f, 0.39f, 0.18f); tshadow.raycastTarget = false;
        var targetGo = Img("TargetShape", gameplay.transform, Load(SM + "star_gold.png"));
        Place(targetGo, C(0.5f, 0.5f), new Vector2(0, 60), new Vector2(212, 212)); targetGo.raycastTarget = false;
        var targetView = targetGo.gameObject.AddComponent<ShapeTargetView>();
        targetView.targetSprites = new[] { Load(SM + "circle_coral.png"), Load(SM + "square_blue.png"), Load(SM + "triangle_mint.png"), Load(SM + "star_gold.png") };
        targetGo.gameObject.AddComponent<ShapeBob>().amplitude = 9f;

        // answers
        var answers = Empty("Answers", gameplay.transform); Stretch(answers);
        var panel = answers.AddComponent<ShapeAnswerPanel>();
        string[] btnSprites = { SM + "button_coral.png", SM + "button_blue.png", SM + "button_lav.png" };
        var shapeButtons = new ShapeButton[3];
        float[] xs = { -330, 0, 330 };
        for (int i = 0; i < 3; i++)
        {
            var b = Img("AnswerButton" + i, answers.transform, Load(btnSprites[i]));
            Place(b, C(0.5f, 0.5f), new Vector2(xs[i], -222), new Vector2(184, 158));
            var sb = b.gameObject.AddComponent<ShapeButton>();
            var shapeImg = Img("Shape", b.transform, Load(SM + "circle_white.png"));
            Place(shapeImg, C(0.5f, 0.5f), new Vector2(0, 6), new Vector2(98, 98)); shapeImg.raycastTarget = false;
            var chk = Img("Check", b.transform, Load(Art + "fx/check_badge.png"));
            Place(chk, C(0.5f, 0.5f), new Vector2(78, 72), new Vector2(66, 66)); chk.raycastTarget = false; chk.gameObject.SetActive(false);
            var xb = Img("X", b.transform, Load(Art + "fx/cross.png"));
            Place(xb, C(0.5f, 0.5f), new Vector2(78, 72), new Vector2(64, 64)); xb.raycastTarget = false; xb.gameObject.SetActive(false);
            sb.shapeImage = shapeImg; sb.checkBadge = chk; sb.xBadge = xb;
            shapeButtons[i] = sb;
        }
        panel.buttons = shapeButtons;
        panel.whiteSprites = new[] { Load(SM + "circle_white.png"), Load(SM + "square_white.png"), Load(SM + "triangle_white.png"), Load(SM + "star_white.png") };
        panel.confettiSprite = Load(Art + "ui/confetti.png");
        panel.chimeClip = Aud("chime"); panel.buzzClip = Aud("buzz");

        // progress HUD (top-right)
        var prog = Img("Progress", gameplay.transform, Load(Art + "hud/card_white.png"));
        prog.type = Image.Type.Sliced; prog.raycastTarget = false;
        var prt = prog.rectTransform; prt.anchorMin = prt.anchorMax = new Vector2(1, 1); prt.pivot = new Vector2(1, 1);
        prt.sizeDelta = new Vector2(258, 76); prt.anchoredPosition = new Vector2(-26, -26);
        var hud = prog.gameObject.AddComponent<ShapeHud>();
        var hudStars = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            var st = Img("Star" + i, prog.transform, Load(SM + "star_white.png"));
            Place(st, C(0.5f, 0.5f), new Vector2(-92 + i * 46, 0), new Vector2(36, 36)); st.raycastTarget = false; st.color = Off;
            hudStars[i] = st;
        }
        hud.stars = hudStars; hud.goldColor = Gold; hud.offColor = Off;

        // ============ START SCREEN ============
        var start = Empty("StartScreen", root); Stretch(start);
        // decorative floating shapes
        AddDeco(start.transform, SM + "circle_coral.png", new Vector2(-440, -130), 120);
        AddDeco(start.transform, SM + "square_blue.png", new Vector2(440, -110), 120);
        AddDeco(start.transform, SM + "triangle_mint.png", new Vector2(-545, 150), 112);
        AddDeco(start.transform, SM + "star_gold.png", new Vector2(545, 150), 120);
        var title = Txt("Title", start.transform, "Shape Match", 110, Coral, TextAnchor.MiddleCenter, _fredoka);
        Place(title, C(0.5f, 0.5f), new Vector2(0, 150), new Vector2(1000, 200));
        var ol = title.gameObject.AddComponent<Outline>(); ol.effectColor = Color.white; ol.effectDistance = new Vector2(4, -4);
        var sub = Txt("Subtitle", start.transform, "Tap the shape that matches!", 40, Ink, TextAnchor.MiddleCenter, _nunito);
        Place(sub, C(0.5f, 0.5f), new Vector2(0, 30), new Vector2(900, 80));
        var startBtn = Img("StartButton", start.transform, Load(SM + "button_blue.png"));
        Place(startBtn, C(0.5f, 0.5f), new Vector2(0, -150), new Vector2(320, 150));
        var startTxt = Txt("Label", startBtn.transform, "Play", 64, Color.white, TextAnchor.MiddleCenter, _fredoka);
        Place(startTxt, C(0.5f, 0.5f), new Vector2(0, 6), new Vector2(300, 120));
        var startMenu = startBtn.gameObject.AddComponent<ShapeMenuButton>(); startMenu.action = ShapeMenuButton.Action.Play;

        // ============ REWARD SCREEN ============
        var reward = Empty("RewardPanel", root); Stretch(reward);
        var card = Img("Card", reward.transform, Load(Art + "hud/card_white.png"));
        card.type = Image.Type.Sliced; card.raycastTarget = false;
        Place(card, C(0.5f, 0.5f), new Vector2(0, 20), new Vector2(780, 440));
        var great = Txt("GreatJob", card.transform, "Great job!", 84, Coral, TextAnchor.MiddleCenter, _fredoka);
        Place(great, C(0.5f, 0.5f), new Vector2(0, 120), new Vector2(700, 140));
        var go2 = great.gameObject.AddComponent<Outline>(); go2.effectColor = Color.white; go2.effectDistance = new Vector2(3, -3);
        for (int i = 0; i < 5; i++)
        {
            var st = Img("RewardStar" + i, card.transform, Load(SM + "star_white.png"));
            Place(st, C(0.5f, 0.5f), new Vector2(-180 + i * 90, 6), new Vector2(72, 72)); st.color = Gold; st.raycastTarget = false;
            st.gameObject.AddComponent<ShapeBob>().amplitude = 6f;
        }
        var again = Img("PlayAgainButton", card.transform, Load(SM + "button_blue.png"));
        Place(again, C(0.5f, 0.5f), new Vector2(0, -130), new Vector2(340, 132));
        var againTxt = Txt("Label", again.transform, "Play Again", 46, Color.white, TextAnchor.MiddleCenter, _fredoka);
        Place(againTxt, C(0.5f, 0.5f), new Vector2(0, 4), new Vector2(320, 110));
        var againMenu = again.gameObject.AddComponent<ShapeMenuButton>(); againMenu.action = ShapeMenuButton.Action.PlayAgain;

        // ============ HOME BUTTON (always visible; same look/spot as Kids Chef) ============
        var home = Img("HomeButton", root, Load(Art + "chef/btn_round_pink.png"));
        var hrt = home.rectTransform; hrt.anchorMin = hrt.anchorMax = new Vector2(0, 1); hrt.pivot = new Vector2(0, 1);
        hrt.sizeDelta = new Vector2(88, 88); hrt.anchoredPosition = new Vector2(24, -24);
        var hicon = Img("Icon", home.transform, Load(Art + "ui/home_icon.png"));
        Place(hicon, C(0.5f, 0.5f), new Vector2(0, 2), new Vector2(46, 46)); hicon.raycastTarget = false;
        var homeMenu = home.gameObject.AddComponent<ShapeMenuButton>(); homeMenu.action = ShapeMenuButton.Action.Home;

        // ============ CONFETTI LAYER (on top) ============
        var confetti = Empty("ConfettiLayer", root); Stretch(confetti);
        confetti.GetComponent<RectTransform>().SetAsLastSibling();
        var canvasGroup = confetti.AddComponent<CanvasGroup>(); canvasGroup.blocksRaycasts = false; canvasGroup.interactable = false;
        panel.confettiLayer = confetti.GetComponent<RectTransform>();

        // ============ AUDIO ============
        var audioGo = new GameObject("Audio");
        audioGo.transform.SetParent(root, false);
        audioGo.AddComponent<AudioSource>();
        var sfx = audioGo.AddComponent<ShapeSfx>(); sfx.volume = 0.8f;
        var musicGo = new GameObject("Music"); musicGo.transform.SetParent(root, false);
        var music = musicGo.AddComponent<AudioSource>();
        music.clip = Aud("music_bed"); music.loop = true; music.playOnAwake = true; music.volume = 0.32f; music.spatialBlend = 0f;

        // ============ GAME MANAGER (wire everything) ============
        var gmGo = new GameObject("GameManager");
        gmGo.transform.SetParent(root, false);
        var gm = gmGo.AddComponent<ShapeGameManager>();
        gm.target = targetView;
        gm.answerPanel = panel;
        gm.hud = hud;
        gm.startScreen = start;
        gm.gameplayRoot = gameplay;
        gm.rewardPanel = reward;
        gm.homeButton = home.gameObject;
        gm.startClip = Aud("start");
        gm.fanfareClip = Aud("fanfare");
        gm.totalRounds = 5;
        gm.nextRoundDelay = 0.9f;
        startMenu.manager = gm; againMenu.manager = gm; homeMenu.manager = gm;

        // initial visibility — Start screen up
        gameplay.SetActive(false);
        reward.SetActive(false);
        home.gameObject.SetActive(true);   // Home stays available on every screen
        start.SetActive(true);

        EditorSceneManager.MarkSceneDirty(scene);
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[ShapeMatchSceneBuilder] built + saved " + ScenePath);
    }

    // ---- helpers ----
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

    static void AddCloud(Transform parent, string name, Vector2 pos, float scale, float driftSpeed)
    {
        var c = Img(name, parent, Load(Art + "bg/cloud.png"));
        Place(c, C(0.5f, 0.5f), pos, new Vector2(220 * scale, 127 * scale));
        c.raycastTarget = false;
        var d = c.gameObject.AddComponent<ShapeDrift>(); d.speed = driftSpeed; d.range = 760f;
    }

    static void AddDeco(Transform parent, string spritePath, Vector2 pos, float size)
    {
        var d = Img("Deco", parent, Load(spritePath));
        Place(d, C(0.5f, 0.5f), pos, new Vector2(size, size)); d.raycastTarget = false;
        d.color = new Color(1, 1, 1, 0.9f);
        d.gameObject.AddComponent<ShapeBob>().amplitude = 12f;
    }
}
