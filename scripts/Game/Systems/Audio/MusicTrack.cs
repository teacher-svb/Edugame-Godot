using Godot;

namespace TnT.EduGame.Audio
{
    /// <summary>
    /// A music stream plus the tempo info needed to switch between versions of the same song
    /// without losing the musical position (e.g. the 110 and 140 BPM versions of a theme).
    /// </summary>
    [GlobalClass, Tool]
    public partial class MusicTrack : Resource
    {
        [Export] public AudioStream Stream { get; set; }

        /// <summary>Tempo of the stream, used to map the playback position between tracks.</summary>
        [Export(PropertyHint.Range, "1,300,0.1")] public float Bpm { get; set; } = 120f;

        /// <summary>
        /// Tracks with the same (non-empty) song id are versions of one song: switching between
        /// them continues at the same beat instead of starting over.
        /// </summary>
        [Export] public string SongId { get; set; } = "";

        public double SecondsPerBeat => 60.0 / Bpm;

        public bool IsSameSongAs(MusicTrack other) =>
            other != null && !string.IsNullOrEmpty(SongId) && SongId == other.SongId;
    }
}
