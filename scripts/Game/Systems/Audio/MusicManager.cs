using Godot;

namespace TnT.EduGame.Audio
{
    /// <summary>
    /// Plays the background music and crossfades between tracks. Lives in the Managers autoload,
    /// so music keeps playing across scene changes. Scenes pick their track with a <see cref="SceneMusic"/> node.
    /// </summary>
    public partial class MusicManager : Node
    {
        public static MusicManager Instance { get; private set; }

        [Export] StringName _bus = "Music";
        [Export(PropertyHint.Range, "-60,6,0.1,suffix:dB")] float _volumeDb = -20.7f;

        AudioStreamPlayer _current;
        AudioStreamPlayer _previous;
        Tween _fade;

        public MusicTrack CurrentTrack { get; private set; }

        public override void _EnterTree()
        {
            Instance = this;
            ProcessMode = ProcessModeEnum.Always; // keep playing while the game is paused

            _current = CreatePlayer("MusicA");
            _previous = CreatePlayer("MusicB");
        }

        public override void _ExitTree()
        {
            if (Instance == this)
                Instance = null;
        }

        AudioStreamPlayer CreatePlayer(string name)
        {
            var player = new AudioStreamPlayer { Name = name, Bus = _bus, VolumeLinear = 0f };
            AddChild(player);
            return player;
        }

        float TargetVolume => Mathf.DbToLinear(_volumeDb);

        /// <summary>
        /// Crossfades to <paramref name="track"/>. Does nothing if it is already playing.
        /// A version of the same song (same <see cref="MusicTrack.SongId"/>) continues at the same beat.
        /// Pass null to fade out.
        /// </summary>
        public void Play(MusicTrack track, float fadeSeconds = 1f)
        {
            if (track == CurrentTrack && (track == null || _current.Playing))
                return;

            var startAt = track != null ? StartPositionFor(track) : 0.0;

            (_current, _previous) = (_previous, _current);
            CurrentTrack = track;

            _fade?.Kill();
            _current.Stop();
            _current.VolumeLinear = 0f;

            if (track?.Stream != null)
            {
                _current.Stream = track.Stream;
                _current.Play((float)startAt);
            }

            _fade = CreateTween().SetParallel();
            var duration = Mathf.Max(fadeSeconds, 0.01f);
            if (_current.Playing)
                _fade.TweenProperty(_current, "volume_linear", TargetVolume, duration);
            _fade.TweenProperty(_previous, "volume_linear", 0f, duration);
            _fade.Chain().TweenCallback(Callable.From(_previous.Stop));
        }

        /// <summary>Maps the current beat onto the new track's tempo, wrapped to its length.</summary>
        double StartPositionFor(MusicTrack next)
        {
            if (!next.IsSameSongAs(CurrentTrack) || !_current.Playing || next.Stream == null)
                return 0.0;

            var position = _current.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix();
            var beat = position / CurrentTrack.SecondsPerBeat;
            var mapped = beat * next.SecondsPerBeat;

            var length = next.Stream.GetLength();
            return length > 0 ? mapped % length : 0.0;
        }
    }
}
