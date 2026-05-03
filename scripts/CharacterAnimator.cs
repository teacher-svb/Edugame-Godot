// CharacterAnimator.cs — owns all animation knowledge
using Godot;
using TnT.EduGame.Characters;
using TnT.Extensions;

public interface IMovementAnimator
{
    void OnMovementStateChanged(string state);
}

public partial class CharacterAnimator : Node, IMovementAnimator
{
    [Export] AnimationTree AnimationTree { get; set; }
    /// <summary>The visual root whose scale is tweened for squash and stretch effects.</summary>
    [Export] Node3D VisualRoot { get; set; }
    [Export] GpuParticles3D LandingParticles { get; set; }

    private AnimationNodeStateMachinePlayback _stateMachine;
    private Tween _squashStretchTween;

    public override void _Ready()
    {
        CharacterController3D cc = this.FindAncestorOfType<Character3D>().FindAnyObjectByType<CharacterController3D>();
        cc.MovementStateChanged += OnMovementStateChanged;
        if (AnimationTree == null) return;
        AnimationTree.Active = true;
        _stateMachine = (AnimationNodeStateMachinePlayback)
            AnimationTree.Get("parameters/playback");
    }

    public void OnMovementStateChanged(string movementState)
    {
        switch (movementState)
        {
            case "idle":
            case "walk":
            case "jump":
            case "fall": _stateMachine.Travel(movementState); break;
            case "jumped": DoStretch(); break;
            case "landed": DoSquash(); break;
        }
    }

    // Elongate on Y, compress on X/Z, then spring back.
    private void DoStretch()
    {
        if (VisualRoot == null) return;
        _squashStretchTween?.Kill();
        _squashStretchTween = CreateTween();

        _squashStretchTween
            .TweenProperty(VisualRoot, "scale", new Vector3(0.8f, 1.3f, 0.8f), 0.1f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);
        _squashStretchTween
            .TweenProperty(VisualRoot, "scale", Vector3.One, 0.22f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    // Flatten on Y, spread on X/Z, then spring back.
    private void DoSquash()
    {
        if (LandingParticles != null)
            LandingParticles.Emitting = true;
            
        if (VisualRoot == null) return;
        VisualRoot.Scale = new Vector3(1.3f, .5f, 1.3f);
        _squashStretchTween?.Kill();
        _squashStretchTween = CreateTween();

        _squashStretchTween
            .TweenProperty(VisualRoot, "scale", Vector3.One, 0.22f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }
}