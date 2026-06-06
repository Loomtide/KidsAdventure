#!/usr/bin/env python3
"""Generate chunky 3D answer buttons, confetti piece, and chime/buzz SFX."""
import os, math, wave
from PIL import Image, ImageDraw
import numpy as np

ART = os.path.join(os.path.dirname(__file__), "..", "Assets", "Art", "ui")
AUD = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio")
os.makedirs(ART, exist_ok=True); os.makedirs(AUD, exist_ok=True)
SS = 2

def hx(h):
    h=h.lstrip('#'); return tuple(int(h[i:i+2],16) for i in (0,2,4))

def save(img,name):
    img.save(os.path.join(ART,name)); print("wrote",name,img.size)

def lerp(a,b,t): return tuple(int(a[i]+(b[i]-a[i])*t) for i in range(3))

def gen_button(name, top, bot, lip, w=240, h=204, lip_frac=0.14, radius=44):
    W,H=w*SS,h*SS; r=radius*SS; ow=7*SS
    img=Image.new('RGBA',(W,H),(0,0,0,0))
    d=ImageDraw.Draw(img)
    # white outline (full rounded rect)
    d.rounded_rectangle([0,0,W-1,H-1],radius=r,fill=(255,255,255,255))
    # base / lip color (inset by outline)
    d.rounded_rectangle([ow,ow,W-1-ow,H-1-ow],radius=r-ow*0.5,fill=(*lip,255))
    # face (gradient), sits on top leaving a lip at the bottom
    lip_h=int(H*lip_frac)
    fy0,fy1=ow+2*SS, H-1-ow-lip_h
    face=Image.new('RGBA',(W,H),(0,0,0,0))
    fd=ImageDraw.Draw(face)
    fd.rounded_rectangle([ow+2*SS,fy0,W-1-ow-2*SS,fy1],radius=r-ow,fill=(255,255,255,255))
    # vertical gradient fill clipped to face shape
    grad=np.zeros((H,W,4),dtype=np.uint8)
    for y in range(H):
        t=max(0.0,min(1.0,(y-fy0)/max(1,(fy1-fy0))))
        c=lerp(top,bot,t); grad[y,:,0:3]=c; grad[y,:,3]=255
    gimg=Image.fromarray(grad,'RGBA')
    img.paste(gimg,(0,0),face)
    # soft top sheen
    sheen=Image.new('RGBA',(W,H),(0,0,0,0))
    ImageDraw.Draw(sheen).rounded_rectangle([ow+6*SS,fy0+3*SS,W-1-ow-6*SS,fy0+int((fy1-fy0)*0.34)],
        radius=r-ow,fill=(255,255,255,60))
    img.alpha_composite(sheen)
    img=img.resize((w,h),Image.LANCZOS)
    save(img,name)

def gen_confetti(w=40,h=54):
    W,H=w*SS,h*SS
    img=Image.new('RGBA',(W,H),(0,0,0,0))
    ImageDraw.Draw(img).rounded_rectangle([0,0,W-1,H-1],radius=int(W*0.28),fill=(255,255,255,255))
    img=img.resize((w,h),Image.LANCZOS)
    save(img,'confetti.png')

def write_wav(path,sig,sr=44100):
    sig=sig/max(1e-9,np.max(np.abs(sig)))*0.9
    pcm=(sig*32767).astype(np.int16)
    with wave.open(path,'w') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(sr); w.writeframes(pcm.tobytes())
    print("wrote",os.path.basename(path),f"{len(sig)/sr*1000:.0f}ms")

def tone(freq,dur,sr=44100,decay=8.0,kind='sine'):
    n=int(sr*dur); t=np.linspace(0,dur,n,endpoint=False)
    if kind=='sine': w=np.sin(2*np.pi*freq*t)
    elif kind=='tri': w=2*np.abs(2*(t*freq-np.floor(t*freq+0.5)))-1
    else: w=np.sign(np.sin(2*np.pi*freq*t))
    return w*np.exp(-t*decay)

def gen_chime(path,sr=44100):
    # bright ascending major arpeggio C5 E5 G5 C6
    notes=[523.25,659.25,783.99,1046.5]; seg=0.085; sig=np.array([])
    for i,f in enumerate(notes):
        s=tone(f,seg,sr,decay=6.0,kind='tri')*0.9
        sig=np.concatenate([sig,s])
    # let last ring
    sig=np.concatenate([sig,tone(notes[-1],0.18,sr,decay=5.0,kind='tri')])
    write_wav(path,sig,sr)

def gen_buzz(path,sr=44100):
    # gentle low "try again" buzz: two short low square blips
    seg=0.09
    a=tone(196,seg,sr,decay=14,kind='square')*0.6
    gap=np.zeros(int(sr*0.04))
    b=tone(165,seg,sr,decay=14,kind='square')*0.6
    sig=np.concatenate([a,gap,b])
    write_wav(path,sig,sr)

if __name__=='__main__':
    gen_button('button_peach.png', hx('ffc095'), hx('ffb27b'), hx('e07a3e'))
    gen_button('button_mint.png',  hx('8af3da'), hx('5fe0bd'), hx('2cae8e'))
    gen_button('button_lav.png',   hx('cdbcff'), hx('b79cff'), hx('7d5fe0'))
    gen_confetti()
    gen_chime(os.path.join(AUD,'chime.wav'))
    gen_buzz(os.path.join(AUD,'buzz.wav'))
    print("done")
