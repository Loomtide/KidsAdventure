#!/usr/bin/env python3
"""Generate Kids Adventure home-screen sprites (soft rounded candy vector) to Assets/Art/home/.
Same visual identity as CountTheFruits / ShapeMatch (Fredoka, glossy rounded shapes)."""
import os, math
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = os.path.join(os.path.dirname(__file__), "..")
OUT = os.path.join(ROOT, "Assets", "Art", "home")
FREDOKA = os.path.join(ROOT, "Assets", "Fonts", "Fredoka.ttf")
os.makedirs(OUT, exist_ok=True)
SS = 2  # supersample

def hx(h):
    h = h.lstrip('#')
    return tuple(int(h[i:i+2], 16) for i in (0, 2, 4))

INK = hx('46365e')
CORAL = hx('ff5d72'); ORANGE = hx('ff9d3f'); GOLD = hx('ffce4f')
GREEN = hx('5fd68a'); BLUE = hx('4fb4ff'); PURPLE = hx('a06ee8'); PINK = hx('ff7ec2')
ROYAL = hx('4f6bff')
PED_TOP = hx('aef0c8'); PED_MID = hx('7be0a8'); PED_DARK = hx('56cf89')

def save(img, name):
    img.save(os.path.join(OUT, name))
    print("wrote", name, img.size)

def blend(img, draw_fn):
    """Draw semi-transparent details on a temp layer and alpha-composite them.
    (ImageDraw on an RGBA image REPLACES pixels - it never blends.)"""
    layer = Image.new('RGBA', img.size, (0, 0, 0, 0))
    draw_fn(ImageDraw.Draw(layer))
    img.alpha_composite(layer)

# ---------- logo: per-letter rainbow bubble text with white stroke + soft shadow ----------
def gen_logo(text="Kids Adventure", final_h=340):
    F = 230 * SS
    font = ImageFont.truetype(FREDOKA, F)
    stroke = int(F * 0.075)
    colors = [CORAL, ORANGE, GOLD, GREEN, BLUE, PURPLE, PINK]
    tiles = []
    ci = 0
    for ch in text:
        if ch == ' ':
            tiles.append((None, int(F * 0.30)))
            continue
        bbox = font.getbbox(ch, stroke_width=stroke)
        w = bbox[2] - bbox[0] + stroke * 2
        h = bbox[3] - bbox[1] + stroke * 2
        tile = Image.new('RGBA', (w + 40, h + 40), (0, 0, 0, 0))
        d = ImageDraw.Draw(tile)
        d.text((20 - bbox[0] + stroke, 20 - bbox[1] + stroke), ch, font=font,
               fill=(*colors[ci % len(colors)], 255), stroke_width=stroke, stroke_fill=(255, 255, 255, 255))
        rot = (3.5 if ci % 2 == 0 else -3.5)
        tile = tile.rotate(rot, expand=True, resample=Image.BICUBIC)
        adv = font.getlength(ch) + stroke * 0.2
        tiles.append((tile, int(adv)))
        ci += 1
    total_w = sum(a for _, a in tiles) + 200
    H = int(F * 1.9)
    img = Image.new('RGBA', (total_w, H), (0, 0, 0, 0))
    x = 100
    i = 0
    for tile, adv in tiles:
        if tile is not None:
            yb = int(math.sin(i * 1.05) * F * 0.055)
            img.alpha_composite(tile, (x - 20, H // 2 - tile.size[1] // 2 + yb))
            i += 1
        x += adv
    bbox = img.getbbox()
    img = img.crop((bbox[0] - 10, bbox[1] - 10, bbox[2] + 10, bbox[3] + 24))
    # soft ink shadow
    a = img.split()[3]
    shadow = Image.new('RGBA', img.size, (0, 0, 0, 0))
    shadow.paste(Image.new('RGBA', img.size, (*INK, 90)), (0, 0), a)
    shadow = shadow.filter(ImageFilter.GaussianBlur(9 * SS))
    out = Image.new('RGBA', (img.size[0] + 30, img.size[1] + 18 * SS + 30), (0, 0, 0, 0))
    out.alpha_composite(shadow, (15, 15 + 9 * SS))
    out.alpha_composite(img, (15, 15))
    final_w = int(out.size[0] * final_h / out.size[1])
    save(out.resize((final_w, final_h), Image.LANCZOS), 'logo.png')

# ---------- pedestal: green podium disc ----------
def gen_pedestal(w=560, h=190):
    W, H = w * SS, h * SS
    img = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img, 'RGBA')
    # base shadow on grass
    d.ellipse([W * 0.04, H * 0.55, W * 0.96, H * 0.99], fill=(*INK, 36))
    # dark side (cylinder)
    d.ellipse([W * 0.03, H * 0.32, W * 0.97, H * 0.92], fill=(*PED_DARK, 255))
    d.rectangle([W * 0.03, H * 0.42, W * 0.97, H * 0.62], fill=(*PED_DARK, 255))
    # mid rim
    d.ellipse([W * 0.03, H * 0.12, W * 0.97, H * 0.72], fill=(*PED_MID, 255))
    # light top surface
    d.ellipse([W * 0.085, H * 0.18, W * 0.915, H * 0.62], fill=(*PED_TOP, 255))
    # subtle inner highlight
    blend(img, lambda ld: ld.ellipse([W * 0.16, H * 0.23, W * 0.84, H * 0.50], fill=(255, 255, 255, 50)))
    save(img.resize((w, h), Image.LANCZOS), 'pedestal.png')

# ---------- label pill (capsule, white stroke) ----------
def gen_pill(name, color, w=520, h=132):
    W, H = w * SS, h * SS
    img = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img, 'RGBA')
    m = 8 * SS
    dark = tuple(max(0, c - 42) for c in color)
    # radius must be <= half the box height or Pillow degenerates the capsule
    def capsule(box, **kw):
        d.rounded_rectangle(box, radius=int((box[3] - box[1]) / 2), **kw)
    capsule([m, m + 6 * SS, W - m, H - m], fill=(*dark, 255))
    capsule([m, m, W - m, H - m - 6 * SS], fill=(*color, 255))
    capsule([m, m, W - m, H - m], outline=(255, 255, 255, 255), width=7 * SS)
    # gloss
    gb = [m + 18 * SS, m + 9 * SS, W - m - 18 * SS, int(H * 0.45)]
    blend(img, lambda ld: ld.rounded_rectangle(gb, radius=int((gb[3] - gb[1]) / 2), fill=(255, 255, 255, 56)))
    save(img.resize((w, h), Image.LANCZOS), name)

# ---------- lock ----------
def gen_lock(w=230, h=270):
    W, H = w * SS, h * SS
    img = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img, 'RGBA')
    grey = hx('aab3c5'); grey_d = hx('8a93a8')
    # shackle
    sw = int(W * 0.16)
    d.arc([W * 0.18, H * 0.02, W * 0.82, H * 0.62], 180, 360, fill=(*grey, 255), width=sw)
    blend(img, lambda ld: ld.arc([W * 0.18 + sw * 0.28, H * 0.02 + sw * 0.28, W * 0.82 - sw * 0.28, H * 0.62 - sw * 0.28],
          200, 340, fill=(*grey_d, 130), width=int(sw * 0.3)))
    # body
    body_dark = tuple(max(0, c - 50) for c in GOLD)
    d.rounded_rectangle([W * 0.06, H * 0.40 + 8 * SS, W * 0.94, H * 0.98], radius=int(W * 0.16), fill=(*body_dark, 255))
    d.rounded_rectangle([W * 0.06, H * 0.40, W * 0.94, H * 0.98 - 8 * SS], radius=int(W * 0.16), fill=(*GOLD, 255))
    blend(img, lambda ld: ld.rounded_rectangle([W * 0.13, H * 0.45, W * 0.87, H * 0.62], radius=int(W * 0.10), fill=(255, 255, 255, 62)))
    # keyhole
    cx, cy = W * 0.5, H * 0.64
    d.ellipse([cx - W * 0.085, cy - W * 0.085, cx + W * 0.085, cy + W * 0.085], fill=(*INK, 255))
    d.polygon([(cx - W * 0.055, cy), (cx + W * 0.055, cy), (cx + W * 0.085, H * 0.88), (cx - W * 0.085, H * 0.88)], fill=(*INK, 255))
    save(img.resize((w, h), Image.LANCZOS), 'lock.png')

# ---------- glossy round button ----------
def gen_round_button(name, color, dia=180):
    D = dia * SS
    img = Image.new('RGBA', (D, D), (0, 0, 0, 0))
    d = ImageDraw.Draw(img, 'RGBA')
    m = 6 * SS
    dark = tuple(max(0, c - 48) for c in color)
    d.ellipse([m, m + 7 * SS, D - m, D - m], fill=(*dark, 255))
    d.ellipse([m, m, D - m, D - m - 7 * SS], fill=(*color, 255))
    d.ellipse([m, m, D - m, D - m], outline=(255, 255, 255, 255), width=7 * SS)
    blend(img, lambda ld: ld.ellipse([D * 0.30, D * 0.12, D * 0.70, D * 0.30], fill=(255, 255, 255, 70)))
    save(img.resize((dia, dia), Image.LANCZOS), name)

# ---------- music note (beamed pair), white ----------
def gen_note(w=130, h=130):
    W, H = w * SS, h * SS
    img = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img, 'RGBA')
    wcol = (255, 255, 255, 255)
    stem_w = int(W * 0.085)
    # heads
    d.ellipse([W * 0.06, H * 0.70, W * 0.40, H * 0.95], fill=wcol)
    d.ellipse([W * 0.58, H * 0.62, W * 0.92, H * 0.87], fill=wcol)
    # stems
    d.rectangle([W * 0.40 - stem_w, H * 0.16, W * 0.40, H * 0.84], fill=wcol)
    d.rectangle([W * 0.92 - stem_w, H * 0.08, W * 0.92, H * 0.76], fill=wcol)
    # beam (slanted)
    d.polygon([(W * 0.40 - stem_w, H * 0.16), (W * 0.92, H * 0.08),
               (W * 0.92, H * 0.26), (W * 0.40 - stem_w, H * 0.34)], fill=wcol)
    save(img.resize((w, h), Image.LANCZOS), 'note_icon.png')

# ---------- sparkle: 4-point star + glow ----------
def gen_sparkle(s=170):
    S = s * SS
    img = Image.new('RGBA', (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img, 'RGBA')
    c = S / 2
    R, r = S * 0.46, S * 0.10
    pts = []
    for i in range(8):
        ang = math.pi / 4 * i - math.pi / 2
        rad = R if i % 2 == 0 else r
        pts.append((c + rad * math.cos(ang), c + rad * math.sin(ang)))
    glow = Image.new('RGBA', (S, S), (0, 0, 0, 0))
    ImageDraw.Draw(glow).polygon(pts, fill=(255, 255, 255, 160))
    glow = glow.filter(ImageFilter.GaussianBlur(S * 0.05))
    img.alpha_composite(glow)
    d.polygon(pts, fill=(255, 255, 255, 245))
    save(img.resize((s, s), Image.LANCZOS), 'sparkle.png')

# ---------- balloon ----------
def gen_balloon(name, color, w=240, h=360):
    W, H = w * SS, h * SS
    img = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img, 'RGBA')
    dark = tuple(max(0, c - 40) for c in color)
    # body
    d.ellipse([W * 0.08, H * 0.02, W * 0.92, H * 0.60], fill=(*dark, 255))
    d.ellipse([W * 0.08, H * 0.01, W * 0.92, H * 0.57], fill=(*color, 255))
    # highlight
    blend(img, lambda ld: ld.ellipse([W * 0.20, H * 0.06, W * 0.48, H * 0.26], fill=(255, 255, 255, 120)))
    # knot
    d.polygon([(W * 0.5 - W * 0.07, H * 0.585), (W * 0.5 + W * 0.07, H * 0.585), (W * 0.5, H * 0.525)], fill=(*dark, 255))
    # wavy string
    pts = [(W * 0.5 + math.sin(t / 14.0) * W * 0.05, H * 0.60 + t / 100.0 * H * 0.38) for t in range(0, 101, 4)]
    d.line(pts, fill=(255, 255, 255, 220), width=int(3.4 * SS), joint='curve')
    save(img.resize((w, h), Image.LANCZOS), name)

# ---------- NEW badge: royal-blue starburst ----------
def gen_new_badge(s=210):
    S = s * SS
    img = Image.new('RGBA', (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img, 'RGBA')
    c = S / 2
    n = 12
    R, r = S * 0.47, S * 0.375
    pts = []
    for i in range(n * 2):
        ang = math.pi / n * i - math.pi / 2
        rad = R if i % 2 == 0 else r
        pts.append((c + rad * math.cos(ang), c + rad * math.sin(ang)))
    dark = tuple(max(0, ch - 50) for ch in ROYAL)
    d.polygon([(x, y + 5 * SS) for x, y in pts], fill=(*dark, 255))
    d.polygon(pts, fill=(*ROYAL, 255))
    font = ImageFont.truetype(FREDOKA, int(S * 0.30))
    txt = Image.new('RGBA', (S, S), (0, 0, 0, 0))
    td = ImageDraw.Draw(txt)
    bbox = td.textbbox((0, 0), "NEW", font=font)
    td.text((c - (bbox[2] - bbox[0]) / 2, c - (bbox[3] - bbox[1]) / 2 - bbox[1]), "NEW",
            font=font, fill=(255, 255, 255, 255))
    txt = txt.rotate(-10, resample=Image.BICUBIC, center=(c, c))
    img.alpha_composite(txt)
    save(img.resize((s, s), Image.LANCZOS), 'new_badge.png')

if __name__ == '__main__':
    gen_logo()
    gen_pedestal()
    gen_pill('pill_coral.png', CORAL)
    gen_pill('pill_blue.png', BLUE)
    gen_pill('pill_grey.png', hx('aab3c5'))
    gen_lock()
    gen_round_button('btn_round_orange.png', ORANGE)
    gen_note()
    gen_sparkle()
    gen_balloon('balloon_coral.png', CORAL)
    gen_balloon('balloon_blue.png', BLUE)
    gen_balloon('balloon_gold.png', GOLD)
    gen_new_badge()
    print("done")
