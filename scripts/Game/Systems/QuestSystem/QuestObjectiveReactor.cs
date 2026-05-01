using System.Linq;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class QuestObjectiveReactor : QuestEventListener
    {
        private QuestReaction[] _reactions = [];

        public override void _Ready()
        {
            _reactions = GetChildren().OfType<QuestReaction>().ToArray();
            base._Ready();
            OnListen += _ => ExecuteReactions();
        }

        private async void ExecuteReactions()
        {
            foreach (var reaction in _reactions)
                await reaction.Execute();
        }
    }
}
