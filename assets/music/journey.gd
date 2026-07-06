extends Node
# Fantasy Medieval Music Generator for Godot
# A heroic journey through ancient kingdoms

# Audio generation settings
const SAMPLE_RATE = 44100
const BUFFER_SIZE = 1024

# Music tempo (90 BPM)
const BPM = 90.0
const BEAT_DURATION = 60.0 / BPM  # Duration of one beat in seconds
const MEASURE_DURATION = BEAT_DURATION * 4  # 4/4 time

# Note frequencies (Dorian scale starting from D)
const NOTES = {
	"D2": 73.42, "E2": 82.41, "F2": 87.31, "G2": 98.00, "A2": 110.00, "Bb2": 116.54, "C2": 65.41,
	"D3": 146.83, "E3": 164.81, "F3": 174.61, "G3": 196.00, "A3": 220.00, "Bb3": 233.08, "C3": 130.81,
	"D4": 293.66, "E4": 329.63, "F4": 349.23, "G4": 392.00, "A4": 440.00, "Bb4": 466.16, "C4": 261.63,
	"D5": 587.33, "E5": 659.25, "F5": 698.46, "G5": 783.99, "A5": 880.00, "Bb5": 932.33, "C5": 523.25,
}

# Oscillator types
enum WaveType { SINE, SQUARE, SAWTOOTH, TRIANGLE }

# Audio stream setup
var playback: AudioStreamGeneratorPlayback
var stream_player: AudioStreamPlayer
var phase = 0.0
var time = 0.0

# Active voices (polyphonic synthesis)
var active_notes = []

# Music state
var current_section = "A"
var section_time = 0.0
var section_change_interval = MEASURE_DURATION * 16  # Switch sections every 16 measures
var beat_counter = 0.0

# Instrument definitions
class Voice:
	var frequency: float = 440.0
	var amplitude: float = 0.5
	var phase: float = 0.0
	var wave_type: int = WaveType.SINE
	var envelope_time: float = 0.0
	var attack: float = 0.01
	var decay: float = 0.3
	var sustain: float = 0.5
	var release: float = 0.8
	var duration: float = 0.5
	var is_releasing: bool = false
	var release_start_time: float = 0.0
	var filter_cutoff: float = 20000.0
	var gain: float = 1.0
	
	func get_envelope() -> float:
		if is_releasing:
			var release_time = envelope_time - release_start_time
			if release_time >= release:
				return 0.0
			return sustain * (1.0 - release_time / release)
		elif envelope_time < attack:
			return envelope_time / attack
		elif envelope_time < attack + decay:
			var decay_progress = (envelope_time - attack) / decay
			return 1.0 - (1.0 - sustain) * decay_progress
		else:
			return sustain
	
	func generate_sample(delta: float) -> float:
		envelope_time += delta
		
		var sample = 0.0
		match wave_type:
			WaveType.SINE:
				sample = sin(phase * TAU)
			WaveType.SAWTOOTH:
				sample = 2.0 * (phase - floor(phase + 0.5))
			WaveType.SQUARE:
				sample = 1.0 if phase < 0.5 else -1.0
			WaveType.TRIANGLE:
				sample = 4.0 * abs(phase - 0.5) - 1.0
		
		phase += frequency / SAMPLE_RATE
		if phase >= 1.0:
			phase -= 1.0
		
		return sample * get_envelope() * amplitude * gain
	
	func is_finished() -> bool:
		return is_releasing and envelope_time >= release_start_time + release
	
	func trigger_release():
		if not is_releasing:
			is_releasing = true
			release_start_time = envelope_time

# Musical patterns for Section A (Castle Courtyard)
var section_a_melody = [
	{"note": "D4", "time": 0.0},
	{"note": "E4", "time": 1.0},
	{"note": "F4", "time": 2.0},
	{"note": "G4", "time": 3.0},
	{"note": "A4", "time": 4.0},
	{"note": "G4", "time": 5.0},
	{"note": "F4", "time": 5.5},
	{"note": "E4", "time": 6.0},
	{"note": "D4", "time": 7.0},
]

var section_a_harp = [
	{"notes": ["D3", "F3", "A3", "D4"], "time": 0.0},
	{"notes": ["C3", "E3", "G3", "C4"], "time": 4.0},
	{"notes": ["Bb2", "D3", "F3", "Bb3"], "time": 8.0},
	{"notes": ["A2", "C3", "E3", "A3"], "time": 12.0},
]

var section_a_flute = [
	{"note": "A4", "time": 1.0},
	{"note": "G4", "time": 3.0},
	{"note": "F4", "time": 5.0},
	{"note": "E4", "time": 6.0},
]

var section_a_drums = [
	{"type": "kick", "time": 0.0},
	{"type": "kick", "time": 3.0},
	{"type": "snare", "time": 5.0},
	{"type": "hihat", "time": 2.0},
	{"type": "hihat", "time": 5.0},
	{"type": "hihat", "time": 7.0},
]

# Musical patterns for Section B (Heroic Journey)
var section_b_melody = [
	{"note": "A4", "time": 0.0},
	{"note": "G4", "time": 1.0},
	{"note": "F4", "time": 2.0},
	{"note": "E4", "time": 3.0},
	{"note": "D4", "time": 4.0},
	{"note": "E4", "time": 5.0},
	{"note": "F4", "time": 6.0},
	{"note": "G4", "time": 7.0},
	{"note": "A4", "time": 8.0},
	{"note": "G4", "time": 9.0},
]

var section_b_harp = [
	{"notes": ["D3", "F3", "A3", "D4"], "time": 0.0},
	{"notes": ["A2", "C3", "E3", "A3"], "time": 4.0},
	{"notes": ["Bb2", "D3", "F3", "Bb3"], "time": 8.0},
	{"notes": ["C3", "E3", "G3", "C4"], "time": 12.0},
]

var section_b_flute = [
	{"note": "D5", "time": 0.0},
	{"note": "C5", "time": 0.5},
	{"note": "A4", "time": 1.0},
	{"note": "G4", "time": 2.0},
	{"note": "F4", "time": 3.0},
	{"note": "E4", "time": 4.0},
	{"note": "G4", "time": 4.5},
	{"note": "A4", "time": 5.0},
]

var section_b_drums = [
	{"type": "kick", "time": 0.0},
	{"type": "kick", "time": 2.0},
	{"type": "kick", "time": 4.0},
	{"type": "snare", "time": 5.0},
	{"type": "snare", "time": 7.0},
	{"type": "hihat", "time": 1.0},
	{"type": "hihat", "time": 2.0},
	{"type": "hihat", "time": 4.0},
	{"type": "hihat", "time": 5.0},
	{"type": "hihat", "time": 7.0},
]

# Persistent elements
var bass_drone_time = 0.0
var bell_toll_time = 0.0

func _ready():
	# Create audio stream player
	stream_player = AudioStreamPlayer.new()
	add_child(stream_player)
	
	# Create generator stream
	var stream = AudioStreamGenerator.new()
	stream.mix_rate = SAMPLE_RATE
	stream.buffer_length = 0.1
	
	stream_player.stream = stream
	stream_player.play()
	playback = stream_player.get_stream_playback()
	
	print("Medieval Fantasy Music Generator Started!")
	print("Section A: The Castle Courtyard")

func _process(delta):
	time += delta
	section_time += delta
	beat_counter = fmod(time / BEAT_DURATION, 8.0)
	
	# Check for section changes (random between A and B)
	if section_time >= section_change_interval:
		section_time = 0.0
		current_section = "B" if randf() > 0.5 else "A"
		print("Switching to Section ", current_section)
		if current_section == "A":
			print("🏰 The Castle Courtyard")
		else:
			print("⚔️ The Heroic Journey")
	
	# Trigger new notes based on current section
	trigger_section_notes()
	
	# Fill audio buffer
	fill_audio_buffer()

func trigger_section_notes():
	var measure_time = fmod(section_time, MEASURE_DURATION * 8)
	var beat_time = measure_time / BEAT_DURATION
	
	# Select patterns based on current section
	var melody = section_a_melody if current_section == "A" else section_b_melody
	var harp = section_a_harp if current_section == "A" else section_b_harp
	var flute = section_a_flute if current_section == "A" else section_b_flute
	var drums = section_a_drums if current_section == "A" else section_b_drums
	
	# Trigger melody notes
	for note_data in melody:
		if is_time_to_trigger(beat_time, note_data.time, 8.0):
			play_piano_note(note_data.note, 0.5)
	
	# Trigger harp arpeggios
	for chord_data in harp:
		if is_time_to_trigger(beat_time, chord_data.time, 16.0):
			play_harp_chord(chord_data.notes)
	
	# Trigger flute notes
	for note_data in flute:
		if is_time_to_trigger(beat_time, note_data.time, 8.0):
			play_flute_note(note_data.note, 0.7)
	
	# Trigger drums
	for drum_data in drums:
		if is_time_to_trigger(beat_time, drum_data.time, 8.0):
			match drum_data.type:
				"kick":
					play_kick()
				"snare":
					play_snare()
				"hihat":
					play_hihat()
	
	# Persistent bass drone (every 8 measures)
	if fmod(time, MEASURE_DURATION * 8) < 0.02:
		play_bass_drone()
	
	# Bell tolls (every 16 measures)
	if fmod(time, MEASURE_DURATION * 16) < 0.02:
		play_bells()

func is_time_to_trigger(current_beat: float, trigger_beat: float, loop_length: float) -> bool:
	var beat_mod = fmod(current_beat, loop_length)
	var prev_beat_mod = fmod(current_beat - 0.1, loop_length)
	
	if prev_beat_mod < 0:
		prev_beat_mod += loop_length
	
	return prev_beat_mod < trigger_beat and beat_mod >= trigger_beat

func play_piano_note(note: String, duration: float):
	var voice = Voice.new()
	voice.frequency = NOTES[note]
	voice.amplitude = 0.3
	voice.wave_type = WaveType.SAWTOOTH
	voice.attack = 0.01
	voice.decay = 0.3
	voice.sustain = 0.5
	voice.release = 0.8
	voice.duration = duration
	voice.filter_cutoff = 1200.0
	voice.gain = 0.8
	active_notes.append(voice)
	
	# Schedule release
	get_tree().create_timer(duration).timeout.connect(func(): voice.trigger_release())

func play_harp_chord(notes: Array):
	for i in range(notes.size()):
		var voice = Voice.new()
		voice.frequency = NOTES[notes[i]]
		voice.amplitude = 0.15
		voice.wave_type = WaveType.TRIANGLE
		voice.attack = 0.001
		voice.decay = 0.4
		voice.sustain = 0.3
		voice.release = 1.2
		voice.duration = 0.5
		voice.gain = 1.5
		active_notes.append(voice)
		
		# Stagger the arpeggio slightly
		await get_tree().create_timer(0.05 * i).timeout
		get_tree().create_timer(0.5).timeout.connect(func(): voice.trigger_release())

func play_flute_note(note: String, duration: float):
	var voice = Voice.new()
	voice.frequency = NOTES[note]
	voice.amplitude = 0.2
	voice.wave_type = WaveType.SINE
	voice.attack = 0.05
	voice.decay = 0.2
	voice.sustain = 0.6
	voice.release = 0.5
	voice.duration = duration
	voice.filter_cutoff = 2000.0
	voice.gain = 0.5
	active_notes.append(voice)
	
	get_tree().create_timer(duration).timeout.connect(func(): voice.trigger_release())

func play_bass_drone():
	var voice = Voice.new()
	voice.frequency = NOTES["D2"]
	voice.amplitude = 0.3
	voice.wave_type = WaveType.SAWTOOTH
	voice.attack = 0.5
	voice.decay = 1.0
	voice.sustain = 0.8
	voice.release = 2.0
	voice.duration = MEASURE_DURATION * 8 - 0.5
	voice.filter_cutoff = 200.0
	voice.gain = 0.5
	active_notes.append(voice)
	
	get_tree().create_timer(voice.duration).timeout.connect(func(): voice.trigger_release())

func play_bells():
	# D5 bell
	var bell1 = Voice.new()
	bell1.frequency = NOTES["D5"]
	bell1.amplitude = 0.15
	bell1.wave_type = WaveType.SINE
	bell1.attack = 0.001
	bell1.decay = 1.4
	bell1.sustain = 0.3
	bell1.release = 2.0
	bell1.duration = 2.0
	bell1.gain = 0.4
	active_notes.append(bell1)
	
	# A5 bell (slightly delayed)
	await get_tree().create_timer(0.1).timeout
	var bell2 = Voice.new()
	bell2.frequency = NOTES["A5"]
	bell2.amplitude = 0.15
	bell2.wave_type = WaveType.SINE
	bell2.attack = 0.001
	bell2.decay = 1.4
	bell2.sustain = 0.3
	bell2.release = 2.0
	bell2.duration = 2.0
	bell2.gain = 0.4
	active_notes.append(bell2)
	
	get_tree().create_timer(2.0).timeout.connect(func(): 
		bell1.trigger_release()
		bell2.trigger_release()
	)

func play_kick():
	var voice = Voice.new()
	voice.frequency = 60.0
	voice.amplitude = 0.6
	voice.wave_type = WaveType.SINE
	voice.attack = 0.001
	voice.decay = 0.2
	voice.sustain = 0.0
	voice.release = 0.1
	voice.duration = 0.15
	voice.gain = 0.7
	active_notes.append(voice)
	
	get_tree().create_timer(0.15).timeout.connect(func(): voice.trigger_release())

func play_snare():
	var voice = Voice.new()
	voice.frequency = 200.0
	voice.amplitude = 0.4
	voice.wave_type = WaveType.SQUARE
	voice.attack = 0.001
	voice.decay = 0.2
	voice.sustain = 0.0
	voice.release = 0.1
	voice.duration = 0.1
	voice.gain = 0.6
	active_notes.append(voice)
	
	get_tree().create_timer(0.1).timeout.connect(func(): voice.trigger_release())

func play_hihat():
	var voice = Voice.new()
	voice.frequency = 8000.0
	voice.amplitude = 0.15
	voice.wave_type = WaveType.SQUARE
	voice.attack = 0.001
	voice.decay = 0.1
	voice.sustain = 0.0
	voice.release = 0.05
	voice.duration = 0.05
	voice.gain = 0.4
	active_notes.append(voice)
	
	get_tree().create_timer(0.05).timeout.connect(func(): voice.trigger_release())

func fill_audio_buffer():
	if not playback:
		return
	
	var frames_available = playback.get_frames_available()
	if frames_available == 0:
		return
	
	var frames_to_fill = min(frames_available, BUFFER_SIZE)
	var buffer = PackedVector2Array()
	buffer.resize(frames_to_fill)
	
	var delta = 1.0 / SAMPLE_RATE
	
	for i in range(frames_to_fill):
		var sample = 0.0
		
		# Mix all active voices
		for voice in active_notes:
			sample += voice.generate_sample(delta)
		
		# Simple clipping
		sample = clamp(sample, -1.0, 1.0)
		
		# Stereo (same signal on both channels)
		buffer[i] = Vector2(sample, sample)
	
	# Remove finished voices
	active_notes = active_notes.filter(func(voice): return not voice.is_finished())
	
	# Push to audio stream
	playback.push_buffer(buffer)
