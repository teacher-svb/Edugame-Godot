# Route 7 Ramble (Strudel chiptune loop)

Nov 16, 2026 · @Syndias

## About

An original 24-bar chiptune loop for [Strudel](https://strudel.cc/), written in the style of handheld RPG overworld (route) music. The melody is new, not taken from any game.

| Setting | Value |
| --- | --- |
| Tempo | 140 BPM, 4/4 (`setcps(140/60/4)`) |
| Key | C major |
| Length | 24 bars in the form A, A, B (8 bars per section), one bar per cycle, loops forever |
| Chords | A: C, F, G, C, Am, F, Dm/G, C. B: Am, Em, F, C, Dm, G, Em, F/G |

## Code

Paste this into the editor at strudel.cc and press Ctrl+Enter.

```javascript
// "Route 7 Ramble" - original chiptune overworld loop
// handheld RPG style: square lead, square arps, triangle bass, noise drums
// form: A A B (24 bars), each section 8 bars, one bar per cycle
setcps(140/60/4)

// plays section a twice, then section b once (8 bars each)
const form = (a, b) => arrange([8, a], [8, a], [8, b])

// ---------- chords ----------
const chords = form(
  "<C F G C Am F [Dm G] C>",
  "<Am Em F C Dm G Em [F G]>"
)

// ---------- lead melody ----------
const leadA = note(`<
  [g4 c5 e5 g5 ~ e5 g5 a5]
  [a5@3 g5 f5@2 e5 f5]
  [g5@2 d5 ~ b4 d5 g5 f5]
  [e5@4 c5 d5 e5 g5]
  [a5@3 g5 e5@2 c5 e5]
  [f5@2 a5 c6 a5@2 g5 f5]
  [d5 f5 a5 f5 g5 b5 d6 b5]
  [c6@4 ~ g5 e5 d5]
>`)
const leadB = note(`<
  [e5@2 a5@2 c6 b5 a5 e5]
  [g5@3 e5 b4@2 e5 g5]
  [a5@2 c6@2 f5 a5 c6 d6]
  [e6@4 d6 c6 g5 e5]
  [f5@2 a5 d6 c6 a5 f5 a5]
  [g5@3 b5 d6@2 b5 g5]
  [e5 g5 b5 e6 d6 b5 g5 e5]
  [f5 a5 c6 a5 g5@2 ~ d5]
>`)

const lead = form(leadA, leadB)
  .s("square").gain(.2)
  .attack(.005).decay(.12).sustain(.6).release(.05)
  .lpf(4500).vib("5:.15")

// ---------- bouncy arpeggio under the melody ----------
const arp = n("[0 2 1 2]*2").chord(chords).anchor("c5").voicing()
  .s("square").gain(.07).decay(.08).sustain(0).lpf(3000)

// ---------- octave-jumping triangle bass ----------
const bassA = note(`<
  [c2 c3 c2 c3 c2 c3 g2 c3]
  [f2 f3 f2 f3 f2 f3 c3 f3]
  [g2 g3 g2 g3 g2 g3 d3 g3]
  [c2 c3 c2 c3 c2 c3 g2 e2]
  [a1 a2 a1 a2 a1 a2 e2 a2]
  [f2 f3 f2 f3 f2 f3 c3 f3]
  [d2 d3 d2 d3 g2 g3 g2 b2]
  [c2 c3 c2 c3 g2 g2 a2 b2]
>`)
const bassB = note(`<
  [a1 a2 a1 a2 a1 a2 e2 a2]
  [e2 e3 e2 e3 e2 e3 b2 e3]
  [f2 f3 f2 f3 f2 f3 c3 f3]
  [c2 c3 c2 c3 c2 c3 g2 c3]
  [d2 d3 d2 d3 d2 d3 a2 d3]
  [g2 g3 g2 g3 g2 g3 d3 g3]
  [e2 e3 e2 e3 e2 e3 b2 e3]
  [f2 f3 f2 f3 g2 g3 g2 b2]
>`)

const bass = form(bassA, bassB)
  .s("triangle").gain(.45).decay(.15).sustain(.7)

// ---------- noise-channel drums ----------
const kick  = s("bd ~ ~ bd ~ ~ bd ~").gain(.25)
const snare = s("~ white ~ white").decay(.12).sustain(0).lpf(5000).gain(.22)
const hats  = s("white*8").decay(.03).sustain(0).hpf(7000).gain("[.12 .06]*4")

stack(lead, arp, bass, kick, snare, hats)
```

## Parts

Each part copies one channel of the old Game Boy sound chip.

| Part | Sound | What it does |
| --- | --- | --- |
| Lead | `square` | Main melody with light vibrato (`vib("5:.15")`) and a soft low-pass filter |
| Arpeggio | `square` | Quiet chord arpeggio voiced below C5, so it stays under the melody |
| Bass | `triangle` | Root notes jumping an octave up and down, the classic chiptune bass |
| Kick | `bd` sample | Syncopated pattern on steps 1, 4 and 7 of each bar |
| Snare | `white` noise | Short noise bursts on beats 2 and 4 |
| Hi-hats | `white` noise | Eighth notes, high-passed, with alternating loud and soft hits |

## Tweaks

- **Tempo:** change `140` in `setcps(140/60/4)`. Around 150 feels like a battle theme, around 110 like a town theme.
- **Lead sound:** raise or lower the vibrato in `.vib("5:.15")`, or swap `"square"` for `"triangle"` for a softer lead.
- **Song form:** change the `form` helper at the top. For example `arrange([8, a], [8, b], [8, a], [8, b])` plays A, B, A, B. Every part (chords, lead, bass) goes through `form`, so they stay in sync.
- **Shortcuts:** Ctrl+Enter plays or updates the code, Ctrl+. stops it.

## Exports

Exported at three tempos, from cycle 0 to 24 (one full pass, loops cleanly).

| File | Tempo | Length |
| --- | --- | --- |
| `route7-ramble-110bpm.wav` | 110 BPM | 52.4 s |
| `route7-ramble-140bpm.wav` | 140 BPM | 41.1 s |
| `route7-ramble-150bpm.wav` | 150 BPM | 38.4 s |

Dungeon theme in the same style: Hollow Depths

Night-time version with the same melody and form: Route 7 Ramble (Night)
