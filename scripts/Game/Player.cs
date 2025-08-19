using Godot;
using System;
using TnT.Systems;
using TnT.Systems.Persistence;

public partial class Player : CharacterController2D//, IBind<Player.PlayerSaveData>
{
	public static Player Instance { get; private set; }

	public static Player Create()
	{
		var playerScene = (PackedScene)GD.Load("res://Scenes/player.tscn");
		var player = (Player)playerScene.Instantiate();
		return player;
	}

	public override void _Ready()
	{
		Instance = this;
		base._Ready();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		if (direction.Length() > 0)
			this.Move(direction);
	}
}
