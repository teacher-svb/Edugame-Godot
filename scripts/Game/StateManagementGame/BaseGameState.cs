
using Godot;
using TnT.Extensions;

namespace TnT.EduGame.GameStates
{
    public abstract partial class BaseGameState : Node
    {
        public override void _Ready()
        {
            var parent = this.FindAncestorOfType<StateManagerGame>();
            parent.RegisterState(this);
        }
    }
}