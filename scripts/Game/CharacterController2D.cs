using Godot;
using System;
using System.Collections.Generic;
using TnT.EduGame.Characters;
using TnT.Extensions;

namespace TnT.Systems
{
	public interface ICharacterController
	{
		void Move(Vector2 movement);
		void MoveTo(Vector3 position);
	}
	public partial class CharacterController2D : CharacterBody2D, ICharacterController
	{
		[Export]
		public float Speed = 8.0f;
		[Export]
		int tileSize = 16;
		protected Vector2 _currentGoal;
		protected Vector2 _nextGoal;
		[Export]
		RayCast2D rayCast;

		public override void _Ready()
		{
			_currentGoal = Position.Snap();
			_nextGoal = _currentGoal;
			rayCast.TargetPosition = new Vector2(tileSize, 0);
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

		public void MoveTo(Vector3 position)
		{
			_currentGoal = position.ToVector2().Snap();
			_nextGoal = position.ToVector2().Snap();
			this.Position = _nextGoal;
		}
	}
}