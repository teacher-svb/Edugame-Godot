using System;
using Godot;
using TnT.EduGame.CharacterState;
using TnT.Extensions;
using TnT.Input;
using TnT.Systems;

public partial class Player : Node//, IBind<Player.PlayerSaveData>
{
	public static Player Instance { get; private set; }

	[Export] InputAction2D _move;
	[Export] InputAction _jump;

	ICharacterController _cc;
	[Export] CharacterStateManager _stateManager;

	public static Player Create()
	{
		var playerScene = (PackedScene)GD.Load("res://Scenes/player.tscn");
		var player = (Player)playerScene.Instantiate();
		return player;
	}

	public override void _Ready()
	{
		Instance = this;
		_cc = this.FindAncestorOfType<CharacterController2D>();
		if (_cc == null)
			_cc = this.FindAncestorOfType<CharacterController3D>();
		base._Ready();

		_move.ActionPressed += StartMoving;
		_move.ActionReleased += StopMoving;
	}

    private void StopMoving()
    {
		_stateManager.Idling();
    }

    private void StartMoving()
	{
		_stateManager.StartInput(_move, _jump);
    }

    // public override void _Process(double delta)
	// {
	// 	base._Process(delta);

	// 	Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
	// 	if (direction.Length() > 0)
	// 		_cc.Move(direction);
	// }

	public void MoveTo(Vector3 target)
    {
		_cc.MoveTo(target);
    }
}
