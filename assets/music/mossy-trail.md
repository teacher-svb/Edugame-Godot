# Mossy Trail (forest loop)

An original 24-bar chiptune loop for [Strudel](https://strudel.cc/), for walking through a forest. Same channel layout and A, A, B form as the other songs, but in a lilting 12/8 shuffle with light percussion and birds.

## About

| Setting | Value |
| --- | --- |
| Tempo | 90 BPM in 12/8 (`setcps(90/60/4)`): 4 dotted beats per bar, 3 steps per beat |
| Key | G major, with mixolydian F chords |
| Length | 24 bars in the form A, A, B (8 bars per section), one bar per cycle, loops forever |
| Chords | A: G, F, C, G, Em, C, Am, D. B: Em, C, G, D, Em, C, F, D |

## Code

Paste this into the editor at strudel.cc and press Ctrl+Enter.

```javascript
// "Mossy Trail" - original chiptune forest-walk loop
// handheld RPG style: square lead, triplet pluck arps, walking bass, woodblock + shaker, birds
// G major with mixolydian F chords, 12/8 shuffle (12 steps per bar, 3 per beat)
// form: A A B (24 bars), each section 8 bars, one bar per cycle
setcps(90/60/4)

// plays section a twice, then section b once (8 bars each)
const form = (a, b) => arrange([8, a], [8, a], [8, b])

// ---------- chords ----------
const chords = form(
  "<G F C G Em C Am D>",
  "<Em C G D Em C F D>"
)

// ---------- lead melody (12 steps per bar) ----------
const leadA = note(`<
  [d5@2 b4 g4@3 b4@2 d5 g5@3]
  [f5@2 e5 d5@2 c5 a4@3 c5@3]
  [e5@2 g5 e5@2 c5 d5@2 e5 c5@3]
  [d5@6 ~@3 b4 c5 d5]
  [e5@2 g5 b5@3 a5@2 g5 e5@3]
  [g5@2 e5 c5@3 e5@2 g5 c6@3]
  [a5@2 g5 e5@2 c5 a4@3 c5@3]
  [f#5@6 ~@3 a4 b4 c5]
>`)
const leadB = note(`<
  [b4@3 e5@3 g5@3 b5@3]
  [c6@5 b5 a5@3 g5@3]
  [b5@2 a5 g5@2 d5 b4@3 d5@3]
  [a5@6 f#5@3 d5@3]
  [e5@2 f#5 g5@2 a5 b5@3 g5@3]
  [c6@3 b5@3 a5@3 g5@3]
  [a5@2 g5 f5@2 e5 f5@3 a5@3]
  [f#5@6 ~@3 a4 b4 c5]
>`)

const lead = form(leadA, leadB)
  .s("square").gain(.16)
  .attack(.01).decay(.15).sustain(.6).release(.08)
  .lpf(3200).vib("5:.12")

// ---------- triplet pluck arpeggio ----------
const arp = n("[0 2 1]*4").chord(chords).anchor("b4").voicing()
  .s("triangle").gain(.18).decay(.12).sustain(0)

// ---------- walking bass ----------
const bassA = note(`<
  [g2@2 ~ d3@2 ~ g3@2 ~ d3@2 ~]
  [f2@2 ~ c3@2 ~ f3@2 ~ c3@2 ~]
  [c2@2 ~ g2@2 ~ c3@2 ~ g2@2 ~]
  [g2@2 ~ d3@2 ~ g3@2 ~ d3@2 ~]
  [e2@2 ~ b2@2 ~ e3@2 ~ b2@2 ~]
  [c2@2 ~ g2@2 ~ c3@2 ~ g2@2 ~]
  [a1@2 ~ e2@2 ~ a2@2 ~ e2@2 ~]
  [d2@2 ~ a2@2 ~ d3@2 ~ c3 b2 a2]
>`)
const bassB = note(`<
  [e2@2 ~ b2@2 ~ e3@2 ~ b2@2 ~]
  [c2@2 ~ g2@2 ~ c3@2 ~ g2@2 ~]
  [g2@2 ~ d3@2 ~ g3@2 ~ d3@2 ~]
  [d2@2 ~ a2@2 ~ d3@2 ~ a2@2 ~]
  [e2@2 ~ b2@2 ~ e3@2 ~ b2@2 ~]
  [c2@2 ~ g2@2 ~ c3@2 ~ g2@2 ~]
  [f2@2 ~ c3@2 ~ f3@2 ~ c3@2 ~]
  [d2@2 ~ a2@2 ~ d3@2 ~ c3 b2 a2]
>`)

const bass = form(bassA, bassB)
  .s("triangle").gain(.5).decay(.18).sustain(.6)

// ---------- light percussion ----------
const kick      = s("bd ~ ~ ~ ~ ~ bd ~ ~ ~ ~ ~").gain(.35)
const woodblock = note("~ ~ ~ g6 ~ ~ ~ ~ ~ g6 ~ ~").s("square")
  .decay(.015).sustain(0).gain(.06)
const shaker    = s("white*12").decay(.02).sustain(0).hpf(7000)
  .gain("[.07 .025 .04]*4")

// ---------- birds (random) ----------
const birds = note("~ ~ ~ ~ ~ ~ ~ ~ [e7 b7 g7] ~ ~ ~").s("sine")
  .decay(.04).sustain(0).gain(.08).degradeBy(.7)
  .delay(.3).delaytime(.22).delayfeedback(.3)

stack(lead, arp, bass, kick, woodblock, shaker, birds)
```

## Parts

| Part | Sound | What it does |
| --- | --- | --- |
| Lead | `square` | Melody with a softer filter than the day theme and light vibrato |
| Arpeggio | `triangle` | Short triplet plucks, three per beat, that carry the skipping feel |
| Bass | `triangle` | Walking bass on root, fifth and octave, with a C, B, A run back to G at the end of each section |
| Kick | `bd` sample | Soft hits on beats 1 and 3 |
| Woodblock | `square` | Short high click on beats 2 and 4 |
| Shaker | `white` noise | Every step, accented on the beat |
| Birds | `sine` | Quick three-note chirp with echo, at random about a third of the time |

## Tweaks

- **Tempo:** the number in `setcps` counts dotted beats, each holding three steps, so it feels busier than the 4/4 songs at the same number. Around 80 to 100 suits a walk.
- **More or fewer birds:** change `.degradeBy(.7)`. Higher values mean fewer chirps.

## Exports

Exported from cycle 0 to 24 (one full pass, loops cleanly).

| File | Tempo | Length |
| --- | --- | --- |
| `mossy-trail-90bpm.wav` | 90 BPM (12/8) | 64.0 s |

Night-time version with the same melody and form: Mossy Trail (Night)
