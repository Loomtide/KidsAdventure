#!/usr/bin/env python3
"""Generate tap-feel FX assets: gold sparkle, '+1' float, and a pop SFX."""
import os, math, wave, struct
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import numpy as np

ART = os.path.join(os.path.dirname(__file__), "..", "Assets", "Art", "fx")
AUD = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio")
os.makedirs(ART, exist_ok=True); os.makedirs(AUD, exist_ok=True)
SS = 2
ROUND_FONT = "/System/Library/Fonts/Supplemental/Arial Rounded Bold.ttf"

def hx(h):
    h=h.lstrip('#'); return tuple(int(h[i:i+2],16) for i in (0,2,4))
GOLD=hx('ffce4f'); PLUS=hx('ff7aa0')

def save(img,name):
    img.save(os.path.join(ART,name)); print("wrote",name,img.size)

def gen_sparkle(size=96):
    W=H=size*SS
    img=Image.new('RGBA',(W,H),(0,0,0,0))
    d=ImageDraw.Draw(img)
    cx=cy=W/2
    # 4-point star (sparkle) via two crossing tapered diamonds + glow
    glow=Image.new('RGBA',(W,H),(0,0,0,0))
    ImageDraw.Draw(glow).ellipse([cx-W*0.30,cy-H*0.30,cx+W*0.30,cy+H*0.30],fill=(*GOLD,120))
    glow=glow.filter(ImageFilter.GaussianBlur(W*0.05))
    img.alpha_composite(glow)
    def star(rl,rs,col):
        pts=[]
        for k in range(4):
            a=math.radians(k*90)
            pts.append((cx+math.cos(a)*rl, cy+math.sin(a)*rl))
            a2=math.radians(k*90+45)
            pts.append((cx+math.cos(a2)*rs, cy+math.sin(a2)*rs))
        d.polygon(pts,fill=col)
    star(W*0.46,W*0.10,(255,255,255,255))
    star(W*0.40,W*0.085,(*GOLD,255))
    img=img.resize((size,size),Image.LANCZOS)
    save(img,'sparkle.png')

def gen_plus_one(w=240,h=180):
    W,H=w*SS,h*SS
    img=Image.new('RGBA',(W,H),(0,0,0,0))
    d=ImageDraw.Draw(img)
    font=ImageFont.truetype(ROUND_FONT,int(H*0.72))
    text="+1"
    bbox=d.textbbox((0,0),text,font=font,stroke_width=int(H*0.10))
    tw,th=bbox[2]-bbox[0],bbox[3]-bbox[1]
    x=(W-tw)/2-bbox[0]; y=(H-th)/2-bbox[1]
    # thick white outline + pink fill, soft drop shadow
    d.text((x,y+H*0.04),text,font=font,fill=(120,40,90,70),stroke_width=int(H*0.10),stroke_fill=(120,40,90,70))
    d.text((x,y),text,font=font,fill=(*PLUS,255),stroke_width=int(H*0.10),stroke_fill=(255,255,255,255))
    img=img.resize((w,h),Image.LANCZOS)
    save(img,'plus_one.png')

def gen_check(size=140):
    W=H=size*SS
    img=Image.new('RGBA',(W,H),(0,0,0,0))
    d=ImageDraw.Draw(img)
    cx=cy=W/2; r=W*0.40
    GREEN=hx('3fd07a')
    d.ellipse([cx-r-W*0.05,cy-r-W*0.05,cx+r+W*0.05,cy+r+W*0.05],fill=(255,255,255,255))  # white ring
    d.ellipse([cx-r,cy-r,cx+r,cy+r],fill=(*GREEN,255))
    # check stroke
    lw=int(W*0.10)
    p1=(cx-r*0.42,cy+r*0.02); p2=(cx-r*0.08,cy+r*0.38); p3=(cx+r*0.46,cy-r*0.34)
    d.line([p1,p2],fill=(255,255,255,255),width=lw)
    d.line([p2,p3],fill=(255,255,255,255),width=lw)
    d.ellipse([p1[0]-lw/2,p1[1]-lw/2,p1[0]+lw/2,p1[1]+lw/2],fill=(255,255,255,255))
    d.ellipse([p2[0]-lw/2,p2[1]-lw/2,p2[0]+lw/2,p2[1]+lw/2],fill=(255,255,255,255))
    d.ellipse([p3[0]-lw/2,p3[1]-lw/2,p3[0]+lw/2,p3[1]+lw/2],fill=(255,255,255,255))
    img=img.resize((size,size),Image.LANCZOS)
    save(img,'check_badge.png')

def gen_pop_wav(path, sr=44100):
    dur=0.12
    n=int(sr*dur)
    t=np.linspace(0,dur,n,endpoint=False)
    # rising pop: freq 520 -> 1150 Hz, fast exp decay envelope
    f=520+ (1150-520)*(t/dur)
    phase=2*np.pi*np.cumsum(f)/sr
    tone=np.sin(phase)
    env=np.exp(-t*34)
    # tiny noise transient at attack
    noise=(np.random.RandomState(7).randn(n))*np.exp(-t*180)*0.25
    sig=(tone*env+noise)
    sig=sig/np.max(np.abs(sig))*0.9
    pcm=(sig*32767).astype(np.int16)
    with wave.open(path,'w') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(sr)
        w.writeframes(pcm.tobytes())
    print("wrote",os.path.basename(path),f"{dur*1000:.0f}ms")

if __name__=='__main__':
    gen_sparkle()
    gen_plus_one()
    gen_check()
    gen_pop_wav(os.path.join(AUD,'pop.wav'))
    print("done")
