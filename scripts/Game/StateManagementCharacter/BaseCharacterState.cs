using Godot;
using TnT.Extensions;

namespace TnT.EduGame.CharacterState
{
    public abstract partial class BaseCharacterState : Node
    {
        public override void _Ready()
        {
            this.FindAncestorOfType<CharacterStateManager>().RegisterState(this);
        }
    }
}
