using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using KidsChef;

/// <summary>
/// One-shot builder for the Kids Chef waffle game (Assets/Scenes/KidsChef.unity):
/// three step panels (mix the batter, cook in the waffle maker, decorate) plus a finale,
/// all driven by ChefGameManager. BuildAll also rebuilds the Kids Adventure hub so the
/// new game appears on a tile. Re-runnable; editor-only, never ships.
/// </summary>
public static class KidsChefBuilder
{
    const string ScenePath = "Assets/Scenes/KidsChef.unity";
    const string Chef = "Assets/Art/chef/";
    const string Home = "Assets/Art/home/";
    const string Ui = "Assets/Art/ui/";

    static readonly Color Ink = new Color(0.275f, 0.212f, 0.369f, 1f);   // #46365e

    static Font _fredoka;

    [MenuItem("KidsAdventure/Build Chef Scene + Hub")]
    public static void BuildAll()
    {
        Build();
        KidsAdventureHomeBuilder.Build();
    }

    [MenuItem("KidsAdventure/Build Chef Scene")]
    public static void Build()
    {
        _fredoka = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Fredoka.ttf");

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
        cam.backgroundColor = new Color(0.91f, 0.60f, 0.32f, 1f);
        cam.transform.position = new Vector3(0, 0, -10);
        var dl = GameObject.Find("Directional Light");
        if (dl != null) Object.DestroyImmediate(dl);

        // --- event system ---
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        // --- canvas ---
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

        // --- manager ---
        var mgrGo = new GameObject("ChefGameManager");
        mgrGo.transform.SetParent(root, false);
        var mgr = mgrGo.AddComponent<ChefGameManager>();

        // ============ BACKGROUNDS ============
        var wood = Img("WoodBg", root, Load(Chef + "wood.png"));
        Stretch(wood.gameObject); wood.raycastTarget = false;
        var kitchen = Img("KitchenBg", root, Load(Chef + "kitchen.png"));
        Stretch(kitchen.gameObject); kitchen.raycastTarget = false;
        mgr.woodBg = wood.gameObject; mgr.kitchenBg = kitchen.gameObject;

        // ============ AUDIO ============
        var audioGo = new GameObject("Audio");
        audioGo.transform.SetParent(root, false);
        var sfx = audioGo.AddComponent<AudioSource>();
        sfx.playOnAwake = false; sfx.spatialBlend = 0f;
        var musicGo = new GameObject("Music");
        musicGo.transform.SetParent(root, false);
        var music = musicGo.AddComponent<AudioSource>();
        music.clip = Aud("chef_theme");
        music.loop = true; music.playOnAwake = true; music.volume = 0.36f; music.spatialBlend = 0f;
        mgr.sfx = sfx;

        // ============ STEP 1 - MIX ============
        var mix = Empty("MixPanel", root); Stretch(mix);
        var bowlPos = new Vector2(60, -40);

        var bowl = Img("Bowl", mix.transform, Load(Chef + "bowl_top.png"));
        Place(bowl, bowlPos, new Vector2(440, 440)); bowl.raycastTarget = false;

        var raw = Empty("RawContents", mix.transform);
        Place(raw, bowlPos, new Vector2(330, 330));
        var rawCg = raw.AddComponent<CanvasGroup>();
        string[] contentSprites = { "milk_fill", "egg_yolk", "sugar_pile", "honey_blob", "banana_bits" };
        var contentGos = new GameObject[contentSprites.Length];
        for (int i = 0; i < contentSprites.Length; i++)
        {
            var c = Img(contentSprites[i], raw.transform, Load(Chef + contentSprites[i] + ".png"));
            Place(c, Vector2.zero, new Vector2(330, 330)); c.raycastTarget = false;
            contentGos[i] = c.gameObject;
        }
        var lumpy = Img("BatterLumpy", mix.transform, Load(Chef + "batter_lumpy.png"));
        Place(lumpy, bowlPos, new Vector2(330, 330)); lumpy.raycastTarget = false;
        var smooth = Img("BatterSmooth", mix.transform, Load(Chef + "batter_smooth.png"));
        Place(smooth, bowlPos, new Vector2(330, 330)); smooth.raycastTarget = false;

        var zoneImg = Img("MixZone", mix.transform, null);
        zoneImg.color = new Color(1, 1, 1, 0); zoneImg.raycastTarget = true;
        Place(zoneImg, bowlPos, new Vector2(390, 390));
        var mixZone = zoneImg.gameObject.AddComponent<ChefMixZone>();
        mixZone.manager = mgr; mixZone.maxRadius = 140f;

        var whisk = Img("Whisk", mix.transform, Load(Chef + "whisk.png"));
        Place(whisk, bowlPos + new Vector2(150, 110), new Vector2(105, 220));
        whisk.raycastTarget = false;
        mixZone.whisk = whisk.rectTransform;

        // draggable ingredients (left side), all targeting the bowl
        var ing = new ChefIngredient[5];
        ing[0] = Ingredient(mix.transform, mgr, "milk", Chef + "milk_jug.png", new Vector2(-490, 170), new Vector2(150, 173), bowl.rectTransform);
        ing[1] = Ingredient(mix.transform, mgr, "egg", Chef + "egg.png", new Vector2(-420, -255), new Vector2(92, 115), bowl.rectTransform);
        ing[2] = Ingredient(mix.transform, mgr, "sugar", Chef + "spoon_sugar.png", new Vector2(-500, -120), new Vector2(176, 88), bowl.rectTransform);
        ing[3] = Ingredient(mix.transform, mgr, "honey", Chef + "spoon_honey.png", new Vector2(-480, 25), new Vector2(176, 88), bowl.rectTransform);
        ing[4] = Ingredient(mix.transform, mgr, "banana", Chef + "banana_bowl.png", new Vector2(-300, 255), new Vector2(168, 120), bowl.rectTransform);

        mgr.mixPanel = mix;
        mgr.ingredients = ing;
        mgr.contents = contentGos;
        mgr.rawContents = rawCg;
        mgr.batterLumpy = lumpy;
        mgr.batterSmooth = smooth;
        mgr.mixZone = mixZone;
        mgr.whisk = whisk.rectTransform;

        // ============ STEP 2 - COOK ============
        var cook = Empty("CookPanel", root); Stretch(cook);
        var makerPos = new Vector2(-200, -30);
        var makerSize = new Vector2(440, 522);

        var makerOpen = Img("MakerOpen", cook.transform, Load(Chef + "maker_open.png"));
        Place(makerOpen, makerPos, makerSize); makerOpen.raycastTarget = false;
        var oil = Img("OilSheen", makerOpen.transform, Load(Chef + "oil_sheen.png"));
        StretchTo(oil); oil.raycastTarget = false;
        var batter4 = Img("BatterFill", makerOpen.transform, Load(Chef + "batter_4.png"));
        StretchTo(batter4); batter4.raycastTarget = false;
        var waffles4 = Img("Waffles4", makerOpen.transform, Load(Chef + "waffles_4.png"));
        StretchTo(waffles4); waffles4.raycastTarget = false;

        var makerClosed = Img("MakerClosed", cook.transform, Load(Chef + "maker_closed.png"));
        Place(makerClosed, makerPos + new Vector2(0, -90), new Vector2(440, 385));
        makerClosed.raycastTarget = false;

        var powerPos = makerPos + new Vector2(0, -makerSize.y * 0.437f);
        var glow = Img("PowerGlow", cook.transform, Load(Chef + "power_glow.png"));
        Place(glow, powerPos, new Vector2(130, 130)); glow.raycastTarget = false;
        var powerBtn = Img("PowerButton", cook.transform, Load(Chef + "power_btn.png"));
        Place(powerBtn, powerPos, new Vector2(64, 64));
        var powerChef = powerBtn.gameObject.AddComponent<ChefButton>();
        powerChef.manager = mgr; powerChef.action = ChefButton.Action.Power;

        var spray = Ingredient(cook.transform, mgr, "spray", Chef + "spray_bottle.png", new Vector2(330, -90), new Vector2(122, 233), makerOpen.rectTransform);
        var pour = Ingredient(cook.transform, mgr, "pour", Chef + "bowl_pour.png", new Vector2(480, -200), new Vector2(185, 149), makerOpen.rectTransform);

        var steamSpawn = Empty("SteamSpawn", cook.transform);
        Place(steamSpawn, makerPos + new Vector2(0, 200), new Vector2(10, 10));

        mgr.cookPanel = cook;
        mgr.makerOpen = makerOpen.gameObject;
        mgr.makerClosed = makerClosed.gameObject;
        mgr.oilSheen = oil;
        mgr.batterFill = batter4;
        mgr.waffles4 = waffles4.gameObject;
        mgr.sprayBottle = spray;
        mgr.pourBowl = pour;
        mgr.powerButton = powerChef;
        mgr.powerGlow = glow;
        mgr.steamSpawn = (RectTransform)steamSpawn.transform;

        // ============ STEP 3 - DECORATE ============
        var decor = Empty("DecorPanel", root); Stretch(decor);
        var platePos = new Vector2(60, -20);

        var plate = Img("Plate", decor.transform, Load(Chef + "plate.png"));
        Place(plate, platePos, new Vector2(560, 413)); plate.raycastTarget = false;
        var wA = Img("WaffleA", decor.transform, Load(Chef + "waffle_single.png"));
        Place(wA, platePos + new Vector2(-35, 35), new Vector2(265, 265)); wA.raycastTarget = false;
        wA.rectTransform.localRotation = Quaternion.Euler(0, 0, 9f);
        var wB = Img("WaffleB", decor.transform, Load(Chef + "waffle_single.png"));
        Place(wB, platePos + new Vector2(40, -15), new Vector2(275, 275)); wB.raycastTarget = false;
        wB.rectTransform.localRotation = Quaternion.Euler(0, 0, -7f);

        var plateZone = Empty("PlateZone", decor.transform);
        Place(plateZone, platePos, new Vector2(470, 330));
        var decorLayer = Empty("DecorLayer", decor.transform); Stretch(decorLayer);

        // tray + sources grouped so the finale can hide them in one go
        var trayGroup = Empty("TrayGroup", decor.transform); Stretch(trayGroup);
        var tray = Img("Tray", trayGroup.transform, Load(Chef + "tray.png"));
        var trt = tray.rectTransform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0f); trt.pivot = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(940, 168); trt.anchoredPosition = new Vector2(0, 92);
        tray.raycastTarget = false;

        // topping sources sit ON the tray but live in the group's centre space
        var sources = new ChefToppingSource[6];
        string[] topSprites = { "strawberry", "banana_slice", "blueberry", "cream", "choc", "syrup" };
        Vector2[] topSizes = { new Vector2(74, 81), new Vector2(64, 64), new Vector2(54, 54),
                               new Vector2(76, 73), new Vector2(64, 57), new Vector2(120, 83) };
        for (int i = 0; i < 6; i++)
        {
            var s = Img("Topping_" + topSprites[i], trayGroup.transform, Load(Chef + topSprites[i] + ".png"));
            Place(s, new Vector2(-355 + i * 142, -268), topSizes[i]);
            var src = s.gameObject.AddComponent<ChefToppingSource>();
            src.manager = mgr;
            src.plateZone = (RectTransform)plateZone.transform;
            src.decorLayer = (RectTransform)decorLayer.transform;
            src.placedSize = topSizes[i] * (topSprites[i] == "syrup" ? 1.5f : 1.15f);
            sources[i] = src;
        }

        var doneBtn = Img("DoneButton", decor.transform, Load(Chef + "pill_gold.png"));
        Place(doneBtn, new Vector2(480, -268), new Vector2(220, 58));
        var doneChef = doneBtn.gameObject.AddComponent<ChefButton>();
        doneChef.manager = mgr; doneChef.action = ChefButton.Action.Done; doneChef.pulse = 0.05f;
        var doneTxt = Txt("Text", doneBtn.transform, "Done!", 32, Color.white);
        Place(doneTxt, new Vector2(0, 2), new Vector2(200, 50));
        AddShadow(doneTxt);

        mgr.decorPanel = decor;
        mgr.plateZone = (RectTransform)plateZone.transform;
        mgr.decorLayer = (RectTransform)decorLayer.transform;
        mgr.toppingSources = sources;
        mgr.doneButton = doneChef;
        mgr.trayGroup = trayGroup;

        // ============ FINALE ============
        var finale = Empty("FinaleGroup", root); Stretch(finale);

        var banner = Txt("Banner", finale.transform, "Yummy!", 96, new Color(1f, 0.81f, 0.31f));
        Place(banner, new Vector2(0, 190), new Vector2(700, 130));
        var bOut = banner.gameObject.AddComponent<Outline>();
        bOut.effectColor = Ink; bOut.effectDistance = new Vector2(3, -3);
        banner.gameObject.SetActive(false);

        var stars = new RectTransform[3];
        Vector2[] starPos = { new Vector2(-200, 70), new Vector2(0, 105), new Vector2(200, 70) };
        for (int i = 0; i < 3; i++)
        {
            var st = Img("Star" + i, finale.transform, Load("Assets/Art/shapematch/star_gold.png"));
            Place(st, starPos[i], new Vector2(i == 1 ? 130 : 105, i == 1 ? 130 : 105));
            st.raycastTarget = false;
            st.gameObject.SetActive(false);
            stars[i] = st.rectTransform;
        }

        var againBtn = Img("PlayAgainButton", finale.transform, Load(Home + "pill_blue.png"));
        Place(againBtn, new Vector2(-150, -260), new Vector2(260, 64));
        var againChef = againBtn.gameObject.AddComponent<ChefButton>();
        againChef.manager = mgr; againChef.action = ChefButton.Action.PlayAgain; againChef.pulse = 0.04f;
        var againTxt = Txt("Text", againBtn.transform, "Play Again", 32, Color.white);
        Place(againTxt, new Vector2(0, 2), new Vector2(240, 54));
        AddShadow(againTxt);

        var homePill = Img("HomePillButton", finale.transform, Load(Home + "pill_coral.png"));
        Place(homePill, new Vector2(150, -260), new Vector2(260, 64));
        var homePillChef = homePill.gameObject.AddComponent<ChefButton>();
        homePillChef.manager = mgr; homePillChef.action = ChefButton.Action.Home; homePillChef.pulse = 0.04f;
        var homeTxt = Txt("Text", homePill.transform, "Home", 32, Color.white);
        Place(homeTxt, new Vector2(0, 2), new Vector2(240, 54));
        AddShadow(homeTxt);

        mgr.finaleGroup = finale;
        mgr.banner = banner.gameObject;
        mgr.stars = stars;

        // ============ FX / DRAG LAYERS ============
        var fx = Empty("FxLayer", root); Stretch(fx);
        var confettiHost = new GameObject("Confetti");
        confettiHost.transform.SetParent(root, false);
        var confetti = confettiHost.AddComponent<ChefConfetti>();
        confetti.sprite = Load(Ui + "confetti.png");
        confetti.layer = (RectTransform)fx.transform;
        mgr.fxLayer = (RectTransform)fx.transform;
        mgr.confetti = confetti;
        mgr.steamSprite = Load(Chef + "steam.png");
        mgr.sprayPuffSprite = Load(Chef + "spray_puff.png");
        mgr.sparkleSprite = Load(Home + "sparkle.png");

        // ============ HUD ============
        var instr = Txt("Instruction", root, "", 40, Color.white);
        var irt = Place(instr, new Vector2(0, 0), new Vector2(900, 60));
        irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 1f);
        irt.anchoredPosition = new Vector2(0, -44);
        var iOut = instr.gameObject.AddComponent<Outline>();
        iOut.effectColor = Ink; iOut.effectDistance = new Vector2(2, -2);
        AddShadow(instr);
        mgr.instruction = instr;

        var mascotImg = Img("Mascot", root, Load(Chef + "mascot_neutral.png"));
        var mrt = mascotImg.rectTransform;
        mrt.anchorMin = mrt.anchorMax = new Vector2(1, 1); mrt.pivot = new Vector2(1, 1);
        mrt.sizeDelta = new Vector2(150, 150); mrt.anchoredPosition = new Vector2(-22, -18);
        mascotImg.raycastTarget = false;
        var mascot = mascotImg.gameObject.AddComponent<ChefMascot>();
        mascot.neutral = Load(Chef + "mascot_neutral.png");
        mascot.happy = Load(Chef + "mascot_happy.png");
        mascot.love = Load(Chef + "mascot_love.png");
        mgr.mascot = mascot;

        var homeBtn = Img("HomeButton", root, Load(Chef + "btn_round_pink.png"));
        var hrt = homeBtn.rectTransform;
        hrt.anchorMin = hrt.anchorMax = new Vector2(0, 1); hrt.pivot = new Vector2(0, 1);
        hrt.sizeDelta = new Vector2(88, 88); hrt.anchoredPosition = new Vector2(24, -24);
        var homeIcon = Img("Icon", homeBtn.transform, Load(Ui + "home_icon.png"));
        Place(homeIcon, new Vector2(0, 2), new Vector2(46, 46));
        homeIcon.raycastTarget = false;
        var homeChef = homeBtn.gameObject.AddComponent<ChefButton>();
        homeChef.manager = mgr; homeChef.action = ChefButton.Action.Home;

        // hint arrow + drag layer on top
        var hint = Empty("HintLayer", root); Stretch(hint);
        var arrowImg = Img("Arrow", hint.transform, Load(Chef + "arrow.png"));
        Place(arrowImg, Vector2.zero, new Vector2(130, 80));
        var arrow = arrowImg.gameObject.AddComponent<ChefArrowHint>();
        mgr.arrow = arrow;

        var drag = Empty("DragLayer", root); Stretch(drag);
        mgr.dragLayer = (RectTransform)drag.transform;

        // ============ AUDIO CLIPS ============
        mgr.pourClip = Aud("pour");
        mgr.crackClip = Aud("crack");
        mgr.sprinkleClip = Aud("sprinkle");
        mgr.plopClip = Aud("plop");
        mgr.whiskClip = Aud("whisk");
        mgr.sprayClip = Aud("spray");
        mgr.sizzleClip = Aud("sizzle");
        mgr.dingClip = Aud("ding");
        mgr.chimeClip = Aud("chime");
        mgr.fanfareClip = Aud("fanfare");
        mgr.popClip = Aud("pop");

        // ============ SAVE ============
        EditorSceneManager.MarkSceneDirty(scene);
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[KidsChefBuilder] built + saved " + ScenePath);
    }

    static ChefIngredient Ingredient(Transform parent, ChefGameManager mgr, string id, string spritePath,
                                     Vector2 pos, Vector2 size, RectTransform target)
    {
        var img = Img("Ingredient_" + id, parent, Load(spritePath));
        Place(img, pos, size);
        img.gameObject.AddComponent<CanvasGroup>();
        var ci = img.gameObject.AddComponent<ChefIngredient>();
        ci.manager = mgr; ci.id = id; ci.target = target;
        return ci;
    }

    static void AddShadow(Text t)
    {
        var sh = t.gameObject.AddComponent<Shadow>();
        sh.effectColor = new Color(Ink.r, Ink.g, Ink.b, 0.45f);
        sh.effectDistance = new Vector2(0, -2);
    }

    // ---- shared helpers (same idioms as the other builders) ----
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

    static Text Txt(string name, Transform parent, string text, int size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = text; t.fontSize = size; t.color = color; t.alignment = TextAnchor.MiddleCenter;
        t.font = _fredoka != null ? _fredoka : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

    static void StretchTo(Image img) => Stretch(img.gameObject);

    static RectTransform Place(Image img, Vector2 pos, Vector2 size) => Place(img.gameObject, pos, size);
    static RectTransform Place(Text t, Vector2 pos, Vector2 size) => Place(t.gameObject, pos, size);
    static RectTransform Place(GameObject go, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size; rt.anchoredPosition = pos; rt.localScale = Vector3.one;
        return rt;
    }
}
