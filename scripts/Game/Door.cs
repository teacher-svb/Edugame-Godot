using Godot;
using System;
using System.Threading.Tasks;
using TnT.EduGame.QuestSystem;
using TnT.EduGame;
using TnT.Extensions;
using TnT.Systems.Persistence;

public partial class Door : Node3D, IBind<DoorSaveData>
{
	CollisionShape3D _col;
	AnimationPlayer _animPlayer;
	DoorSaveData _saveData;

	public string PersistentId { get; private set; }

	public override void _EnterTree()
	{
		PersistentId = GetPath().ToString();
	}

	public override void _Ready()
	{
		_col = this.FindAnyObjectByType<CollisionShape3D>();
		_animPlayer = this.FindAnyObjectByType<AnimationPlayer>();
	}

	public void Bind(DoorSaveData data)
	{
		GD.Print($"door bind {data} {data?.IsOpen}");
		_saveData = data;
		if (!data.IsNew && data.IsOpen)
		{
			_col ??= this.FindAnyObjectByType<CollisionShape3D>();
			_animPlayer ??= this.FindAnyObjectByType<AnimationPlayer>();
			_col.Disabled = true;
			_animPlayer?.Play("door_open");
			_animPlayer?.Advance(_animPlayer.CurrentAnimationLength);
		}
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
