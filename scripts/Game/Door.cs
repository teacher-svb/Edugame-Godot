using Godot;
using System;
using System.Threading.Tasks;
using TnT.EduGame.QuestSystem;
using TnT.EduGame;
using TnT.Extensions;
using TnT.Systems.Persistence;

namespace TnT.EduGame
{
	public partial class Door : Node3D
	{
		CollisionShape3D _col;
		AnimationPlayer _animPlayer;


		public override void _EnterTree()
		{
			PersistentId = GetPath().ToString();
		}

		public override void _Ready()
		{
			_col = this.FindAnyObjectByType<CollisionShape3D>();
			_animPlayer = this.FindAnyObjectByType<AnimationPlayer>();
		}

		private async void ToggleDoor()
		{
			if (_animPlayer != null)
			{
				var animName = _col.Disabled ? "door_close" : "door_open";
				_animPlayer.Play(animName);
				int durationMs = (int)(_animPlayer.CurrentAnimationLength * 1000);

				await Task.Delay(durationMs);
			}

			bool newIsOpen = !_col.Disabled;
			_col.SetDeferred("disabled", newIsOpen);
			if (_saveData != null)
				_saveData.IsOpen = newIsOpen;
		}
	}
}
