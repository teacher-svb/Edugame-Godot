using Godot;
using System;
using TnT.Extensions;
using TnT.Systems;

[GlobalClass]
public partial class CharacterAgent3D : NavigationAgent3D
{
	[Export]
	Node3D[] _targets = Array.Empty<Node3D>();
	int _currentIndex = 0;
	[Export]
	bool _loop = false;

	CharacterController3D _cc;

	public override void _Ready()
	{
		_cc = this.FindAncestorOfType<CharacterController3D>();
	}

	public override void _Process(double delta)
	{
		TargetPosition = _targets[_currentIndex].GlobalPosition;

		if (_currentIndex >= _targets.Length)
			return;

		var direction = GetNextPathPosition() - _cc.GlobalPosition;

		// _cc.Move(direction.ToVector2XZ());
	}

	public void FindPath()
	{
		GD.Print($"finding path... {TargetPosition}");
		if (DistanceToTarget() < .3f)
			_currentIndex++;

		if (_currentIndex >= _targets.Length && _loop == true)
			_currentIndex = 0;
		
		if (_currentIndex >= _targets.Length)
			return;

		TargetPosition = _targets[_currentIndex].GlobalPosition;
		
		GD.Print($"new target... {IsTargetReachable()}");
	}
}
