// CharacterAnimator.cs — owns all animation knowledge
using Godot;

public interface IMovementAnimator
{
    void OnMovementStateChanged(string state);
}

public partial class CharacterAnimator : Node, IMovementAnimator
{
    [Export] public AnimationTree AnimationTree { get; set; }

    // Maps movement states to animation names — easily editable,
    // or replace with a Resource-based mapping for designer-friendly editing
    private readonly System.Collections.Generic.Dictionary<string, string> _stateMap = new()
    {
        { "idle",            "idle"  },
        { "moving",          "walk"  },
        { "airborne_rising", "jump"  },
        { "airborne_falling","fall"  },
    };

    private AnimationNodeStateMachinePlayback _stateMachine;

    public override void _Ready()
    {
        if (AnimationTree == null) return;
        AnimationTree.Active = true;
        _stateMachine = (AnimationNodeStateMachinePlayback)
            AnimationTree.Get("parameters/playback");
    }

    public void OnMovementStateChanged(string movementState)
    {
        if (_stateMachine == null) return;
        if (_stateMap.TryGetValue(movementState, out var animName))
            _stateMachine.Travel(animName);
    }
}