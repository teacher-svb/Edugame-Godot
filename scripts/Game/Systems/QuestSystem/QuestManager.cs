using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using TnT.EduGame.GameStates;

namespace TnT.EduGame.QuestSystem
{
    public partial class QuestManager : Node
    {
        public static QuestManager Instance { get; private set; }
        [Export]
        Array<Quest> _quests = new();
        public List<Quest> Quests => _quests.ToList();
        IQuestProcessor _questChain;

        [Signal]
        public delegate void OnQuestUpdatedEventHandler(QuestObjective objective);
        [Export] QuestEventChannel eventChannel;

        public override void _EnterTree() => Instance = this;

        public override void _Ready()
        {
            base._Ready();
            _questChain = new QuestProcessor<QuestMessageStart>(QuestState.NOTSTARTED, QuestState.INPROGRESS);
            _questChain
                .SetNext(new QuestProcessor<QuestMessageComplete>(QuestState.INPROGRESS, QuestState.COMPLETED))
                .SetNext(new QuestProcessor<QuestMessageFail>(QuestState.INPROGRESS, QuestState.NOTSTARTED));

            this.OnQuestUpdated += (v) => eventChannel.Invoke(v);
            foreach (var quest in _quests)
            {
                quest.Reset();
                quest.OnObjectiveStateChanged += QuestObjectStateChanged;
                quest.ObjectiveTransitionRequested += QuestObjectTransitionRequested;
            }

            SyncSaveData();
        }

        // the quests are shared Resources and outlive this node
        public override void _ExitTree()
        {
            foreach (var quest in _quests)
            {
                if (quest == null)
                    continue;
                quest.OnObjectiveStateChanged -= QuestObjectStateChanged;
                quest.ObjectiveTransitionRequested -= QuestObjectTransitionRequested;
            }
            if (Instance == this)
                Instance = null;
        }

        private void QuestObjectStateChanged(QuestObjective o)
        {
            // the state changes here, when the player dismissed the quest message, i.e. after UpdateQuest synced
            // the save data: without this the last objective of a quest never reached the save
            SyncSaveData();
            EmitSignal(SignalName.OnQuestUpdated, o);
        }

        void SyncSaveData()
        {
            if (_saveData != null)
                _saveData.quests = _quests.Select(q => q.GetSaveData()).ToArray();
        }

        private void QuestObjectTransitionRequested(QuestObjective o, QuestState requestedState, Action commit)
        {
            // nothing to say (a fail without a fail text): commit at once instead of showing an empty box
            var text = o.GetText(requestedState);
            if (string.IsNullOrWhiteSpace(text))
            {
                commit();
                return;
            }
            StateManagerGame.Instance.ShowQuestMessage(o, requestedState, commit);
        }

        public void UpdateQuest(QuestMessage msg)
        {
            if (_questChain == null)
            {
                return;
            }
            _questChain.Process(msg, Quests);

            SyncSaveData();
        }

        // Starts the quest's first objective. Does nothing once any objective has been touched or while its
        // start is pending (message not dismissed yet), so it is safe to call repeatedly (e.g. every time the
        // player talks to the quest giver).
        public void StartQuest(string questId)
        {
            var quest = _quests.FirstOrDefault(q => q.Id == questId);
            if (quest == null)
            {
                GD.PrintErr($"QuestManager: No quest with id '{questId}'.");
                return;
            }

            if (quest.HasPendingTransition || quest.Objectives.Any(o => o.State != QuestState.NOTSTARTED))
                return;

            UpdateQuest(new QuestMessageStart { QuestId = questId, ObjectiveId = quest.CurrentObjectiveId });
        }

        public void StartObjective(string questId, string objectiveId) => UpdateQuest(new QuestMessageStart { QuestId = questId, ObjectiveId = objectiveId });
        public void CompleteObjective(string questId, string objectiveId) => UpdateQuest(new QuestMessageComplete { QuestId = questId, ObjectiveId = objectiveId });
        public void FailObjective(string questId, string objectiveId) => UpdateQuest(new QuestMessageFail { QuestId = questId, ObjectiveId = objectiveId });

    }
}
