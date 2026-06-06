#!/usr/bin/env python3
"""Generate Count the Fruits candy background sprites (soft rounded vector) to Assets/Art/bg/.
Palette matches the frozen hero shot."""
import os, math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Art", "bg")
os.makedirs(OUT, exist_ok=True)
SS = 2  # supersample factor for smooth edges

def hx(h):
    h = h.lstrip('#')
    return tuple(int(h[i:i+2], 16) for i in (0, 2, 4))

SKY_TOP = hx('a8e6ff'); SKY_MID = hx('bdeeff'); MINT = hx('c8f7df')
SUN = hx('fff2bf')
HILL_FAR = hx('9be6c4'); HILL_NEAR = hx('6fd6a6')
GROUND_TOP = hx('7be0a8'); GROUND_BOT = hx('56cf89'); GROUND_EDGE = hx('8ee7b8')

def save(img, name):
    img.save(os.path.join(OUT, name))
    print("wrote", name, img.size)

def lerp(a, b, t):
    return tuple(int(a[i] + (b[i]-a[i])*t) for i in range(3))

# ---- sky: 3-stop vertical gradient, full 16:9 frame ----
def gen_sky(w=1920, h=1080):
    arr = np.zeros((h, w, 3), dtype=np.uint8)
    for y in range(h):
        t = y/(h-1)
        if t < 0.42:
            c = lerp(SKY_TOP, SKY_MID, t/0.42)
        else:
            c = lerp(SKY_MID, MINT, (t-0.42)/0.58)
        arr[y, :, :] = c
    save(Image.fromarray(arr, 'RGB'), 'sky.png')

# ---- sun: soft radial glow, transparent ----
def gen_sun(d=700):
    D = d*SS
    img = Image.new('RGBA', (D, D), (0,0,0,0))
    px = img.load()
    c = D/2
    for y in range(D):
        for x in range(D):
            r = math.hypot(x-c, y-c)/(D/2)
            if r >= 1: continue
            # alpha falloff: bright core, soft tail
            a = max(0.0, 1.0 - r)
            a = a*a*0.95
            px[x, y] = (SUN[0], SUN[1], SUN[2], int(a*255))
    img = img.resize((d, d), Image.LANCZOS)
    save(img, 'sun.png')

# ---- hill: rolling-bump silhouette, flat bottom, transparent ----
def gen_hill(name, color, w=2400, h=600, bumps=5, amp=0.62, base_frac=0.62, seed=0.0):
    W, H = w*SS, h*SS
    img = Image.new('RGBA', (W, H), (0,0,0,0))
    d = ImageDraw.Draw(img)
    baseline = int(H*base_frac)
    # solid fill below baseline
    d.rectangle([0, baseline, W, H], fill=(*color, 255))
    # overlapping big circles forming rolling humps along the baseline
    step = W/(bumps)
    for i in range(bumps+1):
        cx = int(i*step + seed*step)
        r = int(H*amp*(0.85 + 0.30*math.sin(i*1.3 + seed*4)))
        cy = baseline + int(H*0.10)  # centers slightly below baseline so a dome pokes up
        d.ellipse([cx-r, cy-r, cx+r, cy+r], fill=(*color, 255))
    img = img.resize((w, h), Image.LANCZOS)
    save(img, name)

# ---- ground: grassy strip, wavy top + lighter grass edge, transparent above ----
def gen_ground(w=2400, h=460, waves=9, wave_amp=0.05):
    W, H = w*SS, h*SS
    img = Image.new('RGBA', (W, H), (0,0,0,0))
    d = ImageDraw.Draw(img)
    top_mid = int(H*0.16)
    a = int(H*wave_amp)
    # build top contour as a gentle sine wave
    pts = []
    for x in range(0, W+1, 6):
        y = top_mid + int(a*math.sin(x/W*math.pi*2*waves))
        pts.append((x, y))
    poly = pts + [(W, H), (0, H)]
    # vertical gradient fill, masked by the wavy polygon
    grad = np.zeros((H, W, 4), dtype=np.uint8)
    for y in range(H):
        c = lerp(GROUND_TOP, GROUND_BOT, y/(H-1))
        grad[y, :, 0:3] = c; grad[y, :, 3] = 255
    grad_img = Image.fromarray(grad, 'RGBA')
    mask = Image.new('L', (W, H), 0)
    ImageDraw.Draw(mask).polygon(poly, fill=255)
    img.paste(grad_img, (0, 0), mask)
    # lighter grass highlight band hugging the wavy top
    edge = int(H*0.085)
    band = [(x, y) for x, y in pts] + [(x, y+edge) for x, y in reversed(pts)]
    d.polygon(band, fill=(*GROUND_EDGE, 255))
    img = img.resize((w, h), Image.LANCZOS)
    save(img, 'ground.png')

# ---- cloud: soft rounded puff, transparent white (fully inside the texture) ----
def gen_cloud(w=520, h=300):
    W, H = w*SS, h*SS
    img = Image.new('RGBA', (W, H), (0,0,0,0))
    d = ImageDraw.Draw(img)
    white = (255,255,255,255)
    # Everything stays within [top_pad, base] so the puff is never clipped by the
    # texture edge — base sits at 0.82*H, leaving a soft bottom margin.
    base = int(H*0.82)
    # gently rounded base slab (soft, slightly rounded bottom — not a hard cut)
    d.rounded_rectangle([int(W*0.15), int(H*0.48), int(W*0.85), base],
                        radius=int(H*0.16), fill=white)
    # fluffy top lobes, all kept above `base`
    lobes = [(0.28,0.54,0.22),(0.46,0.38,0.28),(0.64,0.48,0.24),(0.79,0.58,0.18)]
    for fx,fy,fr in lobes:
        cx, ccy, r = int(W*fx), int(H*fy), int(H*fr)
        d.ellipse([cx-r, ccy-r, cx+r, ccy+r], fill=white)
    img = img.filter(ImageFilter.GaussianBlur(W*0.004))
    img = img.resize((w, h), Image.LANCZOS)
    save(img, 'cloud.png')

if __name__ == '__main__':
    gen_sky()
    gen_sun()
    gen_hill('hill_far.png', HILL_FAR, h=560, bumps=4, amp=0.55, base_frac=0.66, seed=0.0)
    gen_hill('hill_near.png', HILL_NEAR, h=680, bumps=5, amp=0.66, base_frac=0.60, seed=0.5)
    gen_ground()
    gen_cloud()
    print("done ->", os.path.abspath(OUT))
