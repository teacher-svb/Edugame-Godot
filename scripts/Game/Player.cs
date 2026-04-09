using Godot;
using TnT.Extensions;
using TnT.Systems;

public partial class Player : Node//, IBind<Player.PlayerSaveData>
{
	public static Player Instance { get; private set; }

	CharacterController2D _cc;

	public static Player Create()
	{
		var playerScene = (PackedScene)GD.Load("res://Scenes/player.tscn");
		var player = (Player)playerScene.Instantiate();
		return player;
	}

	public override void _Ready()
	{
		Instance = this;
		_cc = this.FindAncestorOfType<CharacterController2D>();
		base._Ready();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		if (direction.Length() > 0)
			_cc.Move(direction);
	}

	public void MoveTo(Vector3 target)
    {
		_cc.MoveTo(target);
    }
}
