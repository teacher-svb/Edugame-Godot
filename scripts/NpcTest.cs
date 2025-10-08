using Godot;
using System;
using TnT.Extensions;

public partial class NpcTest : CharacterBody2D
{
    NavigationAgent2D _agent;

    [Export]
    Node2D _target;

    [Export]
    float _speed = 300;
    [Export]
    float _acceleration = 7;

    public override void _Ready()
    {
        _agent = GetTree().FindAnyObjectByType<NavigationAgent2D>();
        _agent.TargetPosition = _target.GlobalPosition;
    }
    public override void _PhysicsProcess(double delta)
    {
        var direction = _agent.GetNextPathPosition() - GlobalPosition;

        Velocity = Velocity.Lerp(direction.Normalized() * _speed, _acceleration * (float)delta);

        MoveAndSlide();
    }

    public void FindPath()
    {
        _agent.TargetPosition = _target.GlobalPosition;
    }
}
