
using Godot;
using TnT.Extensions;

namespace TnT.EduGame.GameState
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