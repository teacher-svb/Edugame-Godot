using Godot;
using System;
using TnT.Extensions;
using TnT.Systems;

[GlobalClass]
public partial class CharacterAgent2D : NavigationAgent2D
{
	[Export]
	Node2D[] _targets = Array.Empty<Node2D>();
	int _currentIndex = 0;
	[Export]
	bool _loop = false;

	CharacterController2D _cc;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_cc = this.FindAncestorOfType<CharacterController2D>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (_currentIndex >= _targets.Length)
			return;

		var direction = GetNextPathPosition() - _cc.GlobalPosition;
		// direction = direction.Snapped(1);
		// direction = new Vector2(Mathf.Sign(direction.X), Mathf.Sign(direction.Y));

		_cc.Move(direction.Normalized());
	}

	public void FindPath()
	{
		if (DistanceToTarget() < 8)
			_currentIndex++;

		if (_currentIndex >= _targets.Length && _loop == true)
			_currentIndex = 0;
		
		if (_currentIndex >= _targets.Length)
			return;

		TargetPosition = _targets[_currentIndex].GlobalPosition.Snap();
	}
}
