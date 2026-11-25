using System;
using System.Diagnostics;
using System.Linq;
using Godot;
using TnT.EduGame.Characters.States;
using TnT.EduGame.GameStates;
using TnT.Extensions;
using TnT.Input;
using TnT.Systems.OcclusionFade;

namespace TnT.EduGame.Characters
{
	public partial class Player : Node, IOcclusionTarget
	{
		public static Player Instance { get; private set; }


		InputAction2D MoveAction => StateManagerGame.Instance.MovePlayerAction;
		InputAction JumpAction =>StateManagerGame.Instance.JumpPlayerAction;


		CharacterController3D _cc;
		[Export] CharacterStateManager _stateManager;
		bool _inputActive;

		// resolved lazily: the OcclusionFader can ask before our _Ready
		CharacterController3D Controller => _cc ??= this.FindAncestorOfType<Character3D>().FindAnyObjectByType<CharacterController3D>();
		Node3D IOcclusionTarget.OcclusionBody => this.FindAncestorOfType<Character3D>();
		Camera3D IOcclusionTarget.OcclusionCamera => Controller.Camera;
		RayCast3D IOcclusionTarget.OcclusionRaycast => Controller.OcclusionRaycast;

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
			_cc = Controller;
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
}
