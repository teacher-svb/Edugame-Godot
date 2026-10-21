using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame.QuestSystem
{
    // Ends the character's current state (e.g. stops following or patrolling)
    [GlobalClass]
    public partial class PopStateStep : QuestStep
    {
        [Export] Character3D _character;

        public override Task Run(QuestObjective objective) => _character.PopCurrentStateAsync();
    }
}
