// CharacterAnimator.cs — owns all animation knowledge
using Godot;
using TnT.Extensions;

namespace TnT.EduGame.Characters
{
    public partial class CharacterAnimator : Node, IMovementAnimator
    {
        AnimationTree AnimationTree { get; set; }
        /// <summary>The visual root whose scale is tweened for squash and stretch effects.</summary>
        [Export] Node3D VisualRoot { get; set; }
        [Export] GpuParticles3D LandingParticles { get; set; }

        private AnimationNodeStateMachinePlayback _stateMachine;
        private Tween _squashStretchTween;
        // Scale the visual root rests at. Squash/stretch is relative to it, because several
        // characters point VisualRoot at a model imported at a non-unit scale (e.g. 0.525).
        private Vector3 _restScale = Vector3.One;

        public override void _Ready()
        {
            if (VisualRoot != null)
                _restScale = VisualRoot.Scale;

            CharacterController3D cc = this.FindAncestorOfType<Character3D>()?.FindAnyObjectByType<CharacterController3D>(includeSelf: true);
            if (cc == null) return;
            cc.MovementStateChanged += OnMovementStateChanged;

            AnimationTree = this.FindAncestorOfType<Character3D>().FindAnyObjectByType<AnimationTree>(includeSelf: true);
            if (AnimationTree == null) return;
            AnimationTree.Active = true;
            _stateMachine = (AnimationNodeStateMachinePlayback)
                AnimationTree.Get("parameters/playback");
        }

        // a scripted state (Hold) that movement states don't replace, and the last movement state meanwhile
        string _held;
        string _movementState = "idle";

        /// <summary>
        /// Travels to <paramref name="state"/> of the AnimationTree's state machine and keeps it there: movement states
        /// ("idle", "run", "fall", ...) are ignored until <see cref="Release"/>. For a scripted sequence (a plaything:
        /// "jump", "sit_down", "sit", "stand_up"). A state the machine doesn't have is logged and ignored.
        /// </summary>
        public void Hold(string state)
        {
            if (_held == state)
                return;   // already there: callers may hold the same state every frame
            _held = state;
            if (HasState(state))
                _stateMachine.Travel(state);
            else
                GD.PushWarning($"{Owner?.Name}: the AnimationTree has no state '{state}'");
        }

        /// <summary>Ends <see cref="Hold"/>: back to the character's current movement state.</summary>
        public void Release()
        {
            _held = null;
            OnMovementStateChanged(_movementState);
        }

        /// <summary>Whether the AnimationTree's state machine has <paramref name="state"/>.</summary>
        public bool HasState(string state) =>
            _stateMachine != null && AnimationTree.TreeRoot is AnimationNodeStateMachine machine && machine.HasNode(state);

        /// <summary>Length in seconds of the animation <paramref name="animation"/> of the AnimationTree; 0 without it.</summary>
        public float AnimationLength(string animation) =>
            AnimationTree != null && AnimationTree.HasAnimation(animation) ? (float)AnimationTree.GetAnimation(animation).Length : 0f;

        public void OnMovementStateChanged(string movementState)
        {
            _movementState = movementState;
            if (_held != null)
                return;
            switch (movementState)
            {
                case "idle":
                case "walk":
                case "run":
                case "fall": _stateMachine.Travel(movementState); break;
                case "jump": _stateMachine.Travel(movementState); DoStretch(); break;
                case "land": _stateMachine.Travel(movementState); DoSquash(); break;
            }
        }

        // Elongate on Y, compress on X/Z, then spring back.
        private void DoStretch()
        {
            if (VisualRoot == null) return;
            _squashStretchTween?.Kill();
            _squashStretchTween = CreateTween();

            _squashStretchTween
                .TweenProperty(VisualRoot, "scale", _restScale * new Vector3(0.8f, 1.3f, 0.8f), 0.1f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.In);
            _squashStretchTween
                .TweenProperty(VisualRoot, "scale", _restScale, 0.22f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
        }

        // Flatten on Y, spread on X/Z, then spring back.
        private void DoSquash()
        {
            if (LandingParticles != null)
                LandingParticles.Emitting = true;

            if (VisualRoot == null) return;
            VisualRoot.Scale = _restScale * new Vector3(1.3f, .5f, 1.3f);
            _squashStretchTween?.Kill();
            _squashStretchTween = CreateTween();

            _squashStretchTween
                .TweenProperty(VisualRoot, "scale", _restScale, 0.22f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
        }
    }
}
