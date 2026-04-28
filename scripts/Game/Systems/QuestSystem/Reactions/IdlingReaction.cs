using Godot;
using TnT.EduGame.CharacterState;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class IdlingReaction : CharacterReaction
    {
        protected override void Act(CharacterStateManager sm) => sm.Idling();
    }
}
