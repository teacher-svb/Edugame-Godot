using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.Levels
{
    /// <summary>
    /// Drives an animated prop for the level and its quests (methods take plain strings: CallMethodStep args), e.g. the drawbridge in the Act II town.
    /// On load it holds the pose at the end (or start) of <see cref="StartAnimation"/> (the bridge raised). Quests use
    /// <see cref="Play"/> (a <c>CallMethodStep</c> waits until the animation has finished: the bridge lowering) and
    /// <see cref="SetPose"/> (jump without playing: a <c>QuestsCompletedListener</c> that puts the bridge down again
    /// after loading). Catch-ups run once the scene is ready and bound, so the node order relative to <c>Quests</c>
    /// doesn't matter. Don't call <see cref="SetPose"/> on an animation while a <see cref="Play"/> of it is awaited.
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

        /// <summary>
        /// Plays <paramref name="animation"/> from its start; the task ends when that animation has finished (or another
        /// one replaced it), not when any animation of the player finishes. A looping animation never finishes: it is
        /// started with a warning and the task ends at once.
        /// </summary>
        public Task Play(string animation)
        {
            if (!Has(animation))
                return Task.CompletedTask;
            Player.Play(animation);
            Player.Seek(0, true);
            if (Player.GetAnimation(animation).LoopMode != Animation.LoopModeEnum.None)
            {
                GD.PushWarning($"{GetPath()}: '{animation}' loops and never finishes; Play doesn't wait for it");
                return Task.CompletedTask;
            }

            // a freed player (scene change) emits nothing: the task stays pending, as a quest step's wait should
            var done = new TaskCompletionSource();
            var player = Player;
            void Finished(StringName name) { if (name == animation) End(); }
            void Changed(StringName name) { if (name != animation) End(); }   // Play(other) / SetPose(other) took over
            void End()
            {
                player.AnimationFinished -= Finished;
                player.CurrentAnimationChanged -= Changed;
                done.TrySetResult();
            }
            player.AnimationFinished += Finished;
            player.CurrentAnimationChanged += Changed;
            return done.Task;
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
