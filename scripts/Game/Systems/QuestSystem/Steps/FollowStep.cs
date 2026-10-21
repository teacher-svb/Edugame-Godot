using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame.QuestSystem
{
    // Makes the follower follow the target. Doesn't wait: following never ends on its own.
    [GlobalClass]
    public partial class FollowStep : QuestStep
    {
        [Export] Character3D _follower;
        [Export] Node3D _target;

        public override Task Run(QuestObjective objective)
        {
            _follower.SetStateToFollow(_target);
            return Task.CompletedTask;
        }
    }
}
