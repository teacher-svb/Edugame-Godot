using Godot;

namespace TnT.EduGame.Characters
{
    public interface IMovementAnimator
    {
        void OnMovementStateChanged(string state);
    }
}
