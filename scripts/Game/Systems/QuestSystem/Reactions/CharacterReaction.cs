using Godot;
using TnT.EduGame.CharacterState;
using TnT.Extensions;

namespace TnT.EduGame.QuestSystem
{
    public abstract partial class CharacterReaction : QuestReaction
    {
        [Export] Node3D Character { get; set; }

        protected CharacterStateManager StateManager
            => Character.FindAnyObjectByType<CharacterStateManager>();

        public sealed override void Execute() => Act(StateManager);

        protected abstract void Act(CharacterStateManager sm);
    }
}
