using Godot;
using TnT.EduGame.CharacterState;
using TnT.Extensions;

namespace TnT.EduGame.QuestSystem
{
    public abstract partial class CharacterReaction : QuestReaction
    {
        [Export] public Node3D Character { get; set; }

        protected CharacterStateManager StateManager
            => Character.FindAnyObjectByType<CharacterStateManager>();

        public sealed override void Execute()
        {
            var sm = StateManager;
            sm.Pop();
            Act(sm);
        }

        protected abstract void Act(CharacterStateManager sm);
    }
}
