# Gears in Motion (Guild of Engineers, upbeat)

An original 24-bar chiptune loop for [Strudel](https://strudel.cc/), an upbeat theme for the Guild of Engineers written from scratch. Same A, A, B form as the other songs, with a bright piston-like melody and a busy workshop of machine sounds.

## About

| Setting | Value |
| --- | --- |
| Tempo | 150 BPM, 4/4 (`setcps(150/60/4)`) |
| Key | D major, with a mixolydian C chord |
| Length | 24 bars in the form A, A, B (8 bars per section), one bar per cycle, loops forever |
| Chords | A: D, D, C, G, D, Bm, G, A. B: G, A, F#m, Bm, G, A, D, A |

## Code

Paste this into the editor at strudel.cc and press Ctrl+Enter.

```javascript
// "Gears in Motion" - original upbeat chiptune loop for the Guild of Engineers
// a busy, cheerful workshop: piston-like melody, buzzing conveyor arps, driving bass,
// four-on-the-floor pistons, steam-valve puffs, hammer tinks, a factory whistle and a steam hiss
// D major with a mixolydian C chord, form: A A B (24 bars), one bar per cycle
setcps(150/60/4)

// plays section a twice, then section b once (8 bars each)
const form = (a, b) => arrange([8, a], [8, a], [8, b])

// ---------- chords ----------
const chords = form(
  "<D D C G D Bm G A>",
  "<G A F#m Bm G A D A>"
)

// ---------- lead melody: repeated "piston" notes that spring upward ----------
const leadA = note(`<
  [d5 ~ d5 a5 ~ f#5 a5 d6]
  [a5@2 f#5 a5 b5@2 a5 ~]
  [c5 ~ c5 g5 ~ e5 g5 c6]
  [b5@2 g5 b5 d6@3 ~]
  [d5 ~ d5 a5 ~ f#5 a5 d6]
  [d6@2 b5 f#5 d6@2 c#6 b5]
  [b5@2 g5 b5 d6 e6 d6 b5]
  [a5@3 e5 c#5@2 e5 a5]
>`)
const leadB = note(`<
  [g5@2 ~ g5 b5@2 d6 ~]
  [c#6@2 ~ c#6 e6@2 c#6 a5]
  [f#5@2 ~ f#5 a5@2 c#6 ~]
  [d6@3 c#6 b5@2 f#5@2]
  [g5 b5 d6 g6 ~ d6 b5 d6]
  [e6@2 c#6 a5 e6@2 c#6 e6]
  [f#6@3 e6 d6@2 a5 f#5]
  [e5 f#5 g5 a5 ~ a5 b5 c#6]
>`)

// bright, staccato square lead with a short echo
const lead = form(leadA, leadB)
  .s("square").gain(.16)
  .attack(.003).decay(.08).sustain(.4).release(.03)
  .lpf(5000)
  .delay(.15).delaytime(.33).delayfeedback(.25)

// ---------- conveyor belt: a buzzing sawtooth arpeggio in 16th notes ----------
const conveyor = n("[0 2 1 3]*4").chord(chords).anchor("a4").voicing()
  .s("sawtooth").gain(.1).decay(.05).sustain(0).lpf(1200)

// ---------- driving, syncopated bass ----------
const bassA = note(`<
  [d2 d2 d3 d2 ~ d2 d3 a2]
  [d2 d2 d3 d2 ~ d2 d3 a2]
  [c2 c2 c3 c2 ~ c2 c3 g2]
  [g1 g1 g2 g1 ~ g1 g2 d2]
  [d2 d2 d3 d2 ~ d2 d3 a2]
  [b1 b1 b2 b1 ~ b1 b2 f#2]
  [g1 g1 g2 g1 ~ g1 g2 d2]
  [a1 a1 a2 a1 ~ a1 a2 e2]
>`)
const bassB = note(`<
  [g1 g1 g2 g1 ~ g1 g2 d2]
  [a1 a1 a2 a1 ~ a1 a2 e2]
  [f#1 f#1 f#2 f#1 ~ f#1 f#2 c#2]
  [b1 b1 b2 b1 ~ b1 b2 f#2]
  [g1 g1 g2 g1 ~ g1 g2 d2]
  [a1 a1 a2 a1 ~ a1 a2 e2]
  [d2 d2 d3 d2 ~ d2 d3 a2]
  [a1 a1 a2 a1 a2 b2 c#3 e3]
>`)

const bass = form(bassA, bassB)
  .s("triangle").gain(.5).decay(.12).sustain(.6)

// ---------- workshop machinery ----------
// pistons: a steady four-on-the-floor kick
const pistons = s("bd*4").gain(.3)
// steam valve: a short noise puff on beats 2 and 4
const valve   = s("~ white ~ white").decay(.12).sustain(0).hpf(2000).gain(.18)
// cog clicks: tiny high clicks on the off-beats
const cogs    = note("[~ d7]*4").s("square").decay(.008).sustain(0).gain(.03)
// hammer tinks: a light metallic tap twice per bar
const hammer  = note("~ ~ [~ b6] ~ ~ ~ [~ b6] ~").s("triangle")
  .decay(.05).sustain(0).gain(.07).room(.15)
// factory whistle at the end of every 8 bars
const whistle = note("<~ ~ ~ ~ ~ ~ ~ [~ ~ ~ ~ a6@4]>").s("sine")
  .attack(.04).decay(.3).sustain(.6).release(.2)
  .vib("6:.25").gain(.05)

// a hiss of steam every 4 bars (from the first guild song)
const steam   = s("<~ ~ ~ white>")
  .attack(.3).decay(.6).sustain(0).hpf(4000).gain(.06)

stack(lead, conveyor, bass, pistons, valve, cogs, hammer, whistle, steam)
```

## Parts

| Part | Sound | What it does |
| --- | --- | --- |
| Lead | `square` | Bright, staccato melody: a note repeated like a piston firing, then springing upward |
| Conveyor | `sawtooth` | Buzzing chord arpeggio in fast 16th notes, like a conveyor belt |
| Bass | `triangle` | Driving, slightly syncopated bass with a run up at the end of the loop |
| Pistons | `bd` sample | Steady kick on every beat |
| Steam valve | `white` noise | Short puffs on beats 2 and 4 |
| Cogs | `square` | Tiny high clicks between the beats |
| Hammer | `triangle` | Light metallic tap twice per bar |
| Whistle | `sine` | Wobbly factory whistle at the end of every 8 bars |
| Steam hiss | `white` noise | A hiss that swells in every fourth bar, from the first guild song |

## Exports

Exported at three tempos, from cycle 0 to 24 (one full pass, loops cleanly).

| File | Tempo | Length |
| --- | --- | --- |
| `gears-in-motion-110bpm.wav` | 110 BPM | 52.4 s |
| `gears-in-motion-140bpm.wav` | 140 BPM | 41.1 s |
| `gears-in-motion-150bpm.wav` | 150 BPM | 38.4 s |
