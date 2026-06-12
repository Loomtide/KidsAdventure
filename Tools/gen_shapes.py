#!/usr/bin/env python3
"""Generate Shape Match shape sprites — circle / square / triangle / star.

Two sets, soft rounded vector to match the frozen Shape Match hero shot:
  <shape>_white.png   white tile shape (answer buttons; also tinted gold/grey for HUD stars)
  <shape>_<accent>.png glossy signature-coloured shape with a white outline (the big target)

Also writes Unity .meta sidecars so each PNG imports as a Sprite (PPU 200).
"""
import os, math, hashlib
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Art", "shapematch")
os.makedirs(OUT, exist_ok=True)
SS = 2  # supersample

def hx(h):
    h = h.lstrip('#'); return tuple(int(h[i:i+2], 16) for i in (0, 2, 4))

# signature accent per shape (target colour); answers stay white so matching is by SHAPE
ACCENT = {
    'circle':   ('coral', hx('ff5d72')),
    'square':   ('blue',  hx('59c1ff')),
    'triangle': ('mint',  hx('43d6a6')),
    'star':     ('gold',  hx('ffce4f')),
}

def lerp(a, b, t): return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))
def lighten(c, t): return lerp(c, (255, 255, 255), t)
def darken(c, t):  return lerp(c, (0, 0, 0), t)

def save(img, name):
    img.save(os.path.join(OUT, name)); print("wrote", name, img.size)

# ---- meta sidecar (textureType=Sprite, PPU 200) -------------------------------
META = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 200
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: {spriteid}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""

def write_meta(pngname):
    base = pngname
    guid = hashlib.md5(("shapematch/" + base).encode()).hexdigest()
    spriteid = hashlib.md5(("sprite/" + base).encode()).hexdigest()[:16] + "0800000000000000"
    with open(os.path.join(OUT, pngname + ".meta"), "w") as f:
        f.write(META.format(guid=guid, spriteid=spriteid))

# ---- shape silhouette drawing -------------------------------------------------
def draw_shape(d, shape, cx, cy, R, fill):
    if shape == 'circle':
        d.ellipse([cx - R, cy - R, cx + R, cy + R], fill=fill)
    elif shape == 'square':
        d.rounded_rectangle([cx - R, cy - R, cx + R, cy + R], radius=R * 0.26, fill=fill)
    elif shape == 'triangle':
        v = [(cx, cy - R), (cx + R * 0.92, cy + R * 0.74), (cx - R * 0.92, cy + R * 0.74)]
        d.polygon(v, fill=fill)
    elif shape == 'star':
        pts = []
        for k in range(5):
            a = math.radians(-90 + k * 72)
            pts.append((cx + math.cos(a) * R, cy + math.sin(a) * R))
            a2 = math.radians(-90 + k * 72 + 36)
            pts.append((cx + math.cos(a2) * R * 0.46, cy + math.sin(a2) * R * 0.46))
        d.polygon(pts, fill=fill)

def alpha_mask(shape, size, r_frac):
    W = H = size * SS
    m = Image.new('L', (W, H), 0)
    d = ImageDraw.Draw(m)
    cx = cy = W / 2.0
    R = (W / 2.0) * r_frac
    draw_shape(d, shape, cx, cy, R, 255)
    # smoothly round the sharp corners of polygon shapes (friendlier for ages 3-5)
    if shape in ('triangle', 'star'):
        rad = W * (0.030 if shape == 'triangle' else 0.022)
        blur = m.filter(ImageFilter.GaussianBlur(rad))
        a = np.asarray(blur, dtype=np.float32)
        a = np.clip((a - 110.0) / 36.0, 0.0, 1.0) * 255.0  # soft re-threshold keeps AA
        m = Image.fromarray(a.astype(np.uint8), 'L')
    return m, (cx, cy, R)

# ---- white tile shape (subtle top->bottom form shade) -------------------------
def gen_white(shape, size=300):
    m, _ = alpha_mask(shape, size, 0.90)
    W = H = size * SS
    ys = np.linspace(0.0, 1.0, H)[:, None]
    top = np.array((255, 255, 255), float)
    bot = np.array((228, 228, 240), float)
    rgb = (top[None, None, :] + (bot - top)[None, None, :] * ys[:, :, None]).astype(np.uint8)
    rgb = np.repeat(rgb, W, axis=1) if rgb.shape[1] == 1 else rgb
    arr = np.zeros((H, W, 4), np.uint8)
    arr[:, :, 0:3] = rgb
    arr[:, :, 3] = np.array(m)
    img = Image.fromarray(arr, 'RGBA').resize((size, size), Image.LANCZOS)
    name = f'{shape}_white.png'
    save(img, name); write_meta(name)

# ---- glossy coloured target shape with white outline --------------------------
def gen_target(shape, size=320):
    accent_name, color = ACCENT[shape]
    W = H = size * SS
    out = Image.new('RGBA', (W, H), (0, 0, 0, 0))

    # white outline = full-size silhouette in white
    halo, _ = alpha_mask(shape, size, 0.96)
    white = Image.new('RGBA', (W, H), (255, 255, 255, 255))
    out.paste(white, (0, 0), halo)

    # glossy radial fill, inset so the white outline shows
    fillmask, (cx, cy, Rf) = alpha_mask(shape, size, 0.82)
    gx, gy = cx - Rf * 0.40, cy - Rf * 0.42
    ys, xs = np.mgrid[0:H, 0:W]
    gd = np.hypot((xs - gx) / (Rf * 1.3), (ys - gy) / (Rf * 1.3))
    t = np.clip(gd, 0.0, 1.0)
    light = np.array(lighten(color, 0.55), float)
    base = np.array(color, float)
    shade = np.array(darken(color, 0.28), float)
    inner = light[None, None, :] + (base - light)[None, None, :] * np.clip(t / 0.5, 0, 1)[:, :, None]
    outer = base[None, None, :] + (shade - base)[None, None, :] * np.clip((t - 0.5) / 0.5, 0, 1)[:, :, None]
    rgb = np.where((t < 0.5)[:, :, None], inner, outer).astype(np.uint8)
    farr = np.zeros((H, W, 4), np.uint8)
    farr[:, :, 0:3] = rgb
    farr[:, :, 3] = np.array(fillmask)
    out.alpha_composite(Image.fromarray(farr, 'RGBA'))

    # specular highlight blob, top-left
    spec = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    sd = ImageDraw.Draw(spec)
    sd.ellipse([cx - Rf * 0.52, cy - Rf * 0.58, cx - Rf * 0.16, cy - Rf * 0.20], fill=(255, 255, 255, 200))
    spec = spec.filter(ImageFilter.GaussianBlur(W * 0.01))
    # clip highlight to the fill silhouette
    spec.putalpha(Image.composite(spec.getchannel('A'), Image.new('L', (W, H), 0), fillmask))
    out.alpha_composite(spec)

    img = out.resize((size, size), Image.LANCZOS)
    name = f'{shape}_{accent_name}.png'
    save(img, name); write_meta(name)

# ---- chunky 3D answer buttons (coral / blue / lavender, matching the hero shot) ----
def gen_button(name, top, bot, lip, w=240, h=204, lip_frac=0.14, radius=46):
    W, H = w * SS, h * SS; r = radius * SS; ow = 7 * SS
    img = Image.new('RGBA', (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(img)
    d.rounded_rectangle([0, 0, W - 1, H - 1], radius=r, fill=(255, 255, 255, 255))           # white outline
    d.rounded_rectangle([ow, ow, W - 1 - ow, H - 1 - ow], radius=r - ow * 0.5, fill=(*lip, 255))  # lip/base
    lip_h = int(H * lip_frac); fy0, fy1 = ow + 2 * SS, H - 1 - ow - lip_h
    face = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    ImageDraw.Draw(face).rounded_rectangle([ow + 2 * SS, fy0, W - 1 - ow - 2 * SS, fy1], radius=r - ow, fill=(255, 255, 255, 255))
    grad = np.zeros((H, W, 4), np.uint8)
    for y in range(H):
        t = max(0.0, min(1.0, (y - fy0) / max(1, (fy1 - fy0))))
        grad[y, :, 0:3] = lerp(top, bot, t); grad[y, :, 3] = 255
    img.paste(Image.fromarray(grad, 'RGBA'), (0, 0), face)
    sheen = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    ImageDraw.Draw(sheen).rounded_rectangle([ow + 6 * SS, fy0 + 3 * SS, W - 1 - ow - 6 * SS, fy0 + int((fy1 - fy0) * 0.34)],
                                            radius=r - ow, fill=(255, 255, 255, 60))
    img.alpha_composite(sheen)
    img = img.resize((w, h), Image.LANCZOS)
    save(img, name); write_meta(name)

if __name__ == '__main__':
    for s in ('circle', 'square', 'triangle', 'star'):
        gen_white(s)
        gen_target(s)
    gen_button('button_coral.png', hx('ff8ea0'), hx('ff5d72'), hx('d83d54'))
    gen_button('button_blue.png',  hx('9fdcff'), hx('59c1ff'), hx('2f93d8'))
    gen_button('button_lav.png',   hx('cdbcff'), hx('b79cff'), hx('7d5fe0'))
    print("done")
