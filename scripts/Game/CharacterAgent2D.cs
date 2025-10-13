using Godot;
using System;
using TnT.Extensions;
using TnT.Systems;

[GlobalClass]
public partial class CharacterAgent2D : NavigationAgent2D
{
	[Export]
	Node2D _target;

	CharacterController2D _cc;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_cc = this.FindAncestorOfType<CharacterController2D>();
		TargetPosition = _target.GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_target == null)
			return;
		
		if (DistanceToTarget() < 8)
			return;

		var direction = GetNextPathPosition() - _cc.GlobalPosition;
		_cc.Move(direction.Normalized());
	}

	public void FindPath()
	{
		if (_target == null)
			return;
		TargetPosition = _target.GlobalPosition.Snap();
	}
}
