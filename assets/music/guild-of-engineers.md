# Guild of Engineers (hidden guild loop)

An original 24-bar chiptune loop for [Strudel](https://strudel.cc/), for the hidden Guild of Engineers. Same channel layout and A, A, B form as the other songs, but mysterious and mechanical: a clockwork ostinato, a dry lead and workshop sounds.

## About

| Setting | Value |
| --- | --- |
| Tempo | 150 BPM, 4/4 (`setcps(150/60/4)`) |
| Key | E minor, with a mysterious F major chord |
| Length | 24 bars in the form A, A, B (8 bars per section), one bar per cycle, loops forever |
| Chords | A: Em, Em, C, B, Am, F, B, B7. B: Am, Em, F, C, Am, B, Em, B7 |

## Code

Paste this into the editor at strudel.cc and press Ctrl+Enter.

```javascript
// "Guild of Engineers" - original chiptune loop for a hidden guild
// mysterious and mechanical: clockwork ostinato, dry square lead, pumping bass,
// ticking clocks, gear ratchets, anvil clinks and the odd hiss of steam
// E minor with a mysterious F major chord, form: A A B (24 bars), one bar per cycle
setcps(150/60/4)

// plays section a twice, then section b once (8 bars each)
const form = (a, b) => arrange([8, a], [8, a], [8, b])

// ---------- chords ----------
const chords = form(
  "<Em Em C B Am F B B7>",
  "<Am Em F C Am B Em B7>"
)

// ---------- lead melody (dry, no vibrato, like a machine) ----------
const leadA = note(`<
  [e5@3 g5 f#5@2 e5 b4]
  [e5@4 ~@2 b4 c5]
  [e5@3 c5 g5@2 e5 c5]
  [d#5@4 f#5@2 b5@2]
  [c6@3 b5 a5@2 e5 a5]
  [c6@2 a5 f5 e5@2 f5 a5]
  [b5@3 a5 f#5@2 d#5@2]
  [a5@2 f#5@2 d#5 f#5 b4@2]
>`)
const leadB = note(`<
  [a4@2 c5 e5 a5@3 g5]
  [g5@3 f#5 e5@2 b4@2]
  [f5@2 a5 c6 b5@2 a5 f5]
  [e5@4 g5@2 c6@2]
  [c6@3 b5 a5 g5 f5 e5]
  [d#5@4 ~@2 f#5 a5]
  [g5@2 f#5 e5 d#5@2 e5 f#5]
  [a5@3 f#5 d#5@2 b4@2]
>`)

const lead = form(leadA, leadB)
  .s("square").gain(.15)
  .attack(.005).decay(.1).sustain(.7).release(.05)
  .lpf(2600)
  .delay(.25).delaytime(.47).delayfeedback(.3)

// ---------- clockwork ostinato: a gear turning in 16th notes ----------
const gears = n("[0 2 1 2]*4").chord(chords).anchor("e4").voicing()
  .s("square").gain(.06).decay(.06).sustain(0).lpf(1600)

// ---------- pumping, machine-like bass ----------
const bassA = note(`<
  [e2 ~ e2 e3 ~ e2 b2 ~]
  [e2 ~ e2 e3 ~ e2 b2 ~]
  [c2 ~ c2 c3 ~ c2 g2 ~]
  [b1 ~ b1 b2 ~ b1 f#2 ~]
  [a1 ~ a1 a2 ~ a1 e2 ~]
  [f2 ~ f2 f3 ~ f2 c3 ~]
  [b1 ~ b1 b2 ~ b1 f#2 ~]
  [b1 ~ b1 b2 ~ b1 a2 f#2]
>`)
const bassB = note(`<
  [a1 ~ a1 a2 ~ a1 e2 ~]
  [e2 ~ e2 e3 ~ e2 b2 ~]
  [f2 ~ f2 f3 ~ f2 c3 ~]
  [c2 ~ c2 c3 ~ c2 g2 ~]
  [a1 ~ a1 a2 ~ a1 e2 ~]
  [b1 ~ b1 b2 ~ b1 f#2 ~]
  [e2 ~ e2 e3 ~ e2 b2 ~]
  [b1 ~ b1 b2 ~ b1 a2 f#2]
>`)

const bass = form(bassA, bassB)
  .s("triangle").gain(.5).decay(.12).sustain(.6)

// ---------- workshop percussion ----------
// piston kick
const kick   = s("bd ~ ~ ~ bd ~ ~ ~").gain(.2)
// tick-tock of a clock
const clock  = note("[g6 d6]*4").s("square")
  .decay(.01).sustain(0).gain(.045)
// gear ratchet: very short noise clicks
const ratchet = s("white*16").decay(.008).sustain(0).bpf(3000)
  .gain("[.05 .02 .03 .02]*4")
// anvil clink on the and of beat 4
const anvil  = note("~ ~ ~ ~ ~ ~ ~ e7").s("square")
  .decay(.08).sustain(0).gain(.035).room(.2)
// a hiss of steam every 4 bars
const steam  = s("<~ ~ ~ white>")
  .attack(.3).decay(.6).sustain(0).hpf(4000).gain(.06)

stack(lead, gears, bass, kick, clock, ratchet, anvil, steam)
```

## Parts

| Part | Sound | What it does |
| --- | --- | --- |
| Lead | `square` | Dry melody with no vibrato, so it sounds like a machine, plus an echo |
| Gears | `square` | Quiet, muffled arpeggio in steady 16th notes, like a turning cog |
| Bass | `triangle` | Pumping, piston-like pattern on each chord's root, octave and fifth |
| Kick | `bd` sample | Soft piston hits on beats 1 and 3 |
| Clock | `square` | Tick-tock on every eighth note |
| Ratchet | `white` noise | Very short clicks on every 16th note, like a ratchet |
| Anvil | `square` | Metallic clink at the end of each bar |
| Steam | `white` noise | A hiss that swells in every fourth bar |

## Tweaks

- **More hidden:** slow it down, or lower the lead's `.lpf(2600)` to around 1800 so it sounds like it's behind a wall.
- **More mysterious:** the F major chord in bars 6 and 11 is outside E minor. Add it in more places for a stranger feel.

## Exports

Exported at three tempos, from cycle 0 to 24 (one full pass, loops cleanly).

| File | Tempo | Length |
| --- | --- | --- |
| `guild-of-engineers-110bpm.wav` | 110 BPM | 52.4 s |
| `guild-of-engineers-140bpm.wav` | 140 BPM | 41.1 s |
| `guild-of-engineers-150bpm.wav` | 150 BPM | 38.4 s |
