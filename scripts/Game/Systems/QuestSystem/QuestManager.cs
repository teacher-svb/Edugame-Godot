using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using TnT.EduGame.Characters;
using TnT.Systems;
using TnT.Systems.Persistence;

namespace TnT.EduGame.QuestSystem
{
    public partial class QuestManager : Node, IBind<QuestManagerSaveData>
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
            }

            _saveData.quests = _quests.Select(q => q.GetSaveData()).ToArray();
        }

        private void QuestObjectStateChanged(QuestObjective o)
        {
            GD.Print("emitting signal: onquestupdated");
            EmitSignal(SignalName.OnQuestUpdated, o);
        }

        public void UpdateQuest(QuestMessage msg)
        {
            if (_questChain == null)
            {
                GD.Print("QuestChain not initialized");
                return;
            }
            _questChain.Process(msg, Quests);

            _saveData.quests = _quests.Select(q => q.GetSaveData()).ToArray();
        }


        #region SAVE/LOAD
        public UniqueId UniqueId { get; set; } = new() { Id = Guid.NewGuid().ToString() };
        // [Export] public UniqueId UniqueId { get; set; } = new() { Id = Guid.NewGuid().ToString() };
        // [field: SerializeField, ReadOnly] public string Id { get; set; } = Guid.NewGuid().ToString();
        [Export]
        QuestManagerSaveData _saveData;

        public void Bind(QuestManagerSaveData data)
        {
            _saveData = data;
            _saveData.Id = UniqueId.Id;
            Quests.ForEach(q => q.Bind(_saveData.quests?.FirstOrDefault(s => s.Id == q.Id)));
        }
        #endregion
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
            var quest = quests.FirstOrDefault(q => q.Id == msg.QuestId);
            if (msg is T msgStart && quest != null)
            {
                if (quest.State == _currentState && quest.CurrentObjectiveId == msgStart.ObjectiveId)
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