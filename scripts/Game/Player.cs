using System;
using System.Diagnostics;
using System.Linq;
using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.CharacterState;
using TnT.Extensions;
using TnT.Input;
using TnT.Systems;

public partial class Player : Node, IInputActionable//, IBind<Player.PlayerSaveData>
{
	public static Player Instance { get; private set; }

	[Export]
	public InputActionBase[] InputActions { get; set; } =
	[
		new InputAction2D {
			ActionName = "move",
			Enabled = true,
			NegativeX = new InputAction { InputReference = "move_left",  Enabled = true },
			PositiveX = new InputAction { InputReference = "move_right", Enabled = true },
			NegativeY = new InputAction { InputReference = "move_up",  Enabled = true },
			PositiveY = new InputAction { InputReference = "move_down",    Enabled = true }
		},
		new InputAction   { ActionName = "jump", InputReference = "jump", Enabled = true }
	];

	InputAction2D MoveAction => InputActions.FirstOrDefault(a => a.ActionName == "move") as InputAction2D;
	InputAction JumpAction => InputActions.FirstOrDefault(a => a.ActionName == "jump") as InputAction;


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

	// private void StopMoving(InputActionBase action)
	// {
	// 	if (GetTree().Paused) return;
	// 	if (!_inputActive || MoveAction.IsPressed || _stateManager.IsAutonomousBehaviorActive) return;
	// 	_inputActive = false;
	// 	_stateManager.Pop();
	// }

	// private void StartMoving(InputActionBase action)
	// {
	// 	if (GetTree().Paused) return;
	// 	if (_inputActive || _stateManager.IsAutonomousBehaviorActive) return;
	// 	_inputActive = true;
	// 	_stateManager.StartInput(MoveAction, JumpAction);
	// }

	public void MoveTo(Vector3 target)
	{
		_cc.MoveTo(target);
	}
}
