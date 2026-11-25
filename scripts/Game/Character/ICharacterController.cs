using Godot;

namespace TnT.EduGame.Characters
{
	/// <summary>
	/// A controller that moves a character: by input direction, or to a target position.
	/// </summary>
	public interface ICharacterController
	{
		void Move(Vector2 movement);
		void MoveTo(Vector3 position);
	}
}
