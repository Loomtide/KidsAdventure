#!/usr/bin/env python3
"""Generate HUD sprites: rounded white card (9-slice), basket icon, star (tintable), mascot."""
import os, math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Art", "hud")
os.makedirs(OUT, exist_ok=True)
SS = 2

def hx(h):
    h=h.lstrip('#'); return tuple(int(h[i:i+2],16) for i in (0,2,4))
INK=hx('46365e'); GOLD=hx('ffce4f'); BASKET=hx('e7a86a'); BASKET_D=hx('caa05f')
LEAF=hx('54cf7d'); BIRD=hx('ffc24a'); BIRD_D=hx('f5a623'); BEAK=hx('ff8a3d'); CHEEK=hx('ff9aa0')

def save(img,name):
    img.save(os.path.join(OUT,name)); print("wrote",name,img.size)

def rounded_rect_mask(W,H,r):
    m=Image.new('L',(W,H),0)
    ImageDraw.Draw(m).rounded_rectangle([0,0,W-1,H-1],radius=r,fill=255)
    return m

def gen_card(size=160, pad=4, radius=30):
    """White rounded card with a thin, soft drop shadow and symmetric margins.

    The white fill spans nearly the whole texture (only a `pad`-px margin for the soft
    shadow), so when the Image is 9-sliced (border = pad+radius) and stretched, the white
    card envelopes its content with even padding and fills >70% of any reasonable rect.
    9-slice border to set in the .meta = pad + radius (== 34)."""
    W=H=size*SS; p=pad*SS; r=radius*SS
    inner=[p,p,W-1-p,H-1-p]
    # soft drop shadow, kept inside the thin margin (slight downward offset for lift)
    sh=Image.new('RGBA',(W,H),(0,0,0,0))
    ImageDraw.Draw(sh).rounded_rectangle([inner[0],inner[1]+2*SS,inner[2],inner[3]+2*SS],radius=r,fill=(70,40,110,85))
    sh=sh.filter(ImageFilter.GaussianBlur(2*SS))
    img=sh
    # white card
    card=Image.new('RGBA',(W,H),(0,0,0,0))
    d=ImageDraw.Draw(card)
    d.rounded_rectangle(inner,radius=r,fill=(255,255,255,255))
    img=Image.alpha_composite(img,card)
    img=img.resize((size,size),Image.LANCZOS)
    save(img,'card_white.png')

def gen_star(size=120):
    W=H=size*SS
    img=Image.new('RGBA',(W,H),(0,0,0,0))
    d=ImageDraw.Draw(img)
    cx=cy=W/2; R=W*0.46; r=R*0.46
    pts=[]
    for k in range(5):
        a=math.radians(-90+k*72); pts.append((cx+math.cos(a)*R,cy+math.sin(a)*R))
        a2=math.radians(-90+k*72+36); pts.append((cx+math.cos(a2)*r,cy+math.sin(a2)*r))
    # white star (tinted at runtime via Image.color), with a faint inner for depth
    d.polygon(pts,fill=(255,255,255,255))
    img=img.resize((size,size),Image.LANCZOS)
    save(img,'star_white.png')

def gen_basket(w=120,h=104):
    W,H=w*SS,h*SS
    img=Image.new('RGBA',(W,H),(0,0,0,0))
    d=ImageDraw.Draw(img)
    # body: trapezoid-ish rounded
    top=int(H*0.28)
    d.rounded_rectangle([int(W*0.10),top,int(W*0.90),int(H*0.92)],radius=int(W*0.14),fill=(*BASKET,255))
    # weave lines
    for fx in (0.28,0.46,0.64,0.82):
        d.line([(int(W*fx),top+6*SS),(int(W*(fx-0.04)),int(H*0.9))],fill=(*BASKET_D,255),width=int(W*0.03))
    d.rectangle([int(W*0.10),top, int(W*0.90),top+int(H*0.10)],fill=(*BASKET_D,255))
    # rim
    d.rounded_rectangle([int(W*0.05),int(H*0.18),int(W*0.95),int(H*0.34)],radius=int(H*0.08),fill=(*BASKET_D,255))
    # an apple peeking out
    ax,ay,ar=int(W*0.5),int(H*0.20),int(W*0.16)
    d.ellipse([ax-ar,ay-ar,ax+ar,ay+ar],fill=hx('ff5d72'))
    d.ellipse([ax-ar*0.4,ay-ar*0.5,ax-ar*0.05,ay-ar*0.1],fill=(255,255,255,180))
    img=img.resize((w,h),Image.LANCZOS)
    save(img,'basket.png')

def gen_mascot(size=200):
    W=H=size*SS
    img=Image.new('RGBA',(W,H),(0,0,0,0))
    d=ImageDraw.Draw(img)
    cx,cy=W*0.5,H*0.56; r=W*0.40
    # white outline
    d.ellipse([cx-r-8*SS,cy-r-8*SS,cx+r+8*SS,cy+r+8*SS],fill=(255,255,255,255))
    # body (radial-ish: just two tone)
    d.ellipse([cx-r,cy-r,cx+r,cy+r],fill=(*BIRD,255))
    d.ellipse([cx-r,cy-r*0.2,cx+r,cy+r],fill=(*BIRD,255))
    d.pieslice([cx-r,cy-r,cx+r,cy+r],20,160,fill=(*BIRD_D,40))
    # leaf sprout on top
    leaf=Image.new('RGBA',(W,H),(0,0,0,0))
    ld=ImageDraw.Draw(leaf)
    ld.ellipse([cx-W*0.02,cy-r-W*0.20,cx+W*0.14,cy-r-W*0.02],fill=(*LEAF,255))
    leaf=leaf.rotate(-18,center=(cx,cy-r),resample=Image.BICUBIC)
    img.alpha_composite(leaf)
    img.alpha_composite(Image.new('RGBA',(W,H),(0,0,0,0)))
    d=ImageDraw.Draw(img)
    # eyes
    for ex in (cx-r*0.34,cx+r*0.34):
        d.ellipse([ex-r*0.17,cy-r*0.28,ex+r*0.17,cy+r*0.06],fill=(70,54,94,255))
        d.ellipse([ex-r*0.02,cy-r*0.24,ex+r*0.10,cy-r*0.10],fill=(255,255,255,255))
    # beak
    d.polygon([(cx-r*0.13,cy+r*0.16),(cx+r*0.13,cy+r*0.16),(cx,cy+r*0.40)],fill=(*BEAK,255))
    # cheeks
    for ex in (cx-r*0.58,cx+r*0.58):
        d.ellipse([ex-r*0.13,cy+r*0.10,ex+r*0.13,cy+r*0.30],fill=(*CHEEK,200))
    img=img.resize((size,size),Image.LANCZOS)
    save(img,'mascot.png')

if __name__=='__main__':
    gen_card(); gen_star(); gen_basket(); gen_mascot()
    print("done")
