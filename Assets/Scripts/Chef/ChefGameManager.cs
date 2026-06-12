using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KidsChef
{
    /// <summary>
    /// Orchestrates the three-step waffle recipe: drag ingredients into the bowl, stir,
    /// oil + pour + cook in the waffle maker, then decorate and celebrate. No fail states —
    /// every sub-step simply waits for its one gesture. Phase gates which gestures are
    /// accepted; visuals advance through coroutine-driven fades and pops.
    /// </summary>
    public class ChefGameManager : MonoBehaviour
    {
        public enum Phase { Ingredients, Stir, Spray, Pour, Power, Cooking, Decorate, Finale }
        public Phase phase = Phase.Ingredients;

        [Header("Panels / backgrounds")]
        public GameObject mixPanel;
        public GameObject cookPanel;
        public GameObject decorPanel;
        public GameObject finaleGroup;
        public GameObject woodBg;
        public GameObject kitchenBg;

        [Header("Step 1 - mix")]
        public ChefIngredient[] ingredients;       // milk, egg, sugar, honey, banana
        public GameObject[] contents;              // bowl content overlay per ingredient (same order)
        public CanvasGroup rawContents;            // parent group of those overlays
        public Image batterLumpy;
        public Image batterSmooth;
        public ChefMixZone mixZone;
        public RectTransform whisk;
        public float stirRequired = 2600f;

        [Header("Step 2 - cook")]
        public GameObject makerOpen;
        public GameObject makerClosed;
        public Image oilSheen;
        public Image batterFill;
        public GameObject waffles4;
        public ChefIngredient sprayBottle;
        public ChefIngredient pourBowl;
        public ChefButton powerButton;
        public Image powerGlow;
        public RectTransform steamSpawn;

        [Header("Step 3 - decorate")]
        public RectTransform plateZone;
        public RectTransform decorLayer;
        public ChefToppingSource[] toppingSources;
        public ChefButton doneButton;
        public GameObject trayGroup;
        public int maxToppings = 12;

        [Header("HUD / FX")]
        public Text instruction;
        public ChefMascot mascot;
        public ChefArrowHint arrow;
        public RectTransform fxLayer;
        public RectTransform dragLayer;
        public Sprite steamSprite;
        public Sprite sprayPuffSprite;
        public Sprite sparkleSprite;
        public ChefConfetti confetti;

        [Header("Finale")]
        public GameObject banner;
        public RectTransform[] stars;

        [Header("Audio")]
        public AudioSource sfx;
        public AudioClip pourClip, crackClip, sprinkleClip, plopClip, whiskClip,
                         sprayClip, sizzleClip, dingClip, chimeClip, fanfareClip, popClip;

        [Header("Debug (editor verification)")]
        public bool debugAdvance;

        int _ingredientsLeft;
        float _stir;
        float _lastWhiskSfx;
        int _placedToppings;

        void Start()
        {
            _ingredientsLeft = ingredients.Length;
            mixPanel.SetActive(true);
            cookPanel.SetActive(false);
            decorPanel.SetActive(false);
            finaleGroup.SetActive(false);
            woodBg.SetActive(true);
            kitchenBg.SetActive(false);
            foreach (var c in contents) c.SetActive(false);
            SetAlpha(batterLumpy, 0f); SetAlpha(batterSmooth, 0f);
            whisk.gameObject.SetActive(false);
            makerClosed.SetActive(false);
            SetAlpha(oilSheen, 0f); SetAlpha(batterFill, 0f);
            waffles4.SetActive(false);
            pourBowl.gameObject.SetActive(false);
            powerGlow.gameObject.SetActive(false);
            doneButton.gameObject.SetActive(false);
            instruction.text = "Drag everything into the bowl!";
            PointArrowAtNextIngredient();
        }

        void Update()
        {
            if (debugAdvance) { debugAdvance = false; DebugAdvance(); }
        }

        // ------------------------------------------------ gestures coming in
        /// <summary>An ingredient was dropped on its target. Returns true if accepted (it consumes).</summary>
        public bool TryConsume(ChefIngredient ing)
        {
            switch (ing.id)
            {
                case "spray":
                    if (phase != Phase.Spray) return false;
                    StartCoroutine(SprayApplied());
                    return true;
                case "pour":
                    if (phase != Phase.Pour) return false;
                    StartCoroutine(PourApplied());
                    return true;
                default:
                    if (phase != Phase.Ingredients) return false;
                    return ConsumeMixIngredient(ing);
            }
        }

        bool ConsumeMixIngredient(ChefIngredient ing)
        {
            int i = System.Array.IndexOf(ingredients, ing);
            if (i < 0 || ing.consumed) return false;
            ing.consumed = true; // mark now so the arrow hint skips this one
            contents[i].SetActive(true);
            StartCoroutine(PopRect((RectTransform)contents[i].transform));
            Play(ClipFor(ing.id));
            mascot.Happy(1.2f);
            _ingredientsLeft--;
            if (_ingredientsLeft <= 0) StartCoroutine(BeginStir());
            else PointArrowAtNextIngredient();
            return true;
        }

        AudioClip ClipFor(string id)
        {
            switch (id)
            {
                case "milk": return pourClip;
                case "egg": return crackClip;
                case "sugar": return sprinkleClip;
                case "honey": return pourClip;
                case "banana": return plopClip;
                default: return popClip;
            }
        }

        IEnumerator BeginStir()
        {
            arrow.Hide();
            yield return new WaitForSeconds(0.35f);
            phase = Phase.Stir;
            instruction.text = "Mix, mix, mix!";
            whisk.gameObject.SetActive(true);
            yield return PopRect(whisk);
            mixZone.active = true;
        }

        /// <summary>Stir progress from the mix zone, in canvas pixels of pointer travel.</summary>
        public void OnStir(float delta)
        {
            if (phase != Phase.Stir) return;
            _stir += delta;
            float p = Mathf.Clamp01(_stir / stirRequired);
            rawContents.alpha = 1f - Mathf.Clamp01(p * 2f);
            SetAlpha(batterLumpy, p < 0.5f ? p * 2f : 2f - p * 2f);
            SetAlpha(batterSmooth, Mathf.Clamp01((p - 0.5f) * 2f));
            if (Time.unscaledTime - _lastWhiskSfx > 0.5f) { _lastWhiskSfx = Time.unscaledTime; Play(whiskClip); }
            if (p >= 1f) StartCoroutine(StirDone());
        }

        IEnumerator StirDone()
        {
            phase = Phase.Cooking; // gate further stirring while we transition
            mixZone.active = false;
            Play(chimeClip);
            mascot.Love(1.6f);
            SparkleBurst(((RectTransform)mixZone.transform).anchoredPosition, 5);
            yield return new WaitForSeconds(1.1f);
            whisk.gameObject.SetActive(false);
            mixPanel.SetActive(false);
            woodBg.SetActive(false);
            kitchenBg.SetActive(true);
            cookPanel.SetActive(true);
            phase = Phase.Spray;
            instruction.text = "Spray the oil!";
            ArrowBetween(sprayBottle.Rect, MakerRect, 0.42f);
        }

        IEnumerator SprayApplied()
        {
            phase = Phase.Cooking;
            arrow.Hide();
            Play(sprayClip);
            var plate = MakerPlateCenter;
            for (int i = 0; i < 3; i++)
                SpawnPuff(sprayPuffSprite, plate + new Vector2(Random.Range(-70f, 70f), Random.Range(-30f, 30f)),
                          55f, 110f, 0.9f);
            yield return Fade(oilSheen, 0f, 1f, 0.6f);
            pourBowl.gameObject.SetActive(true);
            yield return PopRect(pourBowl.Rect);
            phase = Phase.Pour;
            instruction.text = "Pour the batter!";
            ArrowBetween(pourBowl.Rect, MakerRect, 0.42f);
        }

        IEnumerator PourApplied()
        {
            phase = Phase.Cooking;
            arrow.Hide();
            Play(pourClip);
            yield return Fade(batterFill, 0f, 1f, 0.8f);
            phase = Phase.Power;
            instruction.text = "Press the button!";
            powerGlow.gameObject.SetActive(true);
            powerButton.pulse = 0.10f;
            mascot.Happy(1.2f);
        }

        public void PowerPressed()
        {
            if (phase != Phase.Power) return;
            StartCoroutine(Cook());
        }

        IEnumerator Cook()
        {
            phase = Phase.Cooking;
            powerGlow.gameObject.SetActive(false);
            powerButton.pulse = 0f;
            Play(popClip);
            yield return new WaitForSeconds(0.25f);
            makerOpen.SetActive(false);
            makerClosed.SetActive(true);
            yield return PopRect((RectTransform)makerClosed.transform);
            instruction.text = "Cooking...";
            Play(sizzleClip);
            float t = 0f;
            while (t < 3.0f)
            {
                SpawnPuff(steamSprite, steamSpawn.anchoredPosition + new Vector2(Random.Range(-120f, 120f), 0f),
                          120f, 90f, 1.5f);
                yield return new WaitForSeconds(0.45f);
                t += 0.45f;
            }
            Play(dingClip);
            makerClosed.SetActive(false);
            makerOpen.SetActive(true);
            SetAlpha(oilSheen, 0f); SetAlpha(batterFill, 0f);
            waffles4.SetActive(true);
            SparkleBurst(MakerPlateCenter, 6);
            mascot.Love(2f);
            instruction.text = "Ding! They're ready!";
            yield return new WaitForSeconds(1.6f);
            cookPanel.SetActive(false);
            kitchenBg.SetActive(false);
            woodBg.SetActive(true);
            decorPanel.SetActive(true);
            phase = Phase.Decorate;
            instruction.text = "Add yummy toppings!";
            ArrowBetween((RectTransform)toppingSources[0].transform, plateZone, 0.42f);
        }

        public bool CanPlaceTopping => phase == Phase.Decorate && _placedToppings < maxToppings;

        public void ToppingPlaced()
        {
            arrow.Hide();
            _placedToppings++;
            Play(plopClip);
            mascot.Happy(0.9f);
            if (_placedToppings == 1) doneButton.gameObject.SetActive(true);
        }

        public void DonePressed()
        {
            if (phase != Phase.Decorate) return;
            StartCoroutine(Finale());
        }

        IEnumerator Finale()
        {
            phase = Phase.Finale;
            doneButton.gameObject.SetActive(false);
            if (trayGroup != null) trayGroup.SetActive(false);
            instruction.text = "";
            Play(fanfareClip);
            mascot.Love(60f);
            finaleGroup.SetActive(true);
            banner.SetActive(true);
            StartCoroutine(PopRect((RectTransform)banner.transform));
            if (confetti != null) confetti.Burst(40);
            foreach (var s in stars)
            {
                s.gameObject.SetActive(true);
                StartCoroutine(PopRect(s));
                Play(popClip);
                yield return new WaitForSeconds(0.28f);
            }
        }

        public void PlayAgain()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("KidsChef");
        }

        public void GoHome()
        {
            // In the Kids Adventure package, Home returns to the hub; standalone it restarts.
            if (Application.CanStreamedLevelBeLoaded("KidsAdventure"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("KidsAdventure");
            else
                PlayAgain();
        }

        // ------------------------------------------------ debug hook (bridge verification)
        void DebugAdvance()
        {
            switch (phase)
            {
                case Phase.Ingredients:
                    foreach (var ing in ingredients)
                        if (!ing.consumed) { ing.ForceConsume(); break; }
                    break;
                case Phase.Stir: OnStir(stirRequired); break;
                case Phase.Spray: sprayBottle.ForceConsume(); break;
                case Phase.Pour: pourBowl.ForceConsume(); break;
                case Phase.Power: PowerPressed(); break;
                case Phase.Decorate:
                    var src = toppingSources[_placedToppings % toppingSources.Length];
                    src.DebugPlace(new Vector2(Random.Range(-110f, 110f), Random.Range(-60f, 60f)));
                    break;
            }
        }

        // ------------------------------------------------ helpers
        RectTransform MakerRect => (RectTransform)makerOpen.transform;
        Vector2 MakerPlateCenter
        {
            get
            {
                var r = MakerRect;
                // base plate sits in the lower 40% of the open maker sprite
                return r.anchoredPosition + new Vector2(0f, -r.sizeDelta.y * 0.22f);
            }
        }

        void PointArrowAtNextIngredient()
        {
            foreach (var ing in ingredients)
                if (!ing.consumed) { ArrowBetween(ing.Rect, ingredients[0].target, 0.45f); return; }
        }

        void ArrowBetween(RectTransform from, RectTransform to, float lerp)
        {
            arrow.PointBetween(AnchoredOf(from), AnchoredOf(to), lerp);
        }

        static Vector2 AnchoredOf(RectTransform rt)
        {
            // Panels and layers are full-stretch with centered pivots, so anchoredPosition
            // of center-anchored children shares one coordinate space across the canvas.
            return rt.anchoredPosition;
        }

        void SpawnPuff(Sprite sprite, Vector2 pos, float size, float rise, float life)
        {
            var go = new GameObject("Puff", typeof(RectTransform));
            go.transform.SetParent(fxLayer, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite; img.raycastTarget = false;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = pos;
            var puff = go.AddComponent<ChefPuff>();
            puff.rise = rise; puff.life = life;
        }

        public void SparkleBurst(Vector2 center, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var pos = center + new Vector2(Random.Range(-140f, 140f), Random.Range(-80f, 100f));
                SpawnPuff(sparkleSprite, pos, Random.Range(34f, 58f), 40f, Random.Range(0.7f, 1.2f));
            }
        }

        void Play(AudioClip clip)
        {
            if (clip != null && sfx != null) sfx.PlayOneShot(clip);
        }

        static void SetAlpha(Image img, float a)
        {
            var c = img.color; c.a = a; img.color = c;
        }

        IEnumerator Fade(Image img, float from, float to, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                SetAlpha(img, Mathf.Lerp(from, to, t / dur));
                yield return null;
            }
            SetAlpha(img, to);
        }

        public static IEnumerator PopRect(RectTransform rt)
        {
            Vector3 baseScale = Vector3.one;
            float t = 0f, dur = 0.30f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float x = Mathf.Clamp01(t / dur);
                const float c1 = 1.7f, c3 = c1 + 1f;
                float e = 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
                if (rt == null) yield break;
                rt.localScale = baseScale * Mathf.Max(0.05f, e);
                yield return null;
            }
            if (rt != null) rt.localScale = baseScale;
        }
    }
}
