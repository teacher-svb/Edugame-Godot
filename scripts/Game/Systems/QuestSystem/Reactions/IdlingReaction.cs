using Godot;
using TnT.EduGame.CharacterState;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class IdlingReaction : CharacterReaction
    {
        [Export] int DurationMs { get; set; }
        protected override void Act(CharacterStateManager sm) => sm.Idling(DurationMs);
    }
}
