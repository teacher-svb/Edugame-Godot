using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using TnT.EduGame.GameState;

namespace TnT.EduGame.QuestSystem
{
    public partial class QuestManager : Node
    {
        public static QuestManager Instance { get; private set; }
        [Export]
        Array<Quest> _quests = new();
        public List<Quest> Quests => _quests.ToList();
        IQuestProcessor _questChain;

        // UnityEvent<QuestObjective> OnQuestUpdated;
        [Signal]
        public delegate void OnQuestUpdatedEventHandler(QuestObjective objective);
        [Export] QuestEventChannel eventChannel;


        public override void _Ready()
        {
            Instance = this;
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

            if (_saveData != null)
                _saveData.quests = _quests.Select(q => q.GetSaveData()).ToArray();
        }

        private void QuestObjectStateChanged(QuestObjective o)
        {
            EmitSignal(SignalName.OnQuestUpdated, o);
        }

        private void QuestObjectTransitionRequested(QuestObjective o, QuestState requestedState, Action commit)
        {
            StateManagerGame.Instance.ShowQuestMessage(o, requestedState, commit);
        }

        public void UpdateQuest(QuestMessage msg)
        {
            if (_questChain == null)
            {
                return;
            }
            _questChain.Process(msg, Quests);

            _saveData.quests = _quests.Select(q => q.GetSaveData()).ToArray();
        }

        // Starts the quest's first objective. Does nothing once any objective has been touched,
        // so it is safe to call repeatedly (e.g. every time the player talks to the quest giver).
        public void StartQuest(string questId)
        {
            var quest = _quests.FirstOrDefault(q => q.Id == questId);
            if (quest == null)
            {
                GD.PrintErr($"QuestManager: No quest with id '{questId}'.");
                return;
            }

            if (quest.Objectives.Any(o => o.State != QuestState.NOTSTARTED))
                return;

            UpdateQuest(new QuestMessageStart { QuestId = questId, ObjectiveId = quest.CurrentObjectiveId });
        }

        public void StartObjective(string questId, string objectiveId) => UpdateQuest(new QuestMessageStart { QuestId = questId, ObjectiveId = objectiveId });
        public void CompleteObjective(string questId, string objectiveId) => UpdateQuest(new QuestMessageComplete { QuestId = questId, ObjectiveId = objectiveId });
        public void FailObjective(string questId, string objectiveId) => UpdateQuest(new QuestMessageFail { QuestId = questId, ObjectiveId = objectiveId });

    }

    public interface IQuestProcessor
    {
        IQuestProcessor SetNext(IQuestProcessor processor);
        void Process(QuestMessage msg, List<Quest> quests);
    }

    public abstract class QuestProcessor : IQuestProcessor
    {
        IQuestProcessor _next;
        public virtual void Process(QuestMessage msg, List<Quest> quests) => _next?.Process(msg, quests);
        public IQuestProcessor SetNext(IQuestProcessor processor) => _next = processor;
    }

    public class QuestProcessor<T> : QuestProcessor where T : QuestMessage
    {
        QuestState _currentState;
        QuestState _resultState;
        public QuestProcessor(QuestState currentState, QuestState resultState)
        {
            _currentState = currentState;
            _resultState = resultState;
        }
        public override void Process(QuestMessage msg, List<Quest> quests)
        {
            if (msg is T)
            {
                var quest = quests.FirstOrDefault(q => q.Id == msg.QuestId);

                // Messages only apply to the current objective in the expected state; anything
                // else is ignored, so warn instead of failing silently (usually a wiring mistake).
                if (quest == null)
                    GD.PushWarning($"QuestManager: {typeof(T).Name} ignored, no quest with id '{msg.QuestId}'.");
                else if (quest.CurrentObjectiveId != msg.ObjectiveId)
                    GD.PushWarning($"QuestManager: {typeof(T).Name} for objective '{msg.ObjectiveId}' ignored in '{quest.Name}', current objective is '{quest.CurrentObjectiveId}'.");
                else if (quest.State != _currentState)
                    GD.PushWarning($"QuestManager: {typeof(T).Name} for objective '{msg.ObjectiveId}' ignored in '{quest.Name}', it is {quest.State} but must be {_currentState}.");
                else
                    quest.State = _resultState;

                return;
            }

            base.Process(msg, quests);
        }
    }

    public abstract class QuestMessage
    {
        public string QuestId { get; set; }
        public string ObjectiveId { get; set; }
    }

    public class QuestMessageStart : QuestMessage { }
    public class QuestMessageFail : QuestMessage { }
    public class QuestMessageComplete : QuestMessage { }
}