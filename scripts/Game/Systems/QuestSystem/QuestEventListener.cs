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
        [Export] internal Godot.Collections.Array<QuestReaction> _reactions = [];

        [Signal] public delegate void ReactionStartEventHandler();
        [Signal] public delegate void ReactionEndEventHandler();

        public override void Raise(params Variant[] values)
        {
            foreach (var value in values)
            {
                if (value.Obj is QuestObjective o)
                {
                    GD.Print(o.ObjectiveId);
                    if (_checkObjectiveId && o.ObjectiveId != _objectiveId) return;
                    GD.Print($"objective Id ok, state is {o.State}");
                    if (_checkState && o.State != _state) return;
                    GD.Print("objective state ok");
                    base.Raise(o);
                    ExecuteReactions();
                }
            }
        }

        private async void ExecuteReactions()
        {
            EmitSignal(SignalName.ReactionStart);
            foreach (var reaction in _reactions)
            {
                await reaction.Execute(this);
            }
            EmitSignal(SignalName.ReactionEnd);
        }
    }
}
