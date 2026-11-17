# Route 7 Ramble (Night)

A night-time version of Route 7 Ramble for [Strudel](https://strudel.cc/). The melody, chords and A, A, B form are the same as the day version, so the two can be crossfaded at a bar line; only the instruments, tempo and drums change.

## About

| Setting | Value |
| --- | --- |
| Tempo | 110 BPM, 4/4 (`setcps(110/60/4)`) |
| Key | C major |
| Length | 24 bars in the form A, A, B (8 bars per section), one bar per cycle, loops forever |
| Chords | A: C, F, G, C, Am, F, Dm/G, C. B: Am, Em, F, C, Dm, G, Em, F/G |

## Code

Paste this into the editor at strudel.cc and press Ctrl+Enter.

```javascript
// "Route 7 Ramble (Night)" - night-time version of the original overworld loop
// same melody, chords and form as the day version; softer and slower
// form: A A B (24 bars), each section 8 bars, one bar per cycle
setcps(110/60/4)

// plays section a twice, then section b once (8 bars each)
const form = (a, b) => arrange([8, a], [8, a], [8, b])

// ---------- chords ----------
const chords = form(
  "<C F G C Am F [Dm G] C>",
  "<Am Em F C Dm G Em [F G]>"
)

// ---------- lead melody (same notes as the day version) ----------
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

// soft triangle lead with slow vibrato and a gentle echo
const lead = form(leadA, leadB)
  .s("triangle").gain(.35)
  .attack(.02).decay(.3).sustain(.5).release(.2)
  .lpf(2500).vib("3:.12")
  .delay(.25).delaytime(.375).delayfeedback(.3)
  .room(.3)

// ---------- twinkling arpeggio, an octave higher ----------
const arp = n("[0 1 2 3]*2").chord(chords).anchor("c6").voicing()
  .s("sine").gain(.05).decay(.15).sustain(0)
  .delay(.3).delaytime(.25).delayfeedback(.4)

// ---------- long, calm bass notes ----------
const bassA = note(`<
  [c2@4 g2@2 c3@2]
  [f2@4 c3@2 f3@2]
  [g2@4 d3@2 g3@2]
  [c2@4 g2@2 e2@2]
  [a1@4 e2@2 a2@2]
  [f2@4 c3@2 f3@2]
  [d2@2 a2@2 g2@2 d3@2]
  [c2@4 g2@2 b1@2]
>`)
const bassB = note(`<
  [a1@4 e2@2 a2@2]
  [e2@4 b2@2 e3@2]
  [f2@4 c3@2 f3@2]
  [c2@4 g2@2 c3@2]
  [d2@4 a2@2 d3@2]
  [g2@4 d3@2 g3@2]
  [e2@4 b2@2 e3@2]
  [f2@2 c3@2 g2@2 b2@2]
>`)

const bass = form(bassA, bassB)
  .s("triangle").gain(1).attack(.01).decay(.4).sustain(.6).release(.2)

// ---------- gentle drums: soft kick, quiet off-beat hats, no snare ----------
const kick = s("bd ~ ~ ~ ~ ~ ~ ~").gain(.2).lpf(800)
const hats = s("[~ white]*4").decay(.02).sustain(0).hpf(9000).gain(.04)

// ---------- crickets (random) ----------
const crickets = note("~ ~ ~ ~ ~ [e7 e7 e7] ~ ~").s("sine")
  .decay(.02).sustain(0).gain(.06).degradeBy(.6)

stack(lead, arp, bass, kick, hats, crickets)
```

## Differences from the day version

| Part | Day | Night |
| --- | --- | --- |
| Tempo | 140 BPM | 110 BPM |
| Lead | Bright `square` with vibrato | Soft `triangle` with slow vibrato, echo and a little reverb |
| Arpeggio | Quiet `square` below C5 | Twinkling `sine` an octave higher, with echo |
| Bass | Bouncy octave jumps | Long, calm notes on each chord |
| Drums | Kick, noise snare and eighth-note hats | Soft muffled kick on beat 1 and quiet off-beat hats, no snare |
| Extras | None | Crickets: quick high chirps that play at random |

## Exports

Exported at three tempos, from cycle 0 to 24 (one full pass, loops cleanly).

| File | Tempo | Length |
| --- | --- | --- |
| `route7-ramble-night-110bpm.wav` | 110 BPM | 52.4 s |
| `route7-ramble-night-140bpm.wav` | 140 BPM | 41.1 s |
| `route7-ramble-night-150bpm.wav` | 150 BPM | 38.4 s |
