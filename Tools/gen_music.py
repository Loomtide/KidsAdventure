#!/usr/bin/env python3
"""Generate a gentle looping music bed + a completion fanfare for Count the Fruits."""
import os, wave
import numpy as np

AUD = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio")
os.makedirs(AUD, exist_ok=True)
SR = 44100

def note(freq):
    return freq

# note frequencies
A4=440.0
def n(name):
    names={'C':-9,'C#':-8,'D':-7,'D#':-6,'E':-5,'F':-4,'F#':-3,'G':-2,'G#':-1,
           'A':0,'A#':1,'B':2}
    # name like 'C4'
    p=name[:-1]; octv=int(name[-1])
    semis=names[p]+(octv-4)*12
    return A4*(2**(semis/12))

def tone(freq, dur, kind='sine', decay=0.0, attack=0.005):
    t=np.linspace(0,dur,int(SR*dur),endpoint=False)
    if kind=='sine': w=np.sin(2*np.pi*freq*t)
    elif kind=='tri': w=2*np.abs(2*(t*freq-np.floor(t*freq+0.5)))-1
    else: w=np.sin(2*np.pi*freq*t)
    env=np.ones_like(t)
    a=int(SR*attack)
    if a>0: env[:a]=np.linspace(0,1,a)
    if decay>0: env*=np.exp(-t*decay)
    else:
        r=int(SR*0.02); env[-r:]*=np.linspace(1,0,r)  # tiny release
    return w*env

def write_wav(path, sig):
    sig=np.clip(sig,-1,1)
    pcm=(sig*32767).astype(np.int16)
    with wave.open(path,'w') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes(pcm.tobytes())
    print("wrote", os.path.basename(path), f"{len(sig)/SR:.1f}s")

def pad_chord(freqs, dur):
    """Soft swelling pad: attack in, release out -> seam-safe per bar."""
    seg=np.zeros(int(SR*dur))
    t=np.linspace(0,dur,len(seg),endpoint=False)
    env=np.sin(np.pi*t/dur)**1.2  # swell up then down to 0 at both ends
    for f in freqs:
        seg+=np.sin(2*np.pi*f*t)
    seg/=len(freqs)
    return seg*env

# ---- percussion + mallet voices for the bouncy bed ----
def kick(dur=0.20):
    t=np.linspace(0,dur,int(SR*dur),endpoint=False)
    f=140*np.exp(-t*34)+50            # punchy pitch drop
    ph=2*np.pi*np.cumsum(f)/SR
    return np.sin(ph)*np.exp(-t*9)

def hat(dur=0.06):
    t=np.linspace(0,dur,int(SR*dur),endpoint=False)
    return (np.random.rand(len(t))*2-1)*np.exp(-t*95)

def snare(dur=0.18):
    t=np.linspace(0,dur,int(SR*dur),endpoint=False)
    body=(np.random.rand(len(t))*2-1)*np.exp(-t*20)*0.7
    tone_=np.sin(2*np.pi*200*t)*np.exp(-t*23)*0.4
    return body+tone_

def pluck(freq, dur):
    """Marimba/xylophone-ish mallet: bright harmonics, quick percussive decay."""
    t=np.linspace(0,dur,int(SR*dur),endpoint=False)
    w=(np.sin(2*np.pi*freq*t)
       +0.45*np.sin(2*np.pi*2*freq*t)
       +0.20*np.sin(2*np.pi*3*freq*t))
    return w*np.exp(-t*6.5)*(1-np.exp(-t*500))   # soft mallet attack + decay

def gen_music_bed(path):
    """Upbeat, bouncy kids' loop: four-on-the-floor-ish drums, a plucky walking
    bass, off-beat chord stabs, a warm pad, and a catchy pentatonic marimba melody
    over C-G-Am-F. Events wrap around the loop end so it repeats seamlessly."""
    np.random.seed(7)
    beat=0.46; bar=beat*4; nbars=8
    out=np.zeros(int(SR*bar*nbars))
    L=len(out)
    def place(sig, at, g=1.0):
        i=int(SR*at)
        idx=(np.arange(len(sig))+i)%L     # wrap-around -> seamless loop tails
        np.add.at(out, idx, sig*g)

    roots =['C2','G2','A2','F2']*2
    chords=[['C4','E4','G4'],['B3','D4','G4'],['A3','C4','E4'],['A3','C4','F4']]*2
    pads  =[['C3','E3','G3'],['G2','B3','D3'],['A2','C3','E3'],['F2','A2','C3']]*2
    # catchy pentatonic melody, one phrase per bar: (beat, note, duration-in-beats)
    mel=[
      [(0,'G4',.5),(.5,'A4',.5),(1,'C5',1),(2,'A4',.5),(2.5,'C5',.5),(3,'G4',.5),(3.5,'A4',.5)],
      [(0,'B4',.5),(.5,'D5',.5),(1,'G5',1),(2,'D5',.5),(2.5,'B4',.5),(3,'D5',1)],
      [(0,'C5',.5),(.5,'E5',.5),(1,'A5',1),(2,'E5',.5),(2.5,'C5',.5),(3,'A4',1)],
      [(0,'C5',.5),(.5,'A4',.5),(1,'F5',1),(2,'C5',.5),(2.5,'A4',.5),(3,'C5',1)],
    ]*2
    for b in range(nbars):
        t0=b*bar
        # warm but quiet pad for glue
        place(pad_chord([n(x) for x in pads[b]], bar), t0, 0.12)
        # drums
        for beati in range(4):
            bt=t0+beati*beat
            if beati in (0,2): place(kick(),  bt,           0.85)
            if beati in (1,3): place(snare(), bt,           0.42)
            place(hat(), bt,            0.28)            # on-beat
            place(hat(), bt+beat*0.5,   0.42)            # accented off-beat
        # bouncy bass: staccato eighths, alternating root / octave
        rf=n(roots[b])
        for e in range(8):
            f=rf if e%2==0 else rf*2
            place(pluck(f, beat*0.45), t0+e*beat*0.5, 0.5)
        # off-beat chord stabs (the "and" of 2 and 4)
        for cf in chords[b]:
            place(pluck(n(cf), beat*0.5), t0+beat*1.5, 0.16)
            place(pluck(n(cf), beat*0.5), t0+beat*3.5, 0.16)
        # lead melody
        for (bo,name,du) in mel[b]:
            place(pluck(n(name), beat*du*0.95), t0+bo*beat, 0.75)
    out=out/np.max(np.abs(out))*0.72
    write_wav(path, out)

def gen_fanfare(path):
    seq=[('C4',0.12),('E4',0.12),('G4',0.12),('C5',0.16),('E5',0.16),('G5',0.30)]
    out=np.array([])
    for name,dur in seq:
        s=tone(n(name),dur,'tri',decay=4.0,attack=0.004)*0.8
        # add a soft fifth for richness
        s=s+tone(n(name)*1.5,dur,'sine',decay=5.0)*0.2
        out=np.concatenate([out,s])
    out=out/np.max(np.abs(out))*0.85
    write_wav(path, out)

def gen_start(path):
    """A short, cheerful 'let's go!' jingle for the Play button — three quick
    ascending notes with a sparkle, distinct from (and shorter than) the end fanfare."""
    seq=[('C5',0.09),('E5',0.09),('G5',0.14)]
    out=np.array([])
    for name,dur in seq:
        s=tone(n(name),dur,'tri',decay=5.0,attack=0.004)*0.7
        s=s+tone(n(name)*2,dur,'sine',decay=7.0)*0.16  # bright octave sparkle
        out=np.concatenate([out,s])
    out=out/np.max(np.abs(out))*0.82
    write_wav(path, out)

if __name__=='__main__':
    gen_music_bed(os.path.join(AUD,'music_bed.wav'))
    gen_fanfare(os.path.join(AUD,'fanfare.wav'))
    gen_start(os.path.join(AUD,'start.wav'))
    print("done")
