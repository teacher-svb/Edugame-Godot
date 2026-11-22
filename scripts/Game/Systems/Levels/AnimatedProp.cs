using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.Levels
{
    /// <summary>
    /// Drives an animated prop for the level and its quests (methods take plain strings: CallMethodStep args), e.g. the drawbridge in the Act II town.
    /// On load it holds the pose at the end (or start) of <see cref="StartAnimation"/> (the bridge raised). Quests use
    /// <see cref="Play"/> (a <c>CallMethodStep</c> waits until the animation has finished: the bridge lowering) and
    /// <see cref="SetPose"/> (jump without playing: a <c>QuestsCompletedListener</c> that puts the bridge down again
    /// after loading). Place this node before the <c>Quests</c> node, so a quest's catch-up runs after it and wins.
    /// </summary>
    [GlobalClass]
    public partial class AnimatedProp : Node
    {
        [Export] public AnimationPlayer Player { get; set; }
        [Export] public StringName StartAnimation { get; set; }
        /// <summary>On: the pose at the end of <see cref="StartAnimation"/>; off: the pose at its start.</summary>
        [Export] public bool StartAtEnd { get; set; } = true;

        public override void _Ready()
        {
            if (StartAnimation != null && !StartAnimation.IsEmpty)
                SetPose(StartAnimation, StartAtEnd);
        }

        /// <summary>Plays <paramref name="animation"/> from its start; the task ends when it has finished.</summary>
        public async Task Play(string animation)
        {
            if (!Has(animation))
                return;
            Player.Play(animation);
            Player.Seek(0, true);
            await ToSignal(Player, AnimationMixer.SignalName.AnimationFinished);
        }

        /// <summary>Jumps to the end (or start) of <paramref name="animation"/> and holds that pose.</summary>
        public void SetPose(string animation, bool atEnd = true)
        {
            if (!Has(animation))
                return;
            Player.Play(animation);
            Player.Seek(atEnd ? Player.CurrentAnimationLength : 0, true);
            Player.Pause();
        }

        bool Has(string animation)
        {
            if (Player != null && !string.IsNullOrEmpty(animation) && Player.HasAnimation(animation))
                return true;
            GD.PushWarning($"{GetPath()}: no AnimationPlayer or no animation '{animation}'");
            return false;
        }
    }
}
