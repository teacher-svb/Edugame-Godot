using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using TnT.EduGame.Characters;
using TnT.Systems.EventSystem;
using TnT.Systems.Persistence;


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

        public delegate void OnQuestObjectiveStateChanged(QuestObjective objective);
        public OnQuestObjectiveStateChanged OnObjectiveStateChanged;

        public delegate void OnQuestObjectiveTransitionRequested(QuestObjective objective, QuestState requestedState, Action commit);
        public OnQuestObjectiveTransitionRequested ObjectiveTransitionRequested;

        [Export]
        public QuestState State
        {
            get => _objectives?.FirstOrDefault(s => s.State != QuestState.COMPLETED)?.State ?? QuestState.NOTSTARTED;
            set
            {
                var currentObjective = _objectives?.FirstOrDefault(s => s.State != QuestState.COMPLETED);
                if (currentObjective == null)
                    return;

                // Defer the actual mutation until whoever is showing the associated
                // quest message calls back (i.e. the player dismissed it), so other
                // systems never observe the new state before the player has.
                if (ObjectiveTransitionRequested == null)
                {
                    CommitTransition(currentObjective, value);
                    return;
                }

                ObjectiveTransitionRequested.Invoke(currentObjective, value, () => CommitTransition(currentObjective, value));
            }
        }

        void CommitTransition(QuestObjective currentObjective, QuestState value)
        {
            currentObjective.State = value;
            OnObjectiveStateChanged?.Invoke(currentObjective);

            if (value == QuestState.COMPLETED)
            {
                var nextObjective = _objectives?.FirstOrDefault(s => s.State != QuestState.COMPLETED);
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

        public string CurrentObjectiveId
        {
            get => _objectives?.FirstOrDefault(s => s.State != QuestState.COMPLETED)?.ObjectiveId ?? Guid.Empty.ToString();
        }

        public void Initialize()
        {
            var firstObjective = _objectives.FirstOrDefault();
            if (firstObjective == null || firstObjective.State == QuestState.COMPLETED)
                return;
            _objectives.FirstOrDefault().State = QuestState.INPROGRESS;
        }

        public void Reset() => Objectives.ForEach(o => o.State = QuestState.NOTSTARTED);

        // #region LOAD/SAVE
        // private QuestSaveData _saveData;
        // public QuestSaveData SaveData => _saveData;

        // public void Bind(QuestSaveData data)
        // {
        //     if (data == null)
        //     {
        //         Reset();
        //         return;
        //     }
        //     _saveData = data;
        //     _id = _saveData.Id;
        //     Objectives.ForEach(o => o.Bind(data.Objectives.FirstOrDefault(s => s.Id == o.ObjectiveId)));
        // }

        // public QuestSaveData GetSaveData()
        // {
        //     if (_saveData == null)
        //         _saveData = new();
        //     _saveData.Id = _id;
        //     _saveData.Objectives = _objectives.Select(o => o.GetSaveData()).ToArray();
        //     return _saveData;
        // }
        // #endregion

    }
}