# Mossy Trail (Night)

A night-time version of Mossy Trail for [Strudel](https://strudel.cc/). The melody, chords, 12/8 shuffle and A, A, B form are the same as the day version, and both run at 90 BPM, so the two can be crossfaded at a bar line.

## About

| Setting | Value |
| --- | --- |
| Tempo | 90 BPM in 12/8 (`setcps(90/60/4)`), same as the day version |
| Key | G major, with mixolydian F chords |
| Length | 24 bars in the form A, A, B (8 bars per section), one bar per cycle, loops forever |
| Chords | A: G, F, C, G, Em, C, Am, D. B: Em, C, G, D, Em, C, F, D |

## Code

Paste this into the editor at strudel.cc and press Ctrl+Enter.

```javascript
// "Mossy Trail (Night)" - night-time version of the forest-walk loop
// same melody, chords and form as the day version; softer and slower
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

// ---------- lead melody (same notes as the day version) ----------
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

// soft triangle lead with slow vibrato, echo and a little reverb
const lead = form(leadA, leadB)
  .s("triangle").gain(.35)
  .attack(.02).decay(.3).sustain(.5).release(.2)
  .lpf(2400).vib("3:.1")
  .delay(.25).delaytime(.26).delayfeedback(.3)
  .room(.35)

// ---------- twinkling triplet arpeggio, higher up ----------
const arp = n("[0 2 1]*4").chord(chords).anchor("e6").voicing()
  .s("sine").gain(.045).decay(.15).sustain(0)
  .delay(.3).delaytime(.26).delayfeedback(.4)

// ---------- long, calm bass notes ----------
const bassA = note(`<
  [g2@6 d3@3 g3@3]
  [f2@6 c3@3 f3@3]
  [c2@6 g2@3 c3@3]
  [g2@6 d3@3 g3@3]
  [e2@6 b2@3 e3@3]
  [c2@6 g2@3 c3@3]
  [a1@6 e2@3 a2@3]
  [d2@6 a2@3 c3 b2 a2]
>`)
const bassB = note(`<
  [e2@6 b2@3 e3@3]
  [c2@6 g2@3 c3@3]
  [g2@6 d3@3 g3@3]
  [d2@6 a2@3 d3@3]
  [e2@6 b2@3 e3@3]
  [c2@6 g2@3 c3@3]
  [f2@6 c3@3 f3@3]
  [d2@6 a2@3 c3 b2 a2]
>`)

const bass = form(bassA, bassB)
  .s("triangle").gain(.9).attack(.01).decay(.4).sustain(.6).release(.2)

// ---------- hushed percussion: soft kick on beat 1, quiet shaker on the beats ----------
const kick   = s("bd ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~").gain(.15).lpf(700)
const shaker = s("[white ~ ~]*4").decay(.02).sustain(0).hpf(9000).gain(.025)

// ---------- crickets (random) ----------
const crickets = note("~ ~ ~ ~ ~ ~ [e7 e7 e7] ~ ~ ~ ~ ~").s("sine")
  .decay(.02).sustain(0).gain(.05).degradeBy(.6)

// ---------- an owl, once every 4 bars ----------
const owl = note("<~ ~ ~ [~ ~ ~ ~ ~ ~ a4@2 ~ f4@3]>").s("sine")
  .attack(.05).decay(.25).sustain(.3).release(.2)
  .lpf(900).gain(.22).room(.4)

stack(lead, arp, bass, kick, shaker, crickets, owl)
```

## Differences from the day version

| Part | Day | Night |
| --- | --- | --- |
| Lead | `square` with vibrato | Soft `triangle` with slow vibrato, triplet echo and a little reverb |
| Arpeggio | Triplet `triangle` plucks | Quiet triplet `sine` twinkles higher up, with echo |
| Bass | Walking bass | Long, calm notes on each chord, same C, B, A run at the end of each section |
| Percussion | Kick, woodblock and accented shaker | Soft muffled kick on beat 1 and a very quiet shaker on the beats, no woodblock |
| Ambience | Birds | Crickets at random and a two-note owl call every fourth bar |

## Exports

Exported from cycle 0 to 24 (one full pass, loops cleanly).

| File | Tempo | Length |
| --- | --- | --- |
| `mossy-trail-night-90bpm.wav` | 90 BPM (12/8) | 64.0 s |
