using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace TnT.EduGame.QuestSystem
{
    public enum QuestState
    {
        NOTSTARTED,
        INPROGRESS,
        COMPLETED,
        FAILED
    }

    [GlobalClass]
    public partial class Quest : Resource
    {
        [Export]
        public string _id { get; private set; } = Guid.NewGuid().ToString();
        public string Id => _id;

        [Export]
        string _name;
        public string Name => _name;
        [Export]
        Array<QuestObjective> _objectives = new();
        public List<QuestObjective> Objectives => _objectives.ToList();
        public bool IsCompleted => _objectives.All(o => o.State == QuestState.COMPLETED);

        public delegate void OnQuestObjectiveStateChanged(QuestObjective objective);
        public OnQuestObjectiveStateChanged OnObjectiveStateChanged;

        public delegate void OnQuestObjectiveTransitionRequested(QuestObjective objective, QuestState requestedState, Action commit);
        public OnQuestObjectiveTransitionRequested ObjectiveTransitionRequested;

        // bumped by Reset (a load or a new game): a commit requested before it belongs to the old session
        int _generation;

        QuestObjective CurrentObjective => _objectives?.FirstOrDefault(s => s.State != QuestState.COMPLETED);

        /// <summary>The current objective's state; COMPLETED once every objective is.</summary>
        public QuestState State => IsCompleted ? QuestState.COMPLETED : CurrentObjective?.State ?? QuestState.NOTSTARTED;

        /// <summary>True while the current objective waits for its requested transition to be committed.</summary>
        public bool HasPendingTransition => CurrentObjective?.HasPendingTransition ?? false;

        public string CurrentObjectiveId => CurrentObjective?.ObjectiveId ?? Guid.Empty.ToString();

        /// <summary>
        /// Requests the current objective's move to <paramref name="value"/>. The actual change waits until whoever
        /// shows the associated quest message calls back (the player dismissed it), so other systems never observe
        /// the new state before the player has. Until then the objective is pending: another request is ignored.
        /// </summary>
        internal void RequestTransition(QuestState value)
        {
            var currentObjective = CurrentObjective;
            if (currentObjective == null)
                return;
            if (currentObjective.HasPendingTransition)
            {
                GD.PushWarning($"Quest '{Name}': transition of '{currentObjective.ObjectiveId}' to {value} ignored, one to {currentObjective.PendingState} is pending.");
                return;
            }

            currentObjective.PendingState = value;
            int generation = _generation;
            if (ObjectiveTransitionRequested == null)
                CommitTransition(currentObjective, value, generation);
            else
                ObjectiveTransitionRequested.Invoke(currentObjective, value, () => CommitTransition(currentObjective, value, generation));
        }

        void CommitTransition(QuestObjective currentObjective, QuestState value, int generation)
        {
            if (generation != _generation)
            {
                // e.g. a message of the old level unwound while a save is loaded: the loaded states win
                GD.PushWarning($"Quest '{Name}': transition of '{currentObjective.ObjectiveId}' to {value} dropped, the quest was reset or loaded since it was requested.");
                return;
            }
            if (currentObjective.PendingState != value)
                return;   // committed already (a callback that runs twice)

            currentObjective.PendingState = null;
            currentObjective.State = value;
            OnObjectiveStateChanged?.Invoke(currentObjective);

            if (value == QuestState.COMPLETED)
            {
                var nextObjective = CurrentObjective;
                if (nextObjective != null)
                    QuestManager.Instance.UpdateQuest(new QuestMessageStart { QuestId = Id, ObjectiveId = nextObjective.ObjectiveId });

            }
            else if (value == QuestState.NOTSTARTED)
            {
                // A failed challenge resets its own objective to NOTSTARTED (QuestProcessor<QuestMessageFail>).
                // Re-arm the same objective immediately so whatever reacts to it going INPROGRESS
                // (e.g. a QuestEventListener re-triggering a MathChallengeTrigger) fires again.
                QuestManager.Instance.UpdateQuest(new QuestMessageStart { QuestId = Id, ObjectiveId = currentObjective.ObjectiveId });
            }
        }

        /// <summary>Every objective not started, nothing pending; commits requested before this are dropped.</summary>
        public void Reset()
        {
            _generation++;
            foreach (var objective in _objectives)
            {
                objective.State = QuestState.NOTSTARTED;
                objective.PendingState = null;
            }
        }
    }
}
