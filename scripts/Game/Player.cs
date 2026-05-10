using System;
using System.Diagnostics;
using System.Linq;
using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.CharacterState;
using TnT.EduGame.GameState;
using TnT.Extensions;
using TnT.Input;
using TnT.Systems;

public partial class Player : Node
{
	public static Player Instance { get; private set; }


	InputAction2D MoveAction => StateManagerGame.Instance.MovePlayerAction;
	InputAction JumpAction =>StateManagerGame.Instance.JumpPlayerAction;


	CharacterController3D _cc;
	[Export] CharacterStateManager _stateManager;
	bool _inputActive;

	public static Player Create()
	{
		var playerScene = (PackedScene)GD.Load("res://Scenes/player.tscn");
		var player = (Player)playerScene.Instantiate();
		return player;
	}

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Pausable;
		Instance = this;
		_cc = this.FindAncestorOfType<Character3D>().FindAnyObjectByType<CharacterController3D>();
		base._Ready();
	}

	public override void _Process(double delta)
	{
		if (_stateManager.IsAutonomousBehaviorActive)
			return;

		if (MoveAction.Triggered || JumpAction.Triggered)
			_stateManager.StartInput(MoveAction, JumpAction);
		else if (MoveAction.IsPressed == false && JumpAction.Triggered == false)
			_ = _stateManager.Pop();
	}

	public void MoveTo(Vector3 target)
	{
		_cc.MoveTo(target);
	}
}
