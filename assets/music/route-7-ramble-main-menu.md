# Route 7 Ramble (Main Menu)

A title-screen arrangement of Route 7 Ramble for [Strudel](https://strudel.cc/). The melody, chords and A, A, B form are the same as the day version, so players recognise the tune in game; the arrangement is bigger and grander.

## About

| Setting | Value |
| --- | --- |
| Tempo | 140 BPM, 4/4 (`setcps(140/60/4)`) |
| Key | C major |
| Length | 24 bars in the form A, A, B (8 bars per section), one bar per cycle, loops forever |
| Chords | A: C, F, G, C, Am, F, Dm/G, C. B: Am, Em, F, C, Dm, G, Em, F/G |

## Code

Paste this into the editor at strudel.cc and press Ctrl+Enter.

```javascript
// "Route 7 Ramble (Main Menu)" - title-screen arrangement of the overworld loop
// same melody, chords and form as the day version; bigger and grander
// form: A A B (24 bars), each section 8 bars, one bar per cycle
setcps(140/60/4)

// plays section a twice, then section b once (8 bars each)
const form = (a, b) => arrange([8, a], [8, a], [8, b])

// ---------- chords ----------
const chords = form(
  "<C F G C Am F [Dm G] C>",
  "<Am Em F C Dm G Em [F G]>"
)
const roots = form(
  "<c2 f2 g2 c2 a1 f2 [d2 g2] c2>",
  "<a1 e2 f2 c2 d2 g2 e2 [f2 g2]>"
)

// ---------- melody (same notes as the day version) ----------
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
const melody = form(leadA, leadB)

// main square lead, centred, with some room
const lead = melody
  .s("square").gain(.18)
  .attack(.005).decay(.15).sustain(.7).release(.1)
  .lpf(5000).vib("5:.12")
  .room(.25)

// the same melody an octave lower
const leadLow = melody.transpose(-12)
  .s("square").gain(.08)
  .decay(.15).sustain(.6).release(.1).lpf(2000)
  .room(.25)

// wide supersaw layer under the lead
const leadWide = melody
  .s("supersaw").gain(.05)
  .attack(.01).decay(.2).sustain(.6).release(.15)
  .lpf(3000).room(.4)

// high sparkle, one octave up, only in the B section
const sparkle = form(silence, leadB).transpose(12)
  .s("sine").gain(.05).decay(.2).sustain(.3)
  .delay(.25).delaytime(.35).delayfeedback(.3)

// ---------- big supersaw pad ----------
const pad = chord(chords).anchor("g4").voicing()
  .s("supersaw").gain(.045)
  .attack(.2).decay(.4).sustain(.7).release(.6)
  .lpf(1600).room(.5)

// ---------- bouncy arpeggio, gently moving left and right ----------
const arp = n("[0 2 1 2]*2").chord(chords).anchor("c5").voicing()
  .s("square").gain(.05).decay(.08).sustain(0).lpf(3000)
  .pan(sine.slow(4).range(.25, .75))

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

// ---------- timpani-style hits on the chord roots, beats 1 and 3 ----------
const timpani = note(roots).struct("x ~ ~ ~ x ~ ~ ~")
  .s("sine").gain(.45).attack(.005).decay(.4).sustain(0)
  .lpf(600).room(.3)

// ---------- drums: crash at each section start, snare fill at each section end ----------
const snareBar = "<[~ white ~ white]!7 [~ white [white white] [white white white white]]>"
const crashBar = "<white ~ ~ ~ ~ ~ ~ ~>"

const kick  = s("bd ~ ~ ~ bd ~ bd ~").gain(.4)
const snare = s(form(snareBar, snareBar))
  .decay(.14).sustain(0).lpf(5000).gain(.3).room(.3)
const hats  = s("white*8").decay(.03).sustain(0).hpf(7000)
  .gain("[.1 .05]*4").pan(.65)
const crash = s(form(crashBar, crashBar))
  .decay(1.6).sustain(0).hpf(4000).gain(.15).room(.4)

stack(lead, leadLow, leadWide, sparkle, pad, arp, bass, timpani, kick, snare, hats, crash)
```

## Differences from the day version

| Part | Day | Main menu |
| --- | --- | --- |
| Lead | Single `square` | `square` lead plus the melody an octave lower and a wide `supersaw` layer, with reverb |
| Pad | None | Swelling `supersaw` chords with lots of reverb |
| Arpeggio | Centred | Quieter, drifting slowly between left and right |
| Timpani | None | Deep hits on the chord roots on beats 1 and 3 |
| Sparkle | None | High echo of the melody in the B section only |
| Drums | Kick, snare and hats | Louder kick, snare and hats, a crash at each section start and a snare fill at each section end |

## Exports

Exported from cycle 0 to 24 (one full pass, loops cleanly).

| File | Tempo | Length |
| --- | --- | --- |
| `route7-ramble-menu-140bpm.wav` | 140 BPM | 41.1 s |
