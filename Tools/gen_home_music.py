#!/usr/bin/env python3
"""Generate the Kids Adventure home-screen theme: a bright, bouncy, kid-friendly loop.
Same seam-safe wrap-around technique as gen_music.py, but a sunnier tune (F major,
skipping rhythm, glockenspiel sparkle) so the hub feels distinct from in-game music."""
import os, wave
import numpy as np

AUD = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio")
os.makedirs(AUD, exist_ok=True)
SR = 44100

A4 = 440.0
def n(name):
    names = {'C': -9, 'C#': -8, 'D': -7, 'D#': -6, 'E': -5, 'F': -4, 'F#': -3, 'G': -2, 'G#': -1,
             'A': 0, 'A#': 1, 'B': 2}
    p = name[:-1]; octv = int(name[-1])
    semis = names[p] + (octv - 4) * 12
    return A4 * (2 ** (semis / 12))

def write_wav(path, sig):
    sig = np.clip(sig, -1, 1)
    pcm = (sig * 32767).astype(np.int16)
    with wave.open(path, 'w') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes(pcm.tobytes())
    print("wrote", os.path.basename(path), f"{len(sig)/SR:.1f}s")

def pad_chord(freqs, dur):
    seg = np.zeros(int(SR * dur))
    t = np.linspace(0, dur, len(seg), endpoint=False)
    env = np.sin(np.pi * t / dur) ** 1.2
    for f in freqs:
        seg += np.sin(2 * np.pi * f * t)
    seg /= len(freqs)
    return seg * env

def kick(dur=0.20):
    t = np.linspace(0, dur, int(SR * dur), endpoint=False)
    f = 140 * np.exp(-t * 34) + 50
    ph = 2 * np.pi * np.cumsum(f) / SR
    return np.sin(ph) * np.exp(-t * 9)

def hat(dur=0.06):
    t = np.linspace(0, dur, int(SR * dur), endpoint=False)
    return (np.random.rand(len(t)) * 2 - 1) * np.exp(-t * 95)

def shaker(dur=0.10):
    t = np.linspace(0, dur, int(SR * dur), endpoint=False)
    return (np.random.rand(len(t)) * 2 - 1) * np.exp(-t * 45) * np.minimum(t * 90, 1)

def pluck(freq, dur):
    t = np.linspace(0, dur, int(SR * dur), endpoint=False)
    w = (np.sin(2 * np.pi * freq * t)
         + 0.45 * np.sin(2 * np.pi * 2 * freq * t)
         + 0.20 * np.sin(2 * np.pi * 3 * freq * t))
    return w * np.exp(-t * 6.5) * (1 - np.exp(-t * 500))

def glock(freq, dur):
    """Glockenspiel sparkle: pure + bright inharmonic partial, long ring."""
    t = np.linspace(0, dur, int(SR * dur), endpoint=False)
    w = np.sin(2 * np.pi * freq * t) + 0.30 * np.sin(2 * np.pi * freq * 2.76 * t)
    return w * np.exp(-t * 3.2) * (1 - np.exp(-t * 700))

def gen_home_theme(path):
    """8 bars in F major at a skipping ~132bpm. Swung shaker groove, oom-pah bass,
    a singable melody, and a glockenspiel echo an octave up. Wrap-around tails."""
    np.random.seed(11)
    beat = 0.4545; bar = beat * 4; nbars = 8
    out = np.zeros(int(SR * bar * nbars))
    L = len(out)
    def place(sig, at, g=1.0):
        i = int(SR * at)
        idx = (np.arange(len(sig)) + i) % L
        np.add.at(out, idx, sig * g)

    roots  = ['F2', 'D2', 'A#2', 'C2'] * 2                       # F  Dm  Bb  C
    fifths = ['C3', 'A2', 'F3', 'G2'] * 2
    pads = [['F3', 'A3', 'C4'], ['D3', 'F3', 'A3'], ['A#2', 'D3', 'F3'], ['C3', 'E3', 'G3']] * 2
    # singable phrase, answered each 2 bars; (beat-offset, note, dur-in-beats)
    mel = [
        [(0, 'F4', .5), (.5, 'G4', .5), (1, 'A4', 1), (2, 'C5', .5), (2.5, 'A4', .5), (3, 'G4', 1)],
        [(0, 'F4', .5), (.5, 'A4', .5), (1, 'D5', 1), (2.5, 'C5', .5), (3, 'A4', 1)],
        [(0, 'D5', .5), (.5, 'C5', .5), (1, 'A#4', 1), (2, 'A4', .5), (2.5, 'G4', .5), (3, 'F4', 1)],
        [(0, 'G4', .5), (.5, 'A4', .5), (1, 'G4', .75), (2, 'E4', .5), (2.5, 'G4', .5), (3, 'C5', 1)],
        [(0, 'A4', .5), (.5, 'C5', .5), (1, 'F5', 1), (2, 'C5', .5), (2.5, 'A4', .5), (3, 'C5', 1)],
        [(0, 'D5', .5), (.5, 'F5', .5), (1, 'A5', 1), (2.5, 'F5', .5), (3, 'D5', 1)],
        [(0, 'A#4', .5), (.5, 'D5', .5), (1, 'F5', 1), (2, 'D5', .5), (2.5, 'A#4', .5), (3, 'D5', 1)],
        [(0, 'C5', .5), (.5, 'D5', .5), (1, 'E5', .75), (2, 'G5', .5), (2.5, 'E5', .5), (3, 'C5', 1)],
    ]
    swing = beat * 0.07
    for b in range(nbars):
        t0 = b * bar
        place(pad_chord([n(x) for x in pads[b]], bar), t0, 0.11)
        for beati in range(4):
            bt = t0 + beati * beat
            if beati in (0, 2): place(kick(), bt, 0.8)
            place(shaker(), bt + beat * 0.5 + swing, 0.36)        # swung off-beat shaker
            if beati in (1, 3): place(hat(), bt, 0.26)
        # oom-pah bass: root on 1/3, fifth on 2/4
        place(pluck(n(roots[b]), beat * 0.5), t0, 0.55)
        place(pluck(n(fifths[b]), beat * 0.45), t0 + beat, 0.38)
        place(pluck(n(roots[b]), beat * 0.5), t0 + beat * 2, 0.55)
        place(pluck(n(fifths[b]), beat * 0.45), t0 + beat * 3, 0.38)
        # off-beat chord chips
        for cf in pads[b]:
            place(pluck(n(cf) * 2, beat * 0.4), t0 + beat * 1.5 + swing, 0.10)
            place(pluck(n(cf) * 2, beat * 0.4), t0 + beat * 3.5 + swing, 0.10)
        # melody + glockenspiel echo an octave up, half a beat later
        for (bo, name, du) in mel[b]:
            place(pluck(n(name), beat * du * 0.95), t0 + bo * beat, 0.72)
            place(glock(n(name) * 2, beat * du * 0.9), t0 + bo * beat + beat * 0.5, 0.10)
    out = out / np.max(np.abs(out)) * 0.70
    write_wav(path, out)

if __name__ == '__main__':
    gen_home_theme(os.path.join(AUD, 'home_theme.wav'))
    print("done")
