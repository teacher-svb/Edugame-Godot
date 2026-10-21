using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame.QuestSystem
{
    // Idles the character and waits until the idle is over
    [GlobalClass]
    public partial class IdleStep : QuestStep
    {
        [Export] Character3D _character;
        [Export] int _durationMs = 1000;

        public override Task Run(QuestObjective objective) => _character.IdleAsync(_durationMs);
    }
}
