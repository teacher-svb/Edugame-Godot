using Godot;
using System;
using TnT.Systems;
using TnT.Systems.Persistence;

public partial class Player : CharacterController2D, IBind<Player.PlayerSaveData>
{
	public static Player Instance { get; private set; }
	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public static Player Create()
	{
		var playerScene = (PackedScene)GD.Load("res://Scenes/player.tscn");
		var player = (Player)playerScene.Instantiate();
		return player;
	}

	public override void _Ready()
	{
		Instance = this;
		base._Ready();
	}

	public override void _Input(InputEvent @event)
	{

		// For actions defined in the InputMap:
		if (@event.IsActionPressed("move"))
		{
			GD.Print("Move action triggered!");
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		if (direction.Length() > 0)
			this.Move(direction);

		_saveData.position = this.Position;
	}

	#region SAVE/LOAD
	[Serializable]
	public class PlayerSaveData : ISaveable
	{
		[Export] public string Id { get; set; }
		[Export] public bool IsNew { get; set; }
		public Vector2 position;
		public string characterId;
	}

	// [Export]
	PlayerSaveData _saveData = null;

	public UniqueId UniqueId { get; set; } = new() { Id = Guid.NewGuid().ToString() };

	public void Bind(PlayerSaveData data)
	{
		_saveData = data;
		_saveData.Id = UniqueId.Id;
		this.Position = _saveData.position;
		this._currentGoal = _saveData.position;
		this._nextGoal = _saveData.position;
	}
	#endregion
}
