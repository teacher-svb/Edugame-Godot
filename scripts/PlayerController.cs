using Godot;
using System;
using TnT.Systems;

public partial class PlayerController : Node
{
	[Export]
	Character character;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		if (direction.Length() > 0)
			character.Move(direction);
	}
}
