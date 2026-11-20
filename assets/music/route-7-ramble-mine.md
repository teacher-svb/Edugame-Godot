# Route 7 Ramble (Mine)

A gloomy underground version of Route 7 Ramble for [Strudel](https://strudel.cc/), for an underground mine. The melody, chords and A, A, B form follow the day version, moved to C minor, with longer held notes, a cave hum, distant thuds, pickaxe clinks and dripping water.

## About

| Setting | Value |
| --- | --- |
| Tempo | 90 BPM, 4/4 (`setcps(90/60/4)`) |
| Key | C minor |
| Length | 24 bars in the form A, A, B (8 bars per section), one bar per cycle, loops forever |
| Chords | A: Cm, Fm, G, Cm, Ab, Fm, Fm/G, Cm. B: Ab, Eb, Fm, Cm, Fm, G, Eb, Fm/G |

## Code

Paste this into the editor at strudel.cc and press Ctrl+Enter.

```javascript
// "Route 7 Ramble (Mine)" - gloomy underground version of the main theme
// same melody shape and form as the day version, moved to C minor;
// longer held notes and fewer of the busy ones, a cave hum, distant thuds, pickaxe clinks and dripping water
// form: A A B (24 bars), each section 8 bars, one bar per cycle
setcps(90/60/4)

// plays section a twice, then section b once (8 bars each)
const form = (a, b) => arrange([8, a], [8, a], [8, b])

// ---------- chords (the day chords, in C minor) ----------
const chords = form(
  "<Cm Fm G Cm Ab Fm [Fm G] Cm>",
  "<Ab Eb Fm Cm Fm G Eb [Fm G]>"
)
const roots = form(
  "<c2 f2 g1 c2 ab1 f2 [f2 g1] c2>",
  "<ab1 eb2 f2 c2 f2 g1 eb2 [f2 g1]>"
)

// ---------- melody: the day tune in C minor, busy notes merged into longer ones ----------
const leadA = note(`<
  [g4 c5 eb5 g5@3 eb5 ab5]
  [ab5@3 g5 f5@3 ~]
  [g5@2 d5@2 b4 d5 g5@2]
  [eb5@5 c5 d5 eb5]
  [ab5@3 g5 eb5@3 ~]
  [f5@2 ab5 c6@4 ~]
  [d5@2 f5@2 g5@2 b4@2]
  [c5@5 ~ eb5 d5]
>`)
const leadB = note(`<
  [eb5@2 ab5@2 c6@2 ab5@2]
  [g5@3 eb5 bb4@4]
  [ab5@2 c6@2 f5@2 ab5@2]
  [eb6@4 d6 c6@3]
  [f5@2 ab5@2 c6@2 ab5@2]
  [g5@3 b4 d5@4]
  [eb5@2 g5@2 bb5@2 g5@2]
  [f5@2 ab5@2 g5@3 d5]
>`)

// soft, hollow triangle lead with a long echo and a big cave reverb
const lead = form(leadA, leadB)
  .s("triangle").gain(.3)
  .attack(.04).decay(.6).sustain(.4).release(.6)
  .lpf(1800)
  .delay(.35).delaytime(.5).delayfeedback(.45)
  .room(.6)

// ---------- dark, muffled chord pad ----------
const pad = chord(chords).anchor("c4").voicing()
  .s("sine").gain(.05)
  .attack(.6).decay(.5).sustain(.6).release(1)
  .lpf(900).room(.5)

// ---------- one long, low bass note per chord ----------
const bass = note(roots)
  .s("triangle").gain(.45)
  .attack(.05).decay(1).sustain(.5).release(.5)
  .lpf(400)

// ---------- cave hum: a constant low drone ----------
const hum = note("c1").s("sine").gain(.12)
  .attack(.5).sustain(1).release(.5)

// ---------- distant thud on beat 1, like work far down the tunnel ----------
const thud = s("bd").gain(.25).lpf(300).room(.5)

// ---------- dripping water (random pitches and timing) ----------
const drips = note("~ ~ [~ a6] ~ ~ ~ [e6 ~] ~").s("sine")
  .decay(.06).sustain(0).gain(.12).degradeBy(.5)
  .delay(.45).delaytime(.33).delayfeedback(.5)
  .room(.4)

// ---------- a pickaxe clinking somewhere in the dark ----------
const pickaxe = note("<~ [~ ~ ~ ~ ~ e7 ~ ~] ~ [~ ~ ~ e7 ~ ~ ~ e7]>").s("square")
  .decay(.05).sustain(0).gain(.025).lpf(5000).room(.7)

// ---------- low rumble of the earth ----------
const rumble = s("brown").gain(.04)
  .attack(1).sustain(1).release(1).lpf(250)

stack(lead, pad, bass, hum, thud, drips, pickaxe, rumble)
```

## Differences from the day version

| Part | Day | Mine |
| --- | --- | --- |
| Key and tempo | C major, 140 BPM | C minor, 90 BPM |
| Melody | Bright and busy | Same tune in C minor, busy runs merged into longer held notes |
| Lead | Bright `square` with vibrato | Soft, muffled `triangle` with a long echo and cave reverb |
| Harmony | Bouncy `square` arpeggio | Dark, slowly swelling `sine` pad |
| Bass | Bouncy octave jumps | One long, low note per chord, plus a constant low C hum and a brown-noise rumble |
| Drums | Kick, noise snare and hats | A single muffled thud per bar, like work far down the tunnel |
| Extras | None | Water drips at random and a faint pickaxe clink in some bars |

## Exports

Exported from cycle 0 to 24 (one full pass, loops cleanly).

| File | Tempo | Length |
| --- | --- | --- |
| `route7-ramble-mine-90bpm.wav` | 90 BPM | 64.0 s |
