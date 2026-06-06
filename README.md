# Count the Fruits 🍎

A polished, kid-friendly **tap-to-count** mini-game made in Unity. Count the apples
scattered on screen, tap the button with the matching number, and clear five rounds of
rising counts to earn a full row of stars.

> ### Built entirely by [LoomTide](https://github.com/Loomtide)
> Every part of this game was produced automatically by **LoomTide** — the game design,
> the C# gameplay code, and **all of the art and audio**. The soft rounded-vector sprites,
> the mascot, the UI, the sound effects, and the background music are **procedurally
> generated** (see [`Tools/`](Tools/)) — nothing is hand-drawn, stock, or licensed.

## Screenshots

| Start | Gameplay | Results |
|---|---|---|
| ![Start screen](Screenshots/start.png) | ![Gameplay](Screenshots/gameplay.png) | ![Results](Screenshots/reward.png) |

## How to play

- Press **Play!** on the start screen.
- Each round scatters **N** apples and asks *"How many apples?"*
- Tap the answer button showing the matching number:
  - ✅ **Correct** → confetti, a happy chime, and a star.
  - ❌ **Wrong** → a gentle shake and buzz — just try again (there's no lose state).
- Difficulty ramps **3 → 4 → 5 → 6 → 7** apples across five rounds, then a **Great job!**
  results screen with **Play again**.
- The 🏠 **Home** button returns to the start screen at any time.

## Running it

Open the project in **Unity 6000.3 LTS** (`6000.3.9f1`), open
`Assets/Scenes/CountTheFruits.unity`, and press **Play**. It works with the mouse on
desktop and touch on device (input is driven through Unity's Input System).

## How the assets are generated

All art and audio are created by small Python scripts (PIL · NumPy · fontTools) under
[`Tools/`](Tools/). Re-run any of them to regenerate the matching assets:

| Script | Generates |
|---|---|
| `gen_bg.py` | sky, sun, rolling hills, ground, and clouds |
| `gen_fruit.py` | the apples |
| `gen_hud.py` | white cards, basket, stars, and the mascot |
| `gen_buttons.py` | the answer / action buttons |
| `gen_fx.py` | sparkle / confetti / "counted" check effects |
| `gen_music.py` | the background music loop, fanfare, and Play jingle |

## Project layout

- `Assets/Scenes/CountTheFruits.unity` — the game scene
- `Assets/Scripts/` — `Core` (game loop, spawner, count, input, audio) · `Interactables`
  (fruit) · `UI` (HUD, answer buttons, start & end screens) · `FX`
- `Assets/Art/{bg,fruit,fx,hud,ui}` — generated sprites
- `Assets/Audio/` — generated music + sound effects
- `Assets/Fonts/` — Fredoka (display) + Nunito (UI)
- `Tools/` — the asset generators

## Requirements

- Unity **6000.3 LTS**
- Packages: 2D Sprite, 2D Pixel Perfect, Input System, uGUI, Animation, Audio, Physics2D
