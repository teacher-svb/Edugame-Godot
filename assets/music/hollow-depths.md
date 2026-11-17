# Hollow Depths (dungeon loop)

An original 24-bar chiptune loop for [Strudel](https://strudel.cc/), the darker companion to Route 7 Ramble for dungeon exploration. Same channel layout and A, A, B form, but in a minor key with sparse drums and an echoing lead.

## About

| Setting | Value |
| --- | --- |
| Tempo | 104 BPM, 4/4 (`setcps(104/60/4)`) |
| Key | D minor |
| Length | 24 bars in the form A, A, B (8 bars per section), one bar per cycle, loops forever |
| Chords | A: Dm, C, Bb, A, Gm, Dm, Eb, A7. B: Gm, Gm, Dm, Dm, Bb, C, A, A7 |

## Code

Paste this into the editor at strudel.cc and press Ctrl+Enter.

```javascript
// "Hollow Depths" - original chiptune dungeon loop
// handheld RPG style: echoing square lead, muted arps, pulsing bass, sparse noise drums
// D minor, form: A A B (24 bars), each section 8 bars, one bar per cycle
setcps(110/60/4)

// plays section a twice, then section b once (8 bars each)
const form = (a, b) => arrange([8, a], [8, a], [8, b])

// ---------- chords ----------
const chords = form(
  "<Dm C Bb A Gm Dm Eb A7>",
  "<Gm Gm Dm Dm Bb C A A7>"
)

// ---------- lead melody ----------
const leadA = note(`<
  [d5@3 f5 a5@2 g5 f5]
  [e5@4 ~ c5 d5 e5]
  [f5@3 d5 bb4@2 d5 f5]
  [e5@3 c#5 a4@2 ~@2]
  [g5@3 bb5 a5@2 g5 f5]
  [a5@4 f5@2 d5@2]
  [g5@2 eb5@2 bb4 eb5 g5 bb5]
  [a5@4 ~ e5 c#5 a4]
>`)
const leadB = note(`<
  [bb4@2 d5@2 g5@3 a5]
  [bb5@3 a5 g5@2 d5@2]
  [f5@2 a5@2 d6@3 c6]
  [a5@6 ~@2]
  [bb5@3 a5 f5@2 d5 f5]
  [g5@3 e5 c5@2 e5 g5]
  [a5@2 g5 f5 e5@2 c#5 e5]
  [a4 c#5 e5 g5 a5@2 ~@2]
>`)

const lead = form(leadA, leadB)
  .s("square").gain(.17)
  .attack(.01).decay(.2).sustain(.5).release(.1)
  .lpf(2800).vib("4:.1")
  .delay(.3).delaytime(.43).delayfeedback(.35)

// ---------- muted arpeggio ----------
const arp = n("[0 1 2 1]*2").chord(chords).anchor("a4").voicing()
  .s("square").gain(.06).decay(.1).sustain(0).lpf(1800)

// ---------- pulsing bass ----------
const bassA = note(`<
  [d2 d2 ~ d2 a2 ~ d2 ~]
  [c2 c2 ~ c2 g2 ~ c2 ~]
  [bb1 bb1 ~ bb1 f2 ~ bb1 ~]
  [a1 a1 ~ a1 e2 ~ a1 ~]
  [g1 g1 ~ g1 d2 ~ g1 ~]
  [d2 d2 ~ d2 a2 ~ d2 ~]
  [eb2 eb2 ~ eb2 bb2 ~ eb2 ~]
  [a1 a1 ~ a1 e2 ~ c#2 e2]
>`)
const bassB = note(`<
  [g1 g1 ~ g1 d2 ~ g1 ~]
  [g1 g1 ~ g1 d2 ~ bb1 ~]
  [d2 d2 ~ d2 a2 ~ d2 ~]
  [d2 d2 ~ d2 a2 ~ f2 ~]
  [bb1 bb1 ~ bb1 f2 ~ bb1 ~]
  [c2 c2 ~ c2 g2 ~ c2 ~]
  [a1 a1 ~ a1 e2 ~ a1 ~]
  [a1 a1 ~ a1 e2 ~ c#2 e2]
>`)

const bass = form(bassA, bassB)
  .s("triangle").gain(.5).decay(.2).sustain(.5)

// ---------- sparse noise-channel drums ----------
const kick  = s("bd ~ ~ ~ bd ~ ~ ~").gain(.15)
const snare = s("~ ~ ~ ~ ~ ~ white ~").decay(.08).sustain(0).lpf(3000).gain(.15)
const hats  = s("white*4").decay(.02).sustain(0).hpf(8000).gain(.06)

// ---------- water drips in the dark (random) ----------
const drip = note("~ ~ ~ ~ ~ ~ [~ a6] ~").s("sine")
  .decay(.06).sustain(0).gain(.12).degradeBy(.5)
  .delay(.4).delaytime(.29).delayfeedback(.45)

stack(lead, arp, bass, kick, snare, hats, drip)
```

## Parts

| Part | Sound | What it does |
| --- | --- | --- |
| Lead | `square` | Slow melody with a soft filter, light vibrato and an echo (`.delay`) for a cave feel |
| Arpeggio | `square` | Very quiet, muffled chord arpeggio voiced below A4 |
| Bass | `triangle` | Low, steady pulse on each chord's root with a fifth in between |
| Kick | `bd` sample | Beats 1 and 3 only |
| Snare | `white` noise | One quiet, dark hit late in each bar |
| Hi-hats | `white` noise | Soft quarter notes |
| Drip | `sine` | A high plink with echo that plays at random about half the time, so the loop never repeats exactly |

## Tweaks

- **Creepier:** lower the tempo to around 90, or change the lead's `.lpf(2800)` to around 1500 for a more muffled sound.
- **More or fewer drips:** change `.degradeBy(.5)`. Higher values mean fewer drips.
- **Echo:** `.delaytime(.43)` is roughly a dotted eighth note at 104 BPM. Adjust it if you change the tempo a lot.

## Exports

Exported at three tempos, from cycle 0 to 24 (one full pass, loops cleanly).

| File | Tempo | Length |
| --- | --- | --- |
| `hollow-depths-110bpm.wav` | 110 BPM | 52.4 s |
| `hollow-depths-140bpm.wav` | 140 BPM | 41.1 s |
| `hollow-depths-150bpm.wav` | 150 BPM | 38.4 s |
