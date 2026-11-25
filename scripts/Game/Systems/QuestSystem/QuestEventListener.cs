using System.Linq;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Listens for quest objective updates, filters them by objective id and/or state, and
    /// on a match raises the base event and runs its child <see cref="QuestStep"/>s.
    /// </summary>
    /// <remarks>
    /// Loading a save (or entering the level) raises no quest events. Two opt-in flags make up for that; both run
    /// once the scene is ready and bound to the save data (<see cref="QuestStepListener.CallWhenSceneReady"/>) and
    /// need an objective id and a state:
    /// <list type="bullet">
    /// <item><see cref="_catchUpAfterLoad"/>: a reaction that changed the world (opened something a later objective
    /// needs) is lost when the game was saved after it. If the objective is already past the state listened for
    /// (COMPLETED), the steps run once more without an objective (messages, cards and saves skip themselves) and
    /// without ReactionStart/ReactionEnd (those usually complete the objective, or toggle something that saves
    /// itself). A step that walks, toggles or opens a challenge must not run again.</item>
    /// <item><see cref="_resumeAfterLoad"/>: for an INPROGRESS reaction that completes its own objective
    /// (ReactionEnd → CompleteObjective). If the objective is still INPROGRESS after the load (saved before this
    /// change blocked saving mid-reaction, or the level was left while it ran), the reaction runs again from its
    /// first step, with the objective and with signals, so ReactionEnd completes it.</item>
    /// </list>
    /// Live reactions hold up saving while they run (<see cref="SaveLoadManager.SaveGame()"/> refuses).
    /// </remarks>
    [GlobalClass]
    public partial class QuestEventListener : QuestStepListener
    {
        [Export] bool _checkObjectiveId;
        [Export] string _objectiveId;
        [Export] bool _checkState;
        [Export] QuestState _state;
        [Export] bool _catchUpAfterLoad;
        [Export] bool _resumeAfterLoad;
        public bool ResumesAfterLoad => _resumeAfterLoad;

        public override void _Ready()
        {
            base._Ready();
            if (!_catchUpAfterLoad && !_resumeAfterLoad)
                return;
            if (!_checkObjectiveId || !_checkState)
            {
                GD.PushWarning($"QuestEventListener '{Name}': catch-up / resume after load needs an objective id and a state");
                return;
            }
            if (_resumeAfterLoad && _state != QuestState.INPROGRESS)
                GD.PushWarning($"QuestEventListener '{Name}': resume after load only works for state INPROGRESS");
            CallWhenSceneReady();
        }

        protected override void OnSceneReady()
        {
            var objective = QuestManager.Instance?.Quests.SelectMany(q => q.Objectives).FirstOrDefault(o => o.ObjectiveId == _objectiveId);
            if (objective == null)
                return;

            if (_catchUpAfterLoad && IsPast(objective.State))
                RunSteps(null, signals: false);
            else if (_resumeAfterLoad && _state == QuestState.INPROGRESS
                     && objective.State == QuestState.INPROGRESS && !objective.HasPendingTransition)
                RunSteps(objective, signals: true, blocksSaving: true);
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
                    RunSteps(o, blocksSaving: true);
                }
            }
        }
    }
}
