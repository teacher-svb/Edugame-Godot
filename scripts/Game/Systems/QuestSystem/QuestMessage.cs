namespace TnT.EduGame.QuestSystem
{
    // A request to the QuestManager to move one objective (see QuestProcessor<T>)
    public abstract class QuestMessage
    {
        public string QuestId { get; set; }
        public string ObjectiveId { get; set; }
    }

    public class QuestMessageStart : QuestMessage { }
    public class QuestMessageFail : QuestMessage { }
    public class QuestMessageComplete : QuestMessage { }
}
