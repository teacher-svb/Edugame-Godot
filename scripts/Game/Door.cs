using Godot;
using System;
using TnT.EduGame.QuestSystem;
using TnT.Extensions;

public partial class Door : StaticBody2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var listener = this.FindAnyObjectByType<QuestEventListener>();
		listener.OnListen += ToggleDoor;
	}

	private void ToggleDoor(Variant value)
	{
		if (value.Obj is QuestObjective o)
		{
			var col = this.FindAnyObjectByType<CollisionShape2D>();
			GD.Print($"Opening door for objective {o.ObjectiveId} when state changed to {o.State}");

			col.SetDeferred("disabled", !col.Disabled);
			// col.Disabled = !col.Disabled;
		}
	}
}
