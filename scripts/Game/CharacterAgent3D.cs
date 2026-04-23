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
	Vector3 direction;

	CharacterController3D _cc;

	public override void _Ready()
	{
		_cc = this.FindAncestorOfType<CharacterController3D>();
		if (_currentIndex < _targets.Length)
		TargetPosition = _targets[_currentIndex].GlobalPosition;
	}

	public override void _Process(double delta)
	{
		if (_currentIndex >= _targets.Length)
			return;

		var nextPos = GetNextPathPosition();
		direction = nextPos - _cc.GlobalPosition;
		_cc.Move(direction.ToVector2XZ());
	}

	async void FindPath()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		_currentIndex++;

		if (_currentIndex >= _targets.Length && _loop)
			_currentIndex = 0;

		if (_currentIndex >= _targets.Length)
			return;

		TargetPosition = _targets[_currentIndex].GlobalPosition;
		GD.Print($"new target: {TargetPosition}, reachable: {IsTargetReachable()}");
	}
}
