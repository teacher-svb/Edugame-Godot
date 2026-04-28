using System.Linq;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class QuestObjectiveBinding : Node
    {
        [Export] public string ObjectiveId { get; set; }
        [Export] public QuestState State { get; set; }

        private QuestReaction[] _reactions = [];

        public override void _Ready()
        {
            _reactions = GetChildren().OfType<QuestReaction>().ToArray();
        }

        public bool Matches(QuestObjective objective)
            => objective.ObjectiveId == ObjectiveId && objective.State == State;

        public void Execute()
        {
            foreach (var reaction in _reactions)
                reaction.Execute();
        }
    }
}
