using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame.QuestSystem
{
    // Patrols along the waypoints. With _waitUntilDone it completes when the last waypoint is
    // reached; turn it off for a looping patrol (CharacterStatePatrolling._loop), which never ends.
    [GlobalClass]
    public partial class PatrolStep : QuestStep
    {
        [Export] Character3D _character;
        [Export] Node3D[] _waypoints = [];
        [Export] bool _waitUntilDone = true;

        public override Task Run(QuestObjective objective)
        {
            if (_waitUntilDone)
                return _character.PatrolAsync(_waypoints);

            _character.SetStateToPatrolling(_waypoints);
            return Task.CompletedTask;
        }
    }
}
