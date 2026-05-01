using System.Linq;
using Godot;
using TnT.Systems.EventSystem;

namespace TnT.EduGame.QuestSystem
{
    [GlobalClass]
    public partial class QuestEventListener : EventListener
    {
        [Export] bool _checkObjectiveId;
        [Export] string _objectiveId;
        [Export] bool _checkState;
        [Export] QuestState _state;

        private QuestReaction[] _reactions = [];

        public override void _Ready()
        {
            base._Ready();
            _reactions = GetChildren().OfType<QuestReaction>().ToArray();
        }

        public override void Raise(params Variant[] values)
        {
            foreach (var value in values)
            {
                if (value.Obj is QuestObjective o)
                {
                    if (_checkObjectiveId && o.ObjectiveId != _objectiveId) return;
                    if (_checkState && o.State != _state) return;
                    base.Raise(o);
                    ExecuteReactions();
                }
            }
        }

        private async void ExecuteReactions()
        {
            foreach (var reaction in _reactions)
                await reaction.Execute();
        }
    }
}
