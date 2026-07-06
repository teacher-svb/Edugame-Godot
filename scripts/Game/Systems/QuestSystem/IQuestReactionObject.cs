
using System;

namespace TnT.EduGame.QuestSystem
{
    public interface IQuestReactionObject
    {
        event Action ReactionCompleted;
    }
}