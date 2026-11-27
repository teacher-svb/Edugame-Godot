using Godot;
using TnT.EduGame.Characters.States;
using TnT.Extensions;
using TnT.Input;
using TnT.Systems.OcclusionFade;

namespace TnT.EduGame.Characters
{
	public partial class Player : Node, IOcclusionTarget
	{
		public static Player Instance { get; private set; }


		// set by the play state (StateManagerGame.StartPlay): the same action instances it enables and disables
		InputAction2D _moveAction;
		InputAction _jumpAction;


		CharacterController3D _cc;
		[Export] CharacterStateManager _stateManager;

		// resolved lazily: the OcclusionFader can ask before our _Ready
		CharacterController3D Controller => _cc ??= this.FindAncestorOfType<Character3D>().FindAnyObjectByType<CharacterController3D>(includeSelf: true);
		Node3D IOcclusionTarget.OcclusionBody => this.FindAncestorOfType<Character3D>();
		Camera3D IOcclusionTarget.OcclusionCamera => Controller.Camera;

		/// <summary>
		/// Whether <paramref name="body"/> (e.g. what entered an area) is the player: it or a node below it is a
		/// <see cref="Player"/>. The one check for areas that react to the player.
		/// </summary>
		public static bool IsPlayer(Node body) => body != null && body.FindAnyObjectByType<Player>(includeSelf: true) != null;

		public override void _Ready()
		{
			ProcessMode = ProcessModeEnum.Pausable;
			Instance = this;
			_cc = Controller;
			base._Ready();
		}

		/// <summary>The actions that move the player; until they are set, the player gets no input.</summary>
		public void SetInputActions(InputAction2D moveAction, InputAction jumpAction)
		{
			_moveAction = moveAction;
			_jumpAction = jumpAction;
		}

		public override void _Process(double delta)
		{
			if (_stateManager.IsAutonomousBehaviorActive || _moveAction == null || _jumpAction == null)
				return;

			// IsPressed, not Triggered (a one-frame edge): a key held through a load or a closing dialogue moves the
			// player too (finding 4.9). StartInput does nothing while the move input state is already on the stack.
			if (_moveAction.IsPressed || _jumpAction.Triggered)
				_stateManager.StartInput(_moveAction, _jumpAction);
			else
				// only the move input state, not whatever else is on top (an IdleStep's idle; finding 4.11)
				_stateManager.StopInput();
		}

		public void MoveTo(Vector3 target)
		{
			_cc.MoveTo(target);
		}
	}
}
