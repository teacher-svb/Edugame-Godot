using Godot;
using TnT.EduGame.CharacterState;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class FollowReaction : CharacterReaction
    {
        [Export] public Node3D Target { get; set; }

        protected override void Act(CharacterStateManager sm) => sm.Follow(Target);
    }
}
