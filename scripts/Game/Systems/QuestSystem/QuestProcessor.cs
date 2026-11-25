using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    // Chain of Responsibility: each processor handles one message type, moving the current objective from one
    // state to another, and passes any other message on (QuestManager builds the chain).
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

                // Messages only apply to the current objective in the expected state, with no transition pending
                // (its message not dismissed yet: a second request would commit twice); anything else is ignored,
                // so warn instead of failing silently (usually a wiring mistake or a trigger hit twice).
                if (quest == null)
                    GD.PushWarning($"QuestManager: {typeof(T).Name} ignored, no quest with id '{msg.QuestId}'.");
                else if (quest.CurrentObjectiveId != msg.ObjectiveId)
                    GD.PushWarning($"QuestManager: {typeof(T).Name} for objective '{msg.ObjectiveId}' ignored in '{quest.Name}', current objective is '{quest.CurrentObjectiveId}'.");
                else if (quest.HasPendingTransition)
                    GD.PushWarning($"QuestManager: {typeof(T).Name} for objective '{msg.ObjectiveId}' ignored in '{quest.Name}', a transition is pending (its message isn't dismissed yet).");
                else if (quest.State != _currentState)
                    GD.PushWarning($"QuestManager: {typeof(T).Name} for objective '{msg.ObjectiveId}' ignored in '{quest.Name}', it is {quest.State} but must be {_currentState}.");
                else
                    quest.RequestTransition(_resultState);

                return;
            }

            base.Process(msg, quests);
        }
    }
}
