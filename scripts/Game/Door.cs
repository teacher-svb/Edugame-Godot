using Godot;
using TnT.Extensions;

namespace TnT.EduGame
{
	public partial class Door : Node3D
	{
		CollisionShape3D _col;
		AnimationPlayer _animPlayer;
		// the state the door is in or swinging towards; null until the first toggle (then the collision says, which Bind sets)
		bool? _open;
		int _toggles;   // bumped per toggle: a swing replaced by a newer toggle doesn't apply its state when it ends

		public override void _Ready()
		{
			_col = this.FindAnyObjectByType<CollisionShape3D>(includeSelf: true);
			_animPlayer = this.FindAnyObjectByType<AnimationPlayer>(includeSelf: true);
		}

		// Opens a closed door, closes an open one (wired to quest reactions). The collision changes once the swing has
		// ended: it waits for the animation itself (stops while the game is paused; Task.Delay ran on, and its
		// continuation could resume after a scene change or at quit). Saved as the state it swings to, so a save made
		// mid-swing loads it in its end pose (Bind).
		private async void ToggleDoor()
		{
			if (_col == null)
			{
				GD.PushWarning($"{Name}: door without a CollisionShape3D, can't toggle");
				return;
			}
			bool open = !(_open ?? _col.Disabled);   // collision off = open
			_open = open;
			int toggle = ++_toggles;
			if (_saveData != null)
				_saveData.IsOpen = open;

			var animName = open ? "door_open" : "door_close";
			if (_animPlayer != null && _animPlayer.HasAnimation(animName))
			{
				_animPlayer.Play(animName);
				await ToSignal(_animPlayer, AnimationMixer.SignalName.AnimationFinished);
				// freed meanwhile (scene change), or toggled again: the newer toggle decides
				if (!IsInstanceValid(this) || toggle != _toggles)
					return;
			}
			_col.SetDeferred(CollisionShape3D.PropertyName.Disabled, open);
		}
	}
}
