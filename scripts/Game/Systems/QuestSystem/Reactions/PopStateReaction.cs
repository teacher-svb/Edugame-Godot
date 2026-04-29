using Godot;
using TnT.EduGame.CharacterState;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class PopStateReaction : CharacterReaction
    {
        protected override void Act(CharacterStateManager sm) => _ = sm.Pop();
    }
}
