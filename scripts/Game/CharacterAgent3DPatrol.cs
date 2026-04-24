using Godot;
using System;
using TnT.Extensions;

[GlobalClass]
public partial class CharacterAgent3DPatrol : NavigationAgent3D
{
    [Export] Node3D[] _targets = Array.Empty<Node3D>();
    [Export] bool _loop = false;

    int _currentIndex = 0;
    Vector3 _direction;
    CharacterController3D _cc;

    public override void _Ready()
    {
        _cc = this.FindAncestorOfType<CharacterController3D>();
        NavigationFinished += AdvanceWaypoint;
        SetProcess(false);

        if (_currentIndex < _targets.Length)
            TargetPosition = _targets[_currentIndex].GlobalPosition;
    }

    public override void _Process(double delta)
    {
        if (_currentIndex >= _targets.Length)
            return;

        _direction = GetNextPathPosition() - _cc.GlobalPosition;
        _cc.Move(_direction.ToVector2XZ());
    }

    async void AdvanceWaypoint()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        _currentIndex++;

        if (_currentIndex >= _targets.Length && _loop)
            _currentIndex = 0;

        if (_currentIndex >= _targets.Length)
            return;

        TargetPosition = _targets[_currentIndex].GlobalPosition;
    }
}
