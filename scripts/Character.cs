using Godot;
using System;
using TnT.Extensions;

namespace TnT.Systems
{
	public partial class Character : CharacterBody2D
	{
		[Export]
		public float Speed = 8.0f;
		[Export]
		int tileSize = 16;
		private Vector2 _currentGoal;
		private Vector2 _nextGoal;
		[Export]
		RayCast2D rayCast;

		public override void _Ready()
		{
			_currentGoal = Position.Snap();
			_nextGoal = _currentGoal;
			rayCast.TargetPosition = new Vector2(tileSize, 0);
		}

		public override void _Draw()
		{
			// DrawLine(new Vector2(0, 0), _currentGoal - Position, new Color(255, 0, 0), 4);
			// DrawLine(new Vector2(0, 0), _nextGoal - Position, new Color(0, 255, 0), 2);
		}

		public override void _Process(double delta)
		{
			QueueRedraw();
		}

		public override void _PhysicsProcess(double delta)
		{
			var newPos = Position.MoveToward(
				_currentGoal,
				(float)(Speed * tileSize * delta)
			);

			if (newPos.DistanceTo(_currentGoal) < 4)
			{
				_currentGoal = (Position + ((_nextGoal - Position) / 2f)).Snap(tileSize);
				newPos = Position.MoveToward(
					_currentGoal,
					(float)(Speed * tileSize * delta)
				);
			}


			Velocity = Position.DirectionTo(newPos) * Speed * tileSize * (float)delta;

			if (Position.DistanceTo(newPos) > 2)
				MoveAndSlide();
			else
				Position = newPos;

			_nextGoal = _currentGoal;
		}

		public void Move(Vector2 movement)
		{
			var nextGoal = (Position + (movement.Normalized() * tileSize * 2.5f).Snap(tileSize)).Snap(tileSize);
			rayCast.LookAt(nextGoal);
			rayCast.ForceRaycastUpdate();

			if (rayCast.IsColliding() == false)
				_nextGoal = nextGoal;
		}
	}
}