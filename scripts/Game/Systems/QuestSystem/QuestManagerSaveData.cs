using TnT.Systems.Persistence;

namespace TnT.EduGame.QuestSystem
{
    // plain C# (not a Godot Resource): Newtonsoft writes only these fields
    public class QuestManagerSaveData : ISaveable
    {
        public string Id { get; set; }
        public bool IsNew { get; set; }

        public QuestSaveData[] quests = [];
    }
}
