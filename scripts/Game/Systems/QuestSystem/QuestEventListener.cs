using Godot;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Listens for quest objective updates, filters them by objective id and/or state, and
    /// on a match raises the base event and runs its child <see cref="QuestStep"/>s.
    /// </summary>
    [GlobalClass]
    public partial class QuestEventListener : QuestStepListener
    {
        [Export] bool _checkObjectiveId;
        [Export] string _objectiveId;
        [Export] bool _checkState;
        [Export] QuestState _state;

        // Any objective that fails an enabled filter stops processing entirely (return, not continue)
        public override void Raise(params Variant[] values)
        {
            foreach (var value in values)
            {
                if (value.Obj is QuestObjective o)
                {
                    if (_checkObjectiveId && o.ObjectiveId != _objectiveId) return;
                    if (_checkState && o.State != _state) return;
                    base.Raise(o);
                    RunSteps(o);
                }
            }
        }
    }
}
