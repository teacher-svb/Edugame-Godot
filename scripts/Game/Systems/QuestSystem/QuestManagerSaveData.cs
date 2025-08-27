
using Godot;
using TnT.Systems.Persistence;

namespace TnT.EduGame.QuestSystem
{
        [GlobalClass]
        public partial class QuestManagerSaveData : Resource, ISaveable
        {
            [Export] public string Id { get; set; }
            [Export] public bool IsNew { get; set; }

            public QuestSaveData[] quests;
}
        }