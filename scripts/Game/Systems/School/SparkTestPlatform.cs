using Godot;
using TnT.Extensions;
using TnT.EduGame.Characters;

namespace TnT.EduGame.School
{
    /// <summary>
    /// The Spark Test in a trial chamber's goal room (card act1-chamber-goal-rooms): the player stepping on it takes
    /// the test, which always fails for Mabel: a fizzle of sparks and a dying glow, then <see cref="Tested"/> for the
    /// quest to react to (connect it to <c>QuestNode.CompleteObjective</c> with the objective id). Taken again after
    /// stepping off. The fizzle keeps playing while a message pauses the game.
    /// </summary>
    [GlobalClass]
    public partial class SparkTestPlatform : Area3D
    {
        [Export] GpuParticles3D _sparks;
        [Export] OmniLight3D _glow;
        [Export] float _glowEnergy = 2.5f;

        [Signal] public delegate void TestedEventHandler();

        /// <summary>How often the test was taken in this level.</summary>
        public int Tests { get; private set; }

        bool _inside;
        Tween _tween;

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
            if (_glow != null)
                _glow.LightEnergy = 0;
        }

        static bool IsPlayer(Node body) => body.FindAnyObjectByType<Player>() != null;

        void OnBodyEntered(Node3D body)
        {
            if (!IsPlayer(body) || _inside)
                return;
            _inside = true;
            Tests++;
            Fizzle();
            EmitSignal(SignalName.Tested);
        }

        void OnBodyExited(Node3D body)
        {
            if (IsPlayer(body))
                _inside = false;
        }

        /// <summary>A spark that doesn't catch: sparks fly up and fall, the glow flares and dies.</summary>
        public void Fizzle()
        {
            _sparks?.Restart();
            if (_glow == null)
                return;
            _tween?.Kill();
            _tween = CreateTween().SetPauseMode(Tween.TweenPauseMode.Process);
            _tween.TweenProperty(_glow, "light_energy", _glowEnergy, 0.08);
            _tween.TweenProperty(_glow, "light_energy", _glowEnergy * 0.2f, 0.12);
            _tween.TweenProperty(_glow, "light_energy", _glowEnergy * 0.6f, 0.1);
            _tween.TweenProperty(_glow, "light_energy", 0f, 0.5).SetEase(Tween.EaseType.In);
        }
    }
}
