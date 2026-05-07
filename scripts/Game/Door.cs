using Godot;
using System;
using System.Threading.Tasks;
using TnT.EduGame.QuestSystem;
using TnT.Extensions;

public partial class Door : Node3D
{
	CollisionShape3D _col;
	AnimationPlayer _animPlayer;
	// Called when the node enters the scene tree for the first time.
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
		_col.SetDeferred("disabled", !_col.Disabled);
	}
}
