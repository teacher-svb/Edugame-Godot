using Godot;

namespace TnT.EduGame.Audio
{
    /// <summary>
    /// Drop into a scene to choose its background music. When the scene loads, the
    /// <see cref="MusicManager"/> crossfades to <see cref="Track"/>.
    /// </summary>
    [GlobalClass]
    public partial class SceneMusic : Node
    {
        [Export] public MusicTrack Track { get; set; }
        [Export(PropertyHint.Range, "0,10,0.1,suffix:s")] public float FadeSeconds { get; set; } = 1f;

        public override void _Ready()
        {
            if (MusicManager.Instance == null)
            {
                GD.PushWarning($"{nameof(SceneMusic)}: no {nameof(MusicManager)} found, music not played.");
                return;
            }
            MusicManager.Instance.Play(Track, FadeSeconds);
        }
    }
}
