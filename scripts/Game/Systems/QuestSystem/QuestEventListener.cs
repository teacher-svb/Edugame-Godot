using System.Linq;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Listens for quest objective updates, filters them by objective id and/or state, and
    /// on a match raises the base event and runs its child <see cref="QuestStep"/>s.
    /// </summary>
    /// <remarks>
    /// Loading a save raises no quest events, so a reaction that changed the world (opened something a later
    /// objective needs) is lost when the game was saved after it but before the quest was completed. Set
    /// <see cref="_catchUpAfterLoad"/> on such a listener: on _Ready and when the <see cref="QuestManager"/> is
    /// bound, if its objective is already past the state it listens for (COMPLETED), it runs its steps once more
    /// without an objective (messages, cards and saves skip themselves) and without ReactionStart/ReactionEnd (those
    /// usually complete the objective, or toggle something that saves itself). Opt-in: a step that walks, toggles or
    /// opens a challenge must not run again.
    /// </remarks>
    [GlobalClass]
    public partial class QuestEventListener : QuestStepListener
    {
        [Export] bool _checkObjectiveId;
        [Export] string _objectiveId;
        [Export] bool _checkState;
        [Export] QuestState _state;
        [Export] bool _catchUpAfterLoad;

        bool _caughtUp;

        public override void _Ready()
        {
            base._Ready();
            if (!_catchUpAfterLoad)
                return;
            if (!_checkObjectiveId || !_checkState)
            {
                GD.PushWarning($"QuestEventListener '{Name}': catch-up after load needs an objective id and a state");
                return;
            }
            QuestManager.Instance.Bound += CatchUp;
            CatchUp();
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            if (_catchUpAfterLoad && QuestManager.Instance != null)
                QuestManager.Instance.Bound -= CatchUp;
        }

        void CatchUp()
        {
            if (_caughtUp)
                return;
            var objective = QuestManager.Instance.Quests.SelectMany(q => q.Objectives).FirstOrDefault(o => o.ObjectiveId == _objectiveId);
            if (objective == null || !IsPast(objective.State))
                return;
            _caughtUp = true;
            RunSteps(null, signals: false);
        }

        // the reaction to `_state` has run once the objective got past it: reached COMPLETED
        bool IsPast(QuestState current) => _state switch
        {
            QuestState.INPROGRESS or QuestState.COMPLETED => current == QuestState.COMPLETED,
            _ => false,
        };

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
