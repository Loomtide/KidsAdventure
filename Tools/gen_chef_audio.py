#!/usr/bin/env python3
"""Generate Kids Chef audio: a cozy kitchen music loop plus the cooking SFX
(pour, crack, sprinkle, whisk, spray, sizzle, ding, plop). Same seam-safe
wrap-around loop technique as gen_home_music.py."""
import os, wave, math
import numpy as np

AUD = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio")
os.makedirs(AUD, exist_ok=True)
SR = 44100

A4 = 440.0
def n(name):
    names = {'C': -9, 'C#': -8, 'D': -7, 'D#': -6, 'E': -5, 'F': -4, 'F#': -3, 'G': -2, 'G#': -1,
             'A': 0, 'A#': 1, 'B': 2}
    p = name[:-1]; octv = int(name[-1])
    return A4 * (2 ** ((names[p] + (octv - 4) * 12) / 12))

def write_wav(path, sig):
    sig = np.clip(sig, -1, 1)
    pcm = (sig * 32767).astype(np.int16)
    with wave.open(path, 'w') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes(pcm.tobytes())
    print("wrote", os.path.basename(path), f"{len(sig)/SR:.2f}s")

def t_axis(dur):
    return np.linspace(0, dur, int(SR * dur), endpoint=False)

def lowpass(sig, alpha):
    out = np.empty_like(sig)
    acc = 0.0
    for i, s in enumerate(sig):
        acc += alpha * (s - acc)
        out[i] = acc
    return out

# ------------------------------------------------------------------ music voices
def pluck(freq, dur):
    t = t_axis(dur)
    w = (np.sin(2 * np.pi * freq * t)
         + 0.45 * np.sin(2 * np.pi * 2 * freq * t)
         + 0.20 * np.sin(2 * np.pi * 3 * freq * t))
    return w * np.exp(-t * 6.5) * (1 - np.exp(-t * 500))

def marimba(freq, dur):
    t = t_axis(dur)
    w = np.sin(2 * np.pi * freq * t) + 0.35 * np.sin(2 * np.pi * freq * 4.0 * t) * np.exp(-t * 18)
    return w * np.exp(-t * 5.0) * (1 - np.exp(-t * 600))

def glock(freq, dur):
    t = t_axis(dur)
    w = np.sin(2 * np.pi * freq * t) + 0.30 * np.sin(2 * np.pi * freq * 2.76 * t)
    return w * np.exp(-t * 3.2) * (1 - np.exp(-t * 700))

def kick(dur=0.20):
    t = t_axis(dur)
    f = 130 * np.exp(-t * 32) + 48
    return np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t * 10)

def shaker(dur=0.09):
    t = t_axis(dur)
    return (np.random.rand(len(t)) * 2 - 1) * np.exp(-t * 50) * np.minimum(t * 110, 1)

def pad_chord(freqs, dur):
    t = t_axis(dur)
    env = np.sin(np.pi * t / dur) ** 1.2
    seg = sum(np.sin(2 * np.pi * f * t) for f in freqs) / len(freqs)
    return seg * env

def gen_chef_theme(path):
    """8 bars in C major at a cozy ~112bpm: marimba comping, soft kick + shaker,
    a simple hummable tune with glockenspiel echo. Wrap-around tails for a clean loop."""
    np.random.seed(21)
    beat = 0.5357; bar = beat * 4; nbars = 8
    out = np.zeros(int(SR * bar * nbars))
    L = len(out)
    def place(sig, at, g=1.0):
        i = int(SR * at)
        idx = (np.arange(len(sig)) + i) % L
        np.add.at(out, idx, sig * g)

    roots  = ['C3', 'A2', 'F2', 'G2'] * 2                      # C  Am  F  G
    pads = [['C3', 'E3', 'G3'], ['A2', 'C3', 'E3'], ['F2', 'A2', 'C3'], ['G2', 'B2', 'D3']] * 2
    mel = [
        [(0, 'E4', 1), (1, 'G4', .5), (1.5, 'E4', .5), (2, 'C4', 1), (3, 'D4', 1)],
        [(0, 'E4', .5), (.5, 'D4', .5), (1, 'C4', 1), (2, 'A3', 1), (3, 'C4', 1)],
        [(0, 'F4', 1), (1, 'A4', .5), (1.5, 'F4', .5), (2, 'C4', 1.5)],
        [(0, 'D4', .5), (.5, 'E4', .5), (1, 'D4', 1), (2, 'B3', 1), (3, 'G3', 1)],
        [(0, 'E4', 1), (1, 'G4', .5), (1.5, 'C5', .5), (2, 'G4', 1), (3, 'E4', 1)],
        [(0, 'A4', .5), (.5, 'G4', .5), (1, 'E4', 1), (2, 'C4', 1.5)],
        [(0, 'F4', .5), (.5, 'G4', .5), (1, 'A4', 1), (2, 'F4', .5), (2.5, 'D4', .5), (3, 'F4', 1)],
        [(0, 'E4', .5), (.5, 'D4', .5), (1, 'E4', 1), (2, 'C4', 2)],
    ]
    for b in range(nbars):
        t0 = b * bar
        place(pad_chord([n(x) for x in pads[b]], bar), t0, 0.10)
        for bi in range(4):
            bt = t0 + bi * beat
            if bi in (0, 2): place(kick(), bt, 0.65)
            place(shaker(), bt + beat * 0.5, 0.28)
        place(pluck(n(roots[b]), beat * 0.8), t0, 0.5)
        place(pluck(n(roots[b]), beat * 0.6), t0 + beat * 2, 0.42)
        for cf in pads[b]:
            place(marimba(n(cf) * 2, beat * 0.5), t0 + beat * 1.5, 0.13)
            place(marimba(n(cf) * 2, beat * 0.5), t0 + beat * 3.5, 0.13)
        for (bo, name, du) in mel[b]:
            place(marimba(n(name), beat * du * 0.95), t0 + bo * beat, 0.72)
            place(glock(n(name) * 2, beat * du * 0.9), t0 + bo * beat + beat * 0.5, 0.09)
    out = out / np.max(np.abs(out)) * 0.65
    write_wav(path, out)

# ------------------------------------------------------------------ SFX
def gen_pour(path):
    """Liquid pour: lowpassed noise swell with bubbly sine blups."""
    np.random.seed(3)
    dur = 0.8
    t = t_axis(dur)
    noise = lowpass(np.random.rand(len(t)) * 2 - 1, 0.08)
    env = np.sin(np.pi * np.minimum(t / dur, 1)) ** 0.8
    sig = noise * env * 1.6
    for at, f in [(0.12, 420), (0.3, 350), (0.46, 480), (0.62, 390)]:
        bt = t_axis(0.07)
        bl = np.sin(2 * np.pi * (f + 260 * bt / 0.07) * bt) * np.exp(-bt * 40) * 0.4
        i = int(at * SR)
        sig[i:i + len(bl)] += bl
    write_wav(path, sig * 0.8)

def gen_crack(path):
    """Egg crack: two snappy noise ticks."""
    np.random.seed(4)
    dur = 0.25
    sig = np.zeros(int(SR * dur))
    for at, g in [(0.0, 1.0), (0.07, 0.7)]:
        t = t_axis(0.05)
        burst = (np.random.rand(len(t)) * 2 - 1) * np.exp(-t * 140) * g
        i = int(at * SR)
        sig[i:i + len(burst)] += burst
    write_wav(path, sig * 0.85)

def gen_sprinkle(path):
    """Sugar sprinkle: a flurry of tiny bright ticks."""
    np.random.seed(5)
    dur = 0.45
    sig = np.zeros(int(SR * dur))
    for k in range(22):
        at = 0.02 + k * 0.018 + np.random.rand() * 0.012
        t = t_axis(0.025)
        f = np.random.uniform(2800, 5200)
        tick = np.sin(2 * np.pi * f * t) * np.exp(-t * 320) * 0.5
        i = int(at * SR)
        if i + len(tick) < len(sig): sig[i:i + len(tick)] += tick
    write_wav(path, sig * 0.8)

def gen_whisk(path):
    """Whisk: three quick swishes of shaped noise."""
    np.random.seed(6)
    dur = 0.66
    sig = np.zeros(int(SR * dur))
    for at in (0.0, 0.21, 0.42):
        t = t_axis(0.18)
        sw = (np.random.rand(len(t)) * 2 - 1)
        sw = sw - lowpass(sw, 0.18)           # highpass-ish
        sw *= np.sin(np.pi * t / 0.18) ** 1.5
        i = int(at * SR)
        sig[i:i + len(sw)] += sw * 0.55
    write_wav(path, sig)

def gen_spray(path):
    """Oil spray: short airy hiss."""
    np.random.seed(7)
    t = t_axis(0.32)
    noise = np.random.rand(len(t)) * 2 - 1
    noise = noise - lowpass(noise, 0.3)
    env = np.minimum(t * 60, 1) * np.exp(-t * 9)
    write_wav(path, noise * env * 0.7)

def gen_sizzle(path):
    """Cooking sizzle: crackly noise bed, ~3s, gentle fade in/out."""
    np.random.seed(8)
    dur = 3.0
    t = t_axis(dur)
    bed = (np.random.rand(len(t)) * 2 - 1)
    bed = bed - lowpass(bed, 0.25)
    bed *= 0.30
    # random crackle pops
    for _ in range(160):
        at = np.random.rand() * (dur - 0.02)
        ct = t_axis(0.012)
        pop = (np.random.rand(len(ct)) * 2 - 1) * np.exp(-ct * 500) * np.random.uniform(0.2, 0.6)
        i = int(at * SR)
        bed[i:i + len(pop)] += pop
    env = np.minimum(t * 8, 1) * np.minimum((dur - t) * 3, 1)
    write_wav(path, bed * env * 0.8)

def gen_ding(path):
    """Done ding: bright bell with a long ring."""
    t = t_axis(1.3)
    f = n('E6')
    w = np.sin(2 * np.pi * f * t) + 0.35 * np.sin(2 * np.pi * f * 2.76 * t)
    sig = w * np.exp(-t * 3.0) * (1 - np.exp(-t * 900))
    write_wav(path, sig * 0.6)

def gen_plop(path):
    """Topping plop: quick pitch-drop blip."""
    t = t_axis(0.16)
    f = 520 * np.exp(-t * 16) + 160
    sig = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t * 18)
    write_wav(path, sig * 0.7)

if __name__ == '__main__':
    gen_chef_theme(os.path.join(AUD, 'chef_theme.wav'))
    gen_pour(os.path.join(AUD, 'pour.wav'))
    gen_crack(os.path.join(AUD, 'crack.wav'))
    gen_sprinkle(os.path.join(AUD, 'sprinkle.wav'))
    gen_whisk(os.path.join(AUD, 'whisk.wav'))
    gen_spray(os.path.join(AUD, 'spray.wav'))
    gen_sizzle(os.path.join(AUD, 'sizzle.wav'))
    gen_ding(os.path.join(AUD, 'ding.wav'))
    gen_plop(os.path.join(AUD, 'plop.wav'))
    print("done")
