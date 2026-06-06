#!/usr/bin/env python3
"""Generate the glossy apple sprite + soft contact shadow for Count the Fruits.
Soft rounded vector, matches the frozen hero shot apple."""
import os, math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Art", "fruit")
os.makedirs(OUT, exist_ok=True)
SS = 2

def hx(h):
    h = h.lstrip('#'); return tuple(int(h[i:i+2],16) for i in (0,2,4))

APPLE   = hx('ff5d72'); APPLE_SH = hx('e23d57'); GLOSS = hx('ffd0d8')
LEAF    = hx('54cf7d'); LEAF_SH = hx('3fae66'); STEM = hx('8a5a3c')
OUTLINE = (255,255,255)

def save(img, name):
    img.save(os.path.join(OUT, name)); print("wrote", name, img.size)

def radial_body(W, H, cx, cy, rx, ry):
    """Glossy radial-shaded apple body as RGBA array, gloss offset top-left."""
    arr = np.zeros((H, W, 4), dtype=np.uint8)
    gx, gy = cx - rx*0.34, cy - ry*0.40  # gloss center
    for y in range(H):
        for x in range(W):
            nx, ny = (x-cx)/rx, (y-cy)/ry
            d = nx*nx + ny*ny
            if d > 1.0: continue
            # radial gradient from gloss point
            gd = math.hypot((x-gx)/(rx*1.25), (y-gy)/(ry*1.25))
            t = min(1.0, gd)
            if t < 0.5:
                c = tuple(int(GLOSS[i] + (APPLE[i]-GLOSS[i])*(t/0.5)) for i in range(3))
            else:
                c = tuple(int(APPLE[i] + (APPLE_SH[i]-APPLE[i])*((t-0.5)/0.5)) for i in range(3))
            # soft edge alpha
            a = 255 if d < 0.93 else int(255*(1-(d-0.93)/0.07))
            arr[y, x] = (c[0], c[1], c[2], max(0, a))
    return arr

def gen_apple(size=560):
    W = H = size*SS
    img = Image.new('RGBA', (W, H), (0,0,0,0))
    d = ImageDraw.Draw(img)
    cx, cy = W*0.5, H*0.56
    rx, ry = W*0.40, H*0.37
    # white outline halo (drawn as slightly larger body silhouette)
    halo = Image.new('RGBA', (W, H), (0,0,0,0))
    ImageDraw.Draw(halo).ellipse([cx-rx-12*SS, cy-ry-12*SS, cx+rx+12*SS, cy+ry+12*SS], fill=(*OUTLINE,255))
    # top dimple: pinch the silhouette a touch (skip — keep simple round apple)
    img.alpha_composite(halo)
    body = Image.fromarray(radial_body(W, H, cx, cy, rx, ry), 'RGBA')
    img.alpha_composite(body)
    # specular gloss blob (tight crisp white highlight, top-left)
    gloss = Image.new('RGBA', (W, H), (0,0,0,0))
    ImageDraw.Draw(gloss).ellipse([cx-rx*0.56, cy-ry*0.60, cx-rx*0.22, cy-ry*0.14], fill=(255,255,255,210))
    gloss = gloss.filter(ImageFilter.GaussianBlur(W*0.006))
    img.alpha_composite(gloss)
    # tiny secondary sparkle dot
    sp = Image.new('RGBA', (W, H), (0,0,0,0))
    ImageDraw.Draw(sp).ellipse([cx-rx*0.16, cy-ry*0.40, cx-rx*0.06, cy-ry*0.28], fill=(255,255,255,200))
    img.alpha_composite(sp.filter(ImageFilter.GaussianBlur(W*0.003)))
    # stem
    sd = ImageDraw.Draw(img)
    sx = cx - W*0.01
    sd.line([(sx, cy-ry*0.92), (sx-W*0.02, cy-ry*1.18)], fill=STEM, width=int(W*0.022))
    # leaf (teardrop) attached at the stem base
    leaf = Image.new('RGBA', (W, H), (0,0,0,0))
    ld = ImageDraw.Draw(leaf)
    lx, ly = cx + W*0.015, cy - ry*1.02
    lw, lh = W*0.16, H*0.10
    ld.ellipse([lx, ly-lh/2, lx+lw, ly+lh/2], fill=(*LEAF,255))
    leaf = leaf.rotate(-22, center=(lx, ly), resample=Image.BICUBIC)
    # leaf inner shade
    img.alpha_composite(leaf)
    # white outline ring on leaf+stem via re-stroke (skip, halo covers body)
    img = img.resize((size, size), Image.LANCZOS)
    save(img, 'apple.png')

def gen_shadow(w=440, h=150):
    W, H = w*SS, h*SS
    img = Image.new('RGBA', (W, H), (0,0,0,0))
    ImageDraw.Draw(img).ellipse([0,0,W,H], fill=(70,40,100,120))
    img = img.filter(ImageFilter.GaussianBlur(W*0.03))
    img = img.resize((w, h), Image.LANCZOS)
    save(img, 'fruit_shadow.png')

if __name__ == '__main__':
    gen_apple()
    gen_shadow()
    print("done")
