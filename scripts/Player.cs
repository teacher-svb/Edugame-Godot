using Godot;
using System;
using TnT.Systems;

public partial class Player : CharacterController2D
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);

		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		if (direction.Length() > 0)
			this.Move(direction);
	}
}
