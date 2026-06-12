#!/usr/bin/env python3
"""Generate Kids Chef (waffle-making) sprites to Assets/Art/chef/.
Same soft rounded candy-vector identity as the rest of the package (Fredoka era):
supersampled Pillow drawing, translucent details via blend() (ImageDraw on RGBA
REPLACES pixels), capsule radii capped at half the box height."""
import os, math, random
from PIL import Image, ImageDraw, ImageFilter

ROOT = os.path.join(os.path.dirname(__file__), "..")
OUT = os.path.join(ROOT, "Assets", "Art", "chef")
os.makedirs(OUT, exist_ok=True)
SS = 2  # supersample

def hx(h):
    h = h.lstrip('#')
    return tuple(int(h[i:i+2], 16) for i in (0, 2, 4))

INK = hx('46365e')
CORAL = hx('ff5d72'); ORANGE = hx('ff9d3f'); GOLD = hx('ffce4f')
GREEN = hx('5fd68a'); BLUE = hx('4fb4ff'); PINK = hx('ff7ec2')
WOOD = hx('e89a52'); WOOD_SEAM = hx('c97c3a'); WOOD_GRAIN = hx('dd8c4a')
WALL = hx('f4eede'); LEAF = hx('e2e4cb')
MAKER = hx('b8dcf5'); MAKER_DK = hx('8fc2e8'); PLATE = hx('2a2f42'); MOLD = hx('1d2233')
TEAL = hx('45c8e0'); TEAL_DK = hx('2ba8c4')
BATTER = hx('f5d98c'); BATTER_DK = hx('e3bd62')
WAFFLE = hx('f0c46a'); WAFFLE_DK = hx('d9a64a'); WAFFLE_CRUST = hx('c98f3a')
FUR = hx('e8b87a'); FUR_DK = hx('d6a162'); MUZZLE = hx('f9e3bf')

def save(img, name, final=None):
    if final:
        img = img.resize(final, Image.LANCZOS)
    img.save(os.path.join(OUT, name))
    print("wrote", name, img.size)

def blend(img, draw_fn):
    """Draw semi-transparent details on a temp layer and alpha-composite them."""
    layer = Image.new('RGBA', img.size, (0, 0, 0, 0))
    draw_fn(ImageDraw.Draw(layer))
    img.alpha_composite(layer)

def canvas(w, h):
    img = Image.new('RGBA', (w * SS, h * SS), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img), w * SS, h * SS

def capsule(d, box, **kw):
    # radius must be <= half the box height or Pillow degenerates the shape
    r = int(min(box[3] - box[1], box[2] - box[0]) / 2)
    d.rounded_rectangle(box, radius=r, **kw)

# ---------------------------------------------------------------- backgrounds
def draw_wood(d, box, seed=3):
    x0, y0, x1, y1 = box
    d.rectangle(box, fill=(*WOOD, 255))
    rng = random.Random(seed)
    nplank = 7
    ph = (y1 - y0) / nplank
    for i in range(nplank):
        py = y0 + i * ph
        t = rng.randint(-12, 12)
        col = tuple(min(255, max(0, c + t)) for c in WOOD)
        d.rectangle([x0, py, x1, py + ph], fill=(*col, 255))
        d.line([(x0, py), (x1, py)], fill=(*WOOD_SEAM, 255), width=2 * SS)
        for g in range(3):
            gy = py + rng.uniform(0.18, 0.85) * ph
            amp = rng.uniform(1.5, 3.5) * SS
            pts = [(x, gy + math.sin(x / (70.0 * SS) + g * 2.1 + i) * amp)
                   for x in range(int(x0), int(x1) + 8 * SS, 8 * SS)]
            d.line(pts, fill=(*WOOD_GRAIN, 255), width=SS)

def gen_wood():
    img, d, W, H = canvas(1280, 720)
    draw_wood(d, [0, 0, W, H])
    # soft light falloff top-left
    blend(img, lambda ld: ld.ellipse([-W * 0.3, -H * 0.5, W * 0.8, H * 0.5], fill=(255, 255, 255, 26)))
    save(img, 'wood.png', (1280, 720))

def gen_kitchen():
    img, d, W, H = canvas(1280, 720)
    wall_h = int(H * 0.46)
    d.rectangle([0, 0, W, wall_h], fill=(*WALL, 255))
    # bamboo-ish leaves on the wall
    rng = random.Random(7)
    for _ in range(26):
        cx, cy = rng.uniform(0, W), rng.uniform(0, wall_h)
        ang = rng.uniform(0, 360)
        lw, lh = rng.uniform(34, 90) * SS / 2, rng.uniform(9, 16) * SS / 2
        leaf = Image.new('RGBA', (int(lw * 2.4), int(lh * 2.4)), (0, 0, 0, 0))
        ImageDraw.Draw(leaf).ellipse([lw * 0.2, lh * 0.7, lw * 2.2, lh * 1.7], fill=(*LEAF, 255))
        leaf = leaf.rotate(ang, expand=True, resample=Image.BICUBIC)
        img.alpha_composite(leaf, (int(cx), int(cy)))
    # counter edge highlight then wood
    draw_wood(d, [0, wall_h, W, H], seed=5)
    d.rectangle([0, wall_h, W, wall_h + 7 * SS], fill=(*hx('f7b97a'), 255))
    blend(img, lambda ld: ld.rectangle([0, wall_h - 16 * SS, W, wall_h], fill=(*INK, 28)))
    save(img, 'kitchen.png', (1280, 720))

# ---------------------------------------------------------------- mascot (chef bear)
def draw_heart(d, cx, cy, s, color):
    d.ellipse([cx - s, cy - s * 0.9, cx, cy + s * 0.1], fill=color)
    d.ellipse([cx, cy - s * 0.9, cx + s, cy + s * 0.1], fill=color)
    d.polygon([(cx - s * 0.96, cy - s * 0.18), (cx + s * 0.96, cy - s * 0.18), (cx, cy + s)], fill=color)

def gen_mascot(name, mood):
    img, d, W, H = canvas(360, 360)
    c = W / 2
    # portrait disc: pink bg + white ring
    d.ellipse([6 * SS, 6 * SS, W - 6 * SS, H - 6 * SS], fill=(*hx('ffd9ec'), 255))
    d.ellipse([6 * SS, 6 * SS, W - 6 * SS, H - 6 * SS], outline=(255, 255, 255, 255), width=10 * SS)
    # ears
    for sx in (-1, 1):
        ex = c + sx * W * 0.27
        d.ellipse([ex - W * 0.095, H * 0.30 - W * 0.095, ex + W * 0.095, H * 0.30 + W * 0.095], fill=(*FUR, 255))
        d.ellipse([ex - W * 0.05, H * 0.30 - W * 0.05, ex + W * 0.05, H * 0.30 + W * 0.05], fill=(*MUZZLE, 255))
    # head
    d.ellipse([c - W * 0.30, H * 0.26, c + W * 0.30, H * 0.82], fill=(*FUR, 255))
    # muzzle
    d.ellipse([c - W * 0.155, H * 0.55, c + W * 0.155, H * 0.78], fill=(*MUZZLE, 255))
    d.ellipse([c - W * 0.045, H * 0.585, c + W * 0.045, H * 0.645], fill=(*INK, 255))  # nose
    # chef hat: band + puffs
    d.rounded_rectangle([c - W * 0.225, H * 0.175, c + W * 0.225, H * 0.305], radius=int(W * 0.045),
                        fill=(255, 255, 255, 255))
    for (px, py, pr) in [(-0.16, 0.115, 0.115), (0.0, 0.075, 0.135), (0.16, 0.115, 0.115)]:
        d.ellipse([c + px * W - pr * W, py * H - pr * W + H * 0.04,
                   c + px * W + pr * W, py * H + pr * W + H * 0.04], fill=(255, 255, 255, 255))
    blend(img, lambda ld: ld.rounded_rectangle([c - W * 0.225, H * 0.27, c + W * 0.225, H * 0.305],
                                               radius=int(W * 0.017), fill=(*INK, 36)))
    # eyes + mouth by mood
    ey = H * 0.50
    if mood == 'neutral':
        for sx in (-1, 1):
            d.ellipse([c + sx * W * 0.115 - W * 0.032, ey - W * 0.032,
                       c + sx * W * 0.115 + W * 0.032, ey + W * 0.032], fill=(*INK, 255))
        d.arc([c - W * 0.075, H * 0.655, c + W * 0.075, H * 0.745], 20, 160, fill=(*INK, 255), width=5 * SS)
    elif mood == 'happy':
        for sx in (-1, 1):
            d.arc([c + sx * W * 0.115 - W * 0.05, ey - W * 0.05,
                   c + sx * W * 0.115 + W * 0.05, ey + W * 0.05], 190, 350, fill=(*INK, 255), width=6 * SS)
        d.chord([c - W * 0.085, H * 0.64, c + W * 0.085, H * 0.76], 10, 170, fill=(*INK, 255))
        d.chord([c - W * 0.05, H * 0.70, c + W * 0.05, H * 0.765], 10, 170, fill=(*CORAL, 255))
    else:  # love
        for sx in (-1, 1):
            draw_heart(d, c + sx * W * 0.115, ey, W * 0.052, (*CORAL, 255))
        d.chord([c - W * 0.085, H * 0.64, c + W * 0.085, H * 0.76], 10, 170, fill=(*INK, 255))
        draw_heart(d, c + W * 0.30, H * 0.36, W * 0.04, (*PINK, 255))
        draw_heart(d, c - W * 0.31, H * 0.45, W * 0.030, (*PINK, 255))
    # blush
    blend(img, lambda ld: [ld.ellipse([c + sx * W * 0.20 - W * 0.045, H * 0.565 - W * 0.028,
                                       c + sx * W * 0.20 + W * 0.045, H * 0.565 + W * 0.028],
                                      fill=(*CORAL, 80)) for sx in (-1, 1)])
    save(img, name, (360, 360))

# ---------------------------------------------------------------- mixing bowl + contents
def gen_bowl_top(s=640):
    img, d, W, H = canvas(s, s)
    m = 8 * SS
    d.ellipse([m, m + 6 * SS, W - m, H - m], fill=(*TEAL_DK, 255))
    d.ellipse([m, m, W - m, H - m - 6 * SS], fill=(*TEAL, 255))
    # interior (white) — content discs sit on top of this
    iw = W * 0.118
    d.ellipse([iw, iw, W - iw, H - iw], fill=(*hx('2d9fb8'), 255))      # inner shadow ring
    d.ellipse([iw + 8 * SS, iw + 10 * SS, W - iw - 8 * SS, H - iw - 4 * SS], fill=(*hx('f6f2ea'), 255))
    # rim gloss
    blend(img, lambda ld: ld.arc([m + 14 * SS, m + 10 * SS, W - m - 14 * SS, H - m - 18 * SS],
                                 200, 320, fill=(255, 255, 255, 130), width=12 * SS))
    save(img, 'bowl_top.png', (s, s))

def content_canvas():
    return canvas(520, 520)

def gen_milk_fill():
    img, d, W, H = content_canvas()
    d.ellipse([W * 0.04, H * 0.04, W * 0.96, H * 0.96], fill=(*hx('ffffff'), 255))
    blend(img, lambda ld: ld.ellipse([W * 0.12, H * 0.10, W * 0.62, H * 0.45], fill=(*hx('fdf6e3'), 150)))
    rng = random.Random(2)
    for _ in range(14):  # rim bubbles
        a = rng.uniform(0, 2 * math.pi); r = W * rng.uniform(0.40, 0.45); br = rng.uniform(3, 7) * SS
        x, y = W / 2 + r * math.cos(a), H / 2 + r * math.sin(a)
        d.ellipse([x - br, y - br, x + br, y + br], outline=(*hx('e8e0d0'), 255), width=SS)
    save(img, 'milk_fill.png', (520, 520))

def gen_egg_yolk():
    img, d, W, H = content_canvas()
    blend(img, lambda ld: ld.ellipse([W * 0.28, H * 0.26, W * 0.78, H * 0.66], fill=(255, 252, 240, 150)))
    d.ellipse([W * 0.42, H * 0.36, W * 0.62, H * 0.56], fill=(*hx('ffc24a'), 255))
    d.ellipse([W * 0.455, H * 0.385, W * 0.525, H * 0.45], fill=(*hx('ffe09a'), 255))
    save(img, 'egg_yolk.png', (520, 520))

def gen_sugar_pile():
    img, d, W, H = content_canvas()
    d.ellipse([W * 0.30, H * 0.52, W * 0.62, H * 0.74], fill=(*hx('fdfdfb'), 255))
    d.ellipse([W * 0.36, H * 0.47, W * 0.56, H * 0.62], fill=(*hx('ffffff'), 255))
    rng = random.Random(4)
    for _ in range(40):
        x = rng.uniform(W * 0.30, W * 0.62); y = rng.uniform(H * 0.48, H * 0.72)
        d.ellipse([x, y, x + 2.5 * SS, y + 2.5 * SS], fill=(*hx('dcd6c8'), 255))
    save(img, 'sugar_pile.png', (520, 520))

def gen_honey_blob():
    img, d, W, H = content_canvas()
    d.ellipse([W * 0.52, H * 0.24, W * 0.80, H * 0.50], fill=(*hx('c46a1f'), 255))
    d.ellipse([W * 0.60, H * 0.40, W * 0.74, H * 0.56], fill=(*hx('c46a1f'), 255))
    blend(img, lambda ld: ld.ellipse([W * 0.57, H * 0.28, W * 0.67, H * 0.36], fill=(255, 240, 200, 150)))
    save(img, 'honey_blob.png', (520, 520))

def banana_slice_at(d, x, y, r):
    d.ellipse([x - r, y - r, x + r, y + r], fill=(*hx('ffe48a'), 255))
    d.ellipse([x - r * 0.78, y - r * 0.78, x + r * 0.78, y + r * 0.78], fill=(*hx('fff3c4'), 255))
    for k in range(5):
        a = k * 2 * math.pi / 5 + 0.5
        d.line([(x, y), (x + r * 0.34 * math.cos(a), y + r * 0.34 * math.sin(a))],
               fill=(*hx('e9c96a'), 255), width=SS)

def gen_banana_bits():
    img, d, W, H = content_canvas()
    rng = random.Random(6)
    for (fx, fy) in [(0.26, 0.30), (0.40, 0.20), (0.20, 0.52), (0.36, 0.66), (0.55, 0.72), (0.70, 0.62)]:
        banana_slice_at(d, W * fx, H * fy, rng.uniform(0.055, 0.07) * W)
    save(img, 'banana_bits.png', (520, 520))

def gen_batter(name, smooth):
    img, d, W, H = content_canvas()
    d.ellipse([W * 0.04, H * 0.04, W * 0.96, H * 0.96], fill=(*BATTER, 255))
    if not smooth:
        rng = random.Random(9)
        for _ in range(12):  # lumps
            x = rng.uniform(W * 0.18, W * 0.74); y = rng.uniform(H * 0.18, H * 0.74); r = rng.uniform(0.04, 0.09) * W
            d.ellipse([x, y, x + r, y + r], fill=(*hx('efcf7e'), 255))
            d.ellipse([x + r * 0.2, y + r * 0.2, x + r * 0.7, y + r * 0.7], fill=(*hx('fbe7ae'), 255))
    else:
        # smooth swirl
        cx, cy = W / 2, H / 2
        pts = []
        for t in range(0, 660, 6):
            a = math.radians(t)
            r = W * 0.40 * (t / 660.0)
            pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
        d.line(pts, fill=(*BATTER_DK, 255), width=int(W * 0.045), joint='curve')
        blend(img, lambda ld: ld.line([(p[0], p[1] - W * 0.012) for p in pts],
                                      fill=(255, 250, 230, 90), width=int(W * 0.015), joint='curve'))
    blend(img, lambda ld: ld.ellipse([W * 0.14, H * 0.10, W * 0.55, H * 0.36], fill=(255, 250, 235, 70)))
    save(img, name, (520, 520))

# ---------------------------------------------------------------- step-1 ingredients
def gen_milk_jug(w=260, h=300):
    img, d, W, H = canvas(w, h)
    body = [W * 0.16, H * 0.18, W * 0.78, H * 0.95]
    d.rounded_rectangle(body, radius=int(W * 0.14), fill=(*hx('eef3f7'), 255),
                        outline=(*hx('c3cfdb'), 255), width=4 * SS)
    # milk inside + top
    d.rounded_rectangle([W * 0.20, H * 0.42, W * 0.74, H * 0.91], radius=int(W * 0.11), fill=(*hx('ffffff'), 255))
    d.ellipse([W * 0.20, H * 0.16, W * 0.74, H * 0.34], fill=(*hx('ffffff'), 255), outline=(*hx('c3cfdb'), 255), width=3 * SS)
    # spout
    d.polygon([(W * 0.16, H * 0.24), (W * 0.04, H * 0.165), (W * 0.20, H * 0.155)], fill=(*hx('ffffff'), 255))
    # handle
    d.arc([W * 0.66, H * 0.30, W * 0.99, H * 0.66], 280, 100, fill=(*hx('c3cfdb'), 255), width=9 * SS)
    blend(img, lambda ld: ld.rounded_rectangle([W * 0.23, H * 0.26, W * 0.34, H * 0.82],
                                               radius=int(W * 0.05), fill=(255, 255, 255, 120)))
    save(img, 'milk_jug.png', (w, h))

def gen_egg(w=160, h=200):
    img, d, W, H = canvas(w, h)
    d.ellipse([W * 0.10, H * 0.06, W * 0.90, H * 0.97], fill=(*hx('fdf8ef'), 255), outline=(*hx('ded5c2'), 255), width=3 * SS)
    blend(img, lambda ld: ld.ellipse([W * 0.24, H * 0.14, W * 0.52, H * 0.40], fill=(255, 255, 255, 160)))
    save(img, 'egg.png', (w, h))

def gen_spoon(name, handle_col, fill_col, sugar=False, w=300, h=150):
    img, d, W, H = canvas(w, h)
    cy = H * 0.55
    capsule(d, [W * 0.02, cy - H * 0.085, W * 0.56, cy + H * 0.085], fill=(*handle_col, 255))
    bow = [W * 0.50, H * 0.16, W * 0.97, H * 0.92]
    d.ellipse(bow, fill=(*handle_col, 255))
    d.ellipse([bow[0] + 7 * SS, bow[1] + 7 * SS, bow[2] - 7 * SS, bow[3] - 7 * SS], fill=(*fill_col, 255))
    if sugar:
        rng = random.Random(5)
        for _ in range(26):
            x = rng.uniform(bow[0] + 12 * SS, bow[2] - 14 * SS); y = rng.uniform(bow[1] + 12 * SS, bow[3] - 14 * SS)
            if (x - W * 0.735) ** 2 / ((W * 0.21) ** 2) + (y - cy) ** 2 / ((H * 0.33) ** 2) < 1:
                d.ellipse([x, y, x + 2.4 * SS, y + 2.4 * SS], fill=(*hx('e8e2d4'), 255))
    else:
        blend(img, lambda ld: ld.ellipse([W * 0.60, H * 0.28, W * 0.76, H * 0.46], fill=(255, 245, 215, 130)))
    blend(img, lambda ld: ld.rounded_rectangle([W * 0.06, cy - H * 0.05, W * 0.40, cy - H * 0.005],
                                               radius=int(H * 0.022), fill=(255, 255, 255, 90)))
    save(img, name, (w, h))

def gen_banana_bowl(w=280, h=200):
    img, d, W, H = canvas(w, h)
    # shallow pink bowl
    d.ellipse([W * 0.06, H * 0.30, W * 0.94, H * 0.95], fill=(*hx('e85f9a'), 255))
    d.ellipse([W * 0.10, H * 0.26, W * 0.90, H * 0.78], fill=(*PINK, 255))
    d.ellipse([W * 0.15, H * 0.30, W * 0.85, H * 0.68], fill=(*hx('fff3d6'), 255))
    for (fx, fy, r) in [(0.32, 0.44, 0.105), (0.50, 0.38, 0.115), (0.68, 0.46, 0.10), (0.42, 0.55, 0.10), (0.59, 0.55, 0.095)]:
        banana_slice_at(d, W * fx, H * fy, W * r)
    save(img, 'banana_bowl.png', (w, h))

def gen_whisk(w=220, h=460):
    img, d, W, H = canvas(w, h)
    cx = W / 2
    capsule(d, [cx - W * 0.115, H * 0.02, cx + W * 0.115, H * 0.40], fill=(*PINK, 255))
    blend(img, lambda ld: ld.rounded_rectangle([cx - W * 0.07, H * 0.045, cx - W * 0.005, H * 0.34],
                                               radius=int(W * 0.03), fill=(255, 255, 255, 100)))
    grey = hx('b9c4d4')
    for k, fr in enumerate([0.36, 0.27, 0.155]):
        d.ellipse([cx - W * fr, H * 0.36, cx + W * fr, H * 0.985],
                  outline=(*grey, 255), width=int(6.5 * SS))
    d.line([(cx, H * 0.36), (cx, H * 0.985)], fill=(*grey, 255), width=int(6.5 * SS))
    d.ellipse([cx - W * 0.13, H * 0.355, cx + W * 0.13, H * 0.425], fill=(*hx('98a5b8'), 255))
    save(img, 'whisk.png', (w, h))

# ---------------------------------------------------------------- waffle maker
def rounded_sq(d, box, r, fill, outline=None, width=0):
    d.rounded_rectangle(box, radius=r, fill=fill, outline=outline, width=width)

def waffle_pockets(d, box, cell_col, gap_col):
    """3x3 pocket grid inside box, gap_col background assumed drawn."""
    x0, y0, x1, y1 = box
    cw = (x1 - x0) / 3.0; ch = (y1 - y0) / 3.0
    inset = min(cw, ch) * 0.12
    for r in range(3):
        for c in range(3):
            d.rounded_rectangle([x0 + c * cw + inset, y0 + r * ch + inset,
                                 x0 + (c + 1) * cw - inset, y0 + (r + 1) * ch - inset],
                                radius=int(min(cw, ch) * 0.18), fill=cell_col)

MOLD_BOXES = None  # filled by maker_geometry, reused by overlays (fractions of 640x760)
def maker_geometry(W, H):
    """Mold group boxes (2x2) on the BASE plate of the open maker, in px."""
    plate = [W * 0.165, H * 0.545, W * 0.835, H * 0.91]
    boxes = []
    px0, py0, px1, py1 = plate
    gw = (px1 - px0); gh = (py1 - py0)
    pad = gw * 0.045
    for r in range(2):
        for c in range(2):
            boxes.append([px0 + pad + c * (gw / 2) + (0 if c == 0 else pad * 0.2),
                          py0 + pad + r * (gh / 2) + (0 if r == 0 else pad * 0.2),
                          px0 + (c + 1) * (gw / 2) - pad + (0 if c == 1 else -pad * 0.2),
                          py0 + (r + 1) * (gh / 2) - pad + (0 if r == 1 else -pad * 0.2)])
    return plate, boxes

def gen_maker_open(w=640, h=760):
    img, d, W, H = canvas(w, h)
    # ---- upright lid ----
    rounded_sq(d, [W * 0.13, H * 0.012, W * 0.87, H * 0.43], int(W * 0.07), (*MAKER, 255))
    rounded_sq(d, [W * 0.175, H * 0.045, W * 0.825, H * 0.40], int(W * 0.05), (*PLATE, 255))
    lid_plate, _ = None, None
    # lid molds (2x2 of 3x3 pockets)
    lx0, ly0, lx1, ly1 = W * 0.21, H * 0.07, W * 0.79, H * 0.375
    for r in range(2):
        for c in range(2):
            bx = [lx0 + c * (lx1 - lx0) / 2 + (lx1 - lx0) * 0.02, ly0 + r * (ly1 - ly0) / 2 + (ly1 - ly0) * 0.03,
                  lx0 + (c + 1) * (lx1 - lx0) / 2 - (lx1 - lx0) * 0.02, ly0 + (r + 1) * (ly1 - ly0) / 2 - (ly1 - ly0) * 0.03]
            waffle_pockets(d, bx, (*MOLD, 255), None)
    # hinge
    rounded_sq(d, [W * 0.42, H * 0.425, W * 0.58, H * 0.475], int(W * 0.02), (*MAKER_DK, 255))
    # ---- base ----
    blend(img, lambda ld: ld.ellipse([W * 0.06, H * 0.92, W * 0.94, H * 0.995], fill=(*INK, 40)))
    rounded_sq(d, [W * 0.09, H * 0.475, W * 0.91, H * 0.965], int(W * 0.075), (*MAKER, 255))
    plate, boxes = maker_geometry(W, H)
    rounded_sq(d, [plate[0] - W * 0.02, plate[1] - H * 0.012, plate[2] + W * 0.02, plate[3] + H * 0.012],
               int(W * 0.045), (*PLATE, 255))
    for bx in boxes:
        waffle_pockets(d, bx, (*MOLD, 255), None)
    # body gloss + power recess (button itself is a separate sprite)
    blend(img, lambda ld: ld.rounded_rectangle([W * 0.115, H * 0.49, W * 0.16, H * 0.93],
                                               radius=int(W * 0.02), fill=(255, 255, 255, 70)))
    blend(img, lambda ld: ld.ellipse([W * 0.46, H * 0.918, W * 0.54, H * 0.957], fill=(*INK, 40)))
    save(img, 'maker_open.png', (w, h))

def gen_maker_overlay(name, kind, w=640, h=760):
    img, d, W, H = canvas(w, h)
    _, boxes = maker_geometry(W, H)
    if kind == 'oil':
        rng = random.Random(8)
        def dots(ld):
            for bx in boxes:
                for _ in range(16):
                    x = rng.uniform(bx[0], bx[2]); y = rng.uniform(bx[1], bx[3]); r = rng.uniform(2, 5) * SS
                    ld.ellipse([x - r, y - r, x + r, y + r], fill=(255, 235, 150, 150))
        blend(img, dots)
    elif kind == 'batter':
        for bx in boxes:
            inset = (bx[2] - bx[0]) * 0.04
            d.rounded_rectangle([bx[0] + inset, bx[1] + inset, bx[2] - inset, bx[3] - inset],
                                radius=int((bx[2] - bx[0]) * 0.12), fill=(*BATTER, 255))
            blend(img, lambda ld, b=bx, i=inset: ld.rounded_rectangle(
                [b[0] + i * 2.4, b[1] + i * 2.4, b[0] + (b[2] - b[0]) * 0.55, b[1] + (b[3] - b[1]) * 0.4],
                radius=int((b[2] - b[0]) * 0.08), fill=(255, 250, 230, 90)))
    else:  # waffles
        for bx in boxes:
            grow = (bx[2] - bx[0]) * 0.045
            b = [bx[0] - grow, bx[1] - grow, bx[2] + grow, bx[3] + grow]
            d.rounded_rectangle(b, radius=int((b[2] - b[0]) * 0.16), fill=(*WAFFLE_CRUST, 255))
            d.rounded_rectangle([b[0] + 3 * SS, b[1] + 3 * SS, b[2] - 3 * SS, b[3] - 5 * SS],
                                radius=int((b[2] - b[0]) * 0.14), fill=(*WAFFLE, 255))
            waffle_pockets(d, [b[0] + 8 * SS, b[1] + 8 * SS, b[2] - 8 * SS, b[3] - 8 * SS], (*WAFFLE_DK, 255), None)
    save(img, name, (w, h))

def gen_maker_closed(w=640, h=560):
    img, d, W, H = canvas(w, h)
    blend(img, lambda ld: ld.ellipse([W * 0.05, H * 0.88, W * 0.95, H * 0.995], fill=(*INK, 45)))
    # base lip (slightly wider than the lid, with a seam shadow under the lid)
    rounded_sq(d, [W * 0.085, H * 0.62, W * 0.915, H * 0.94], int(W * 0.07), (*MAKER_DK, 255))
    rounded_sq(d, [W * 0.085, H * 0.62, W * 0.915, H * 0.80], int(W * 0.07), (*hx('a5d0ee'), 255))
    # hinge bump at the back
    rounded_sq(d, [W * 0.40, H * 0.005, W * 0.60, H * 0.10], int(W * 0.03), (*MAKER_DK, 255))
    # domed lid with a raised inner panel
    rounded_sq(d, [W * 0.075, H * 0.045, W * 0.925, H * 0.72], int(W * 0.115), (*MAKER, 255))
    rounded_sq(d, [W * 0.075, H * 0.045, W * 0.925, H * 0.72], int(W * 0.115), None,
               outline=(*MAKER_DK, 255), width=3 * SS)
    rounded_sq(d, [W * 0.155, H * 0.115, W * 0.845, H * 0.60], int(W * 0.09), (*hx('c8e6fa'), 255))
    rounded_sq(d, [W * 0.155, H * 0.115, W * 0.845, H * 0.60], int(W * 0.09), None,
               outline=(*hx('9fcaec'), 255), width=3 * SS)
    # embossed heart + script underline
    draw_heart(d, W * 0.5, H * 0.32, W * 0.085, (*hx('9fcaec'), 255))
    capsule(d, [W * 0.40, H * 0.475, W * 0.60, H * 0.505], fill=(*hx('9fcaec'), 255))
    blend(img, lambda ld: ld.rounded_rectangle([W * 0.19, H * 0.135, W * 0.46, H * 0.26],
                                               radius=int(W * 0.05), fill=(255, 255, 255, 120)))
    blend(img, lambda ld: ld.ellipse([W * 0.455, H * 0.815, W * 0.545, H * 0.90], fill=(*INK, 40)))
    save(img, 'maker_closed.png', (w, h))

def gen_power_btn(s=120):
    img, d, W, H = canvas(s, s)
    d.ellipse([4 * SS, 6 * SS, W - 4 * SS, H - 2 * SS], fill=(*hx('7fa8c8'), 255))
    d.ellipse([4 * SS, 4 * SS, W - 4 * SS, H - 6 * SS], fill=(*hx('eef6fd'), 255))
    d.ellipse([W * 0.16, H * 0.16, W * 0.84, H * 0.84], fill=(*hx('d7ecfa'), 255))
    # power glyph
    d.arc([W * 0.30, H * 0.32, W * 0.70, H * 0.74], -60, 240, fill=(*hx('4a9e5f'), 255), width=6 * SS)
    d.line([(W * 0.5, H * 0.22), (W * 0.5, H * 0.50)], fill=(*hx('4a9e5f'), 255), width=6 * SS)
    save(img, 'power_btn.png', (s, s))

def gen_glow(name, color, s=220):
    S = s * SS
    img = Image.new('RGBA', (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([S * 0.22, S * 0.22, S * 0.78, S * 0.78], fill=(*color, 200))
    img = img.filter(ImageFilter.GaussianBlur(S * 0.10))
    save(img, name, (s, s))

def gen_spray_bottle(w=220, h=420):
    img, d, W, H = canvas(w, h)
    # glass body with oil
    body = [W * 0.16, H * 0.34, W * 0.84, H * 0.97]
    d.rounded_rectangle(body, radius=int(W * 0.20), fill=(*hx('f2efe6'), 255), outline=(*hx('cfc8b4'), 255), width=3 * SS)
    d.rounded_rectangle([body[0] + 4 * SS, H * 0.52, body[2] - 4 * SS, body[3] - 4 * SS],
                        radius=int(W * 0.17), fill=(*hx('f6d35e'), 255))
    # sprayer tube
    d.line([(W * 0.5, H * 0.36), (W * 0.5, H * 0.90)], fill=(*hx('d9cfa8'), 255), width=4 * SS)
    # orange cap
    rounded_sq(d, [W * 0.24, H * 0.10, W * 0.76, H * 0.36], int(W * 0.07), (*ORANGE, 255))
    rounded_sq(d, [W * 0.38, H * 0.02, W * 0.62, H * 0.11], int(W * 0.04), (*hx('f08a20'), 255))
    rounded_sq(d, [W * 0.20, H * 0.155, W * 0.30, H * 0.27], int(W * 0.03), (*hx('f08a20'), 255))  # trigger
    blend(img, lambda ld: ld.rounded_rectangle([W * 0.24, H * 0.56, W * 0.36, H * 0.90],
                                               radius=int(W * 0.05), fill=(255, 255, 255, 90)))
    save(img, 'spray_bottle.png', (w, h))

def gen_puff(name, tint=(255, 255, 255), s=260):
    S = s * SS
    img = Image.new('RGBA', (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    # rounded cloud: big core + ring of lobes (keeps a compact, symmetric silhouette)
    cx, cy = S * 0.5, S * 0.55
    d.ellipse([cx - S * 0.26, cy - S * 0.24, cx + S * 0.26, cy + S * 0.24], fill=(*tint, 255))
    for i in range(6):
        a = math.pi * 2 * i / 6
        r = S * (0.15 if i % 2 == 0 else 0.115)
        x = cx + S * 0.21 * math.cos(a); y = cy + S * 0.17 * math.sin(a)
        d.ellipse([x - r, y - r, x + r, y + r], fill=(*tint, 255))
    img = img.filter(ImageFilter.GaussianBlur(S * 0.05))
    save(img, name, (s, s))

def gen_bowl_pour(w=260, h=210):
    img, d, W, H = canvas(w, h)
    # side-view bowl with batter, used to drag onto the maker
    d.ellipse([W * 0.07, H * 0.18, W * 0.93, H * 0.62], fill=(*BATTER, 255))
    d.chord([W * 0.04, H * 0.06, W * 0.96, H * 0.96], 0, 180, fill=(*TEAL, 255))
    d.rectangle([W * 0.04, H * 0.30, W * 0.96, H * 0.51], fill=(*TEAL, 255))
    d.ellipse([W * 0.04, H * 0.13, W * 0.96, H * 0.49], fill=(*TEAL_DK, 255))
    d.ellipse([W * 0.09, H * 0.17, W * 0.91, H * 0.45], fill=(*BATTER, 255))
    blend(img, lambda ld: ld.ellipse([W * 0.16, H * 0.20, W * 0.48, H * 0.33], fill=(255, 250, 230, 110)))
    blend(img, lambda ld: ld.arc([W * 0.10, H * 0.30, W * 0.90, H * 0.92], 40, 140,
                                 fill=(255, 255, 255, 90), width=7 * SS))
    save(img, 'bowl_pour.png', (w, h))

# ---------------------------------------------------------------- plate + waffles + toppings
def gen_plate(w=760, h=560):
    img, d, W, H = canvas(w, h)
    blend(img, lambda ld: ld.ellipse([W * 0.04, H * 0.16, W * 0.96, H * 0.99], fill=(*INK, 30)))
    d.ellipse([W * 0.02, H * 0.02, W * 0.98, H * 0.93], fill=(*hx('e9e9ee'), 255))
    d.ellipse([W * 0.045, H * 0.045, W * 0.955, H * 0.87], fill=(*hx('ffffff'), 255))
    d.ellipse([W * 0.16, H * 0.18, W * 0.84, H * 0.78], fill=(*hx('f1f1f4'), 255))
    save(img, 'plate.png', (w, h))

def gen_waffle_single(s=360):
    img, d, W, H = canvas(s, s)
    b = [W * 0.05, H * 0.05, W * 0.95, H * 0.95]
    d.rounded_rectangle(b, radius=int(W * 0.16), fill=(*WAFFLE_CRUST, 255))
    d.rounded_rectangle([b[0] + 3 * SS, b[1] + 3 * SS, b[2] - 3 * SS, b[3] - 6 * SS],
                        radius=int(W * 0.14), fill=(*WAFFLE, 255))
    waffle_pockets(d, [b[0] + 10 * SS, b[1] + 10 * SS, b[2] - 10 * SS, b[3] - 12 * SS], (*WAFFLE_DK, 255), None)
    blend(img, lambda ld: ld.rounded_rectangle([W * 0.12, H * 0.10, W * 0.50, H * 0.28],
                                               radius=int(W * 0.08), fill=(255, 245, 215, 70)))
    save(img, 'waffle_single.png', (s, s))

def gen_strawberry(w=200, h=220):
    img, d, W, H = canvas(w, h)
    body = (*hx('ff4d5e'), 255)
    d.polygon([(W * 0.10, H * 0.38), (W * 0.90, H * 0.38), (W * 0.52, H * 0.97)], fill=body)
    d.ellipse([W * 0.08, H * 0.16, W * 0.92, H * 0.62], fill=body)
    # leaves
    for (a, l) in [(-50, 0.30), (-15, 0.36), (20, 0.34), (55, 0.28)]:
        ang = math.radians(a - 90)
        x2 = W * 0.5 + W * l * math.cos(ang); y2 = H * 0.22 + W * l * math.sin(ang)
        d.line([(W * 0.5, H * 0.235), (x2, y2)], fill=(*GREEN, 255), width=int(10 * SS))
    d.ellipse([W * 0.42, H * 0.10, W * 0.58, H * 0.28], fill=(*GREEN, 255))
    rng = random.Random(3)
    for _ in range(12):
        x = rng.uniform(W * 0.22, W * 0.78); y = rng.uniform(H * 0.30, H * 0.74)
        if (x - W * 0.5) ** 2 / (W * 0.36) ** 2 + (y - H * 0.45) ** 2 / (H * 0.42) ** 2 < 1:
            d.ellipse([x, y, x + 4 * SS, y + 6 * SS], fill=(*hx('ffd9a0'), 255))
    blend(img, lambda ld: ld.ellipse([W * 0.20, H * 0.24, W * 0.42, H * 0.42], fill=(255, 255, 255, 90)))
    save(img, 'strawberry.png', (w, h))

def gen_banana_slice(s=160):
    img, d, W, H = canvas(s, s)
    banana_slice_at(d, W / 2, H / 2, W * 0.45)
    save(img, 'banana_slice.png', (s, s))

def gen_blueberry(s=140):
    img, d, W, H = canvas(s, s)
    d.ellipse([W * 0.06, H * 0.10, W * 0.94, H * 0.97], fill=(*hx('3b4ea8'), 255))
    d.ellipse([W * 0.06, H * 0.06, W * 0.94, H * 0.92], fill=(*hx('4f66c9'), 255))
    # calyx star
    cx, cy = W * 0.5, H * 0.32
    pts = []
    for i in range(10):
        a = math.pi / 5 * i - math.pi / 2
        r = W * (0.10 if i % 2 == 0 else 0.045)
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    d.polygon(pts, fill=(*hx('2c3a85'), 255))
    blend(img, lambda ld: ld.ellipse([W * 0.20, H * 0.16, W * 0.46, H * 0.40], fill=(255, 255, 255, 90)))
    save(img, 'blueberry.png', (s, s))

def gen_cream(w=220, h=210):
    img, d, W, H = canvas(w, h)
    col = (*hx('fdfbf6'), 255); shade = (*hx('e8e2d4'), 255)
    d.ellipse([W * 0.08, H * 0.55, W * 0.92, H * 0.95], fill=shade)
    d.ellipse([W * 0.08, H * 0.50, W * 0.92, H * 0.88], fill=col)
    d.ellipse([W * 0.18, H * 0.32, W * 0.82, H * 0.66], fill=col)
    d.ellipse([W * 0.30, H * 0.16, W * 0.70, H * 0.44], fill=col)
    # curled tip
    d.polygon([(W * 0.46, H * 0.24), (W * 0.62, H * 0.06), (W * 0.60, H * 0.30)], fill=col)
    blend(img, lambda ld: ld.ellipse([W * 0.24, H * 0.38, W * 0.46, H * 0.56], fill=(*hx('d8d2c2'), 110)))
    save(img, 'cream.png', (w, h))

def gen_choc(w=180, h=160):
    img, d, W, H = canvas(w, h)
    dk = hx('5a3a22'); md = hx('7a4f2d'); lt = hx('96653c')
    rounded_sq(d, [W * 0.06, H * 0.14, W * 0.94, H * 0.94], int(W * 0.08), (*dk, 255))
    rounded_sq(d, [W * 0.06, H * 0.06, W * 0.94, H * 0.86], int(W * 0.08), (*md, 255))
    for c in range(2):
        rounded_sq(d, [W * (0.14 + c * 0.42), H * 0.16, W * (0.44 + c * 0.42), H * 0.74], int(W * 0.05), (*lt, 255))
        rounded_sq(d, [W * (0.14 + c * 0.42), H * 0.16, W * (0.44 + c * 0.42), H * 0.42], int(W * 0.05), (*md, 255))
    save(img, 'choc.png', (w, h))

def gen_syrup(w=320, h=220):
    img, d, W, H = canvas(w, h)
    amber = hx('c87a1e'); hi = hx('e8a64a')
    pts = []
    for i, t in enumerate(range(0, 101, 2)):
        x = W * 0.06 + (W * 0.88) * t / 100.0
        y = H * 0.5 + math.sin(t / 100.0 * math.pi * 3) * H * 0.30
        pts.append((x, y))
    d.line(pts, fill=(*amber, 255), width=int(H * 0.17), joint='curve')
    for p in (pts[0], pts[-1]):
        r = H * 0.085
        d.ellipse([p[0] - r, p[1] - r, p[0] + r, p[1] + r], fill=(*amber, 255))
    blend(img, lambda ld: ld.line([(x, y - H * 0.045) for x, y in pts], fill=(*hi, 160),
                                  width=int(H * 0.05), joint='curve'))
    save(img, 'syrup.png', (w, h))

def gen_tray(w=1040, h=190):
    img, d, W, H = canvas(w, h)
    dark = tuple(max(0, c - 42) for c in PINK)
    rounded_sq(d, [4 * SS, 10 * SS, W - 4 * SS, H - 4 * SS], int(H * 0.22), (*dark, 255))
    rounded_sq(d, [4 * SS, 4 * SS, W - 4 * SS, H - 12 * SS], int(H * 0.22), (*PINK, 255))
    rounded_sq(d, [18 * SS, 16 * SS, W - 18 * SS, H - 26 * SS], int(H * 0.18), (*hx('ffd9ec'), 255))
    save(img, 'tray.png', (w, h))

def gen_arrow(w=260, h=160):
    img, d, W, H = canvas(w, h)
    cy = H / 2
    shaft_h = H * 0.32
    head = W * 0.38
    pts = [(W * 0.04, cy - shaft_h / 2), (W - head, cy - shaft_h / 2), (W - head, cy - H * 0.46),
           (W * 0.97, cy), (W - head, cy + H * 0.46), (W - head, cy + shaft_h / 2), (W * 0.04, cy + shaft_h / 2)]
    d.polygon(pts, fill=(*CORAL, 255))
    # white inner: shrink each vertex toward the centroid
    cx = sum(x for x, _ in pts) / len(pts)
    cyc = sum(y for _, y in pts) / len(pts)
    k = 0.86
    inner = [(cx + (x - cx) * k, cyc + (y - cyc) * k) for x, y in pts]
    d.polygon(inner, fill=(255, 255, 255, 255))
    save(img, 'arrow.png', (w, h))

def gen_round_button(name, color, dia=180):
    D = dia * SS
    img = Image.new('RGBA', (D, D), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    m = 6 * SS
    dark = tuple(max(0, c - 48) for c in color)
    d.ellipse([m, m + 7 * SS, D - m, D - m], fill=(*dark, 255))
    d.ellipse([m, m, D - m, D - m - 7 * SS], fill=(*color, 255))
    d.ellipse([m, m, D - m, D - m], outline=(255, 255, 255, 255), width=7 * SS)
    blend(img, lambda ld: ld.ellipse([D * 0.30, D * 0.12, D * 0.70, D * 0.30], fill=(255, 255, 255, 70)))
    save(img, name, (dia, dia))

def gen_pill(name, color, w=520, h=132):
    img, d, W, H = canvas(w, h)
    m = 8 * SS
    dark = tuple(max(0, c - 42) for c in color)
    def cap(box, **kw):
        d.rounded_rectangle(box, radius=int((box[3] - box[1]) / 2), **kw)
    cap([m, m + 6 * SS, W - m, H - m], fill=(*dark, 255))
    cap([m, m, W - m, H - m - 6 * SS], fill=(*color, 255))
    cap([m, m, W - m, H - m], outline=(255, 255, 255, 255), width=7 * SS)
    gb = [m + 18 * SS, m + 9 * SS, W - m - 18 * SS, int(H * 0.45)]
    blend(img, lambda ld: ld.rounded_rectangle(gb, radius=int((gb[3] - gb[1]) / 2), fill=(255, 255, 255, 56)))
    save(img, name, (w, h))

if __name__ == '__main__':
    gen_wood()
    gen_kitchen()
    gen_mascot('mascot_neutral.png', 'neutral')
    gen_mascot('mascot_happy.png', 'happy')
    gen_mascot('mascot_love.png', 'love')
    gen_bowl_top()
    gen_milk_fill()
    gen_egg_yolk()
    gen_sugar_pile()
    gen_honey_blob()
    gen_banana_bits()
    gen_batter('batter_lumpy.png', smooth=False)
    gen_batter('batter_smooth.png', smooth=True)
    gen_milk_jug()
    gen_egg()
    gen_spoon('spoon_sugar.png', PINK, hx('ffffff'), sugar=True)
    gen_spoon('spoon_honey.png', BLUE, hx('c46a1f'))
    gen_banana_bowl()
    gen_whisk()
    gen_maker_open()
    gen_maker_overlay('oil_sheen.png', 'oil')
    gen_maker_overlay('batter_4.png', 'batter')
    gen_maker_overlay('waffles_4.png', 'waffles')
    gen_maker_closed()
    gen_power_btn()
    gen_glow('power_glow.png', hx('8af0a0'))
    gen_spray_bottle()
    gen_puff('steam.png')
    gen_puff('spray_puff.png', tint=(255, 250, 220), s=200)
    gen_bowl_pour()
    gen_plate()
    gen_waffle_single()
    gen_strawberry()
    gen_banana_slice()
    gen_blueberry()
    gen_cream()
    gen_choc()
    gen_syrup()
    gen_tray()
    gen_arrow()
    gen_round_button('btn_round_pink.png', PINK)
    gen_pill('pill_gold.png', GOLD)
    print("done")
