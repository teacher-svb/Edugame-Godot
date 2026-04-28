using Godot;
using TnT.EduGame.CharacterState;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class StartPatrolReaction : CharacterReaction
    {
        [Export] public Node3D[] PatrolTargets { get; set; }

        protected override void Act(CharacterStateManager sm) => sm.StartPatrol(PatrolTargets);
    }
}
