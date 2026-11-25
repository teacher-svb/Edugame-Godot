using System.Collections.Generic;
using TnT.Systems.Persistence;
using TnT.EduGame.Characters;
using TnT.EduGame.QuestSystem;
using TnT.EduGame.Challenges;
using TnT.EduGame.Inventory;

namespace TnT.EduGame
{
    // plain C# (not a Godot Resource): the save file holds only these fields
    public class MyGameData : GameData
    {
        public string CurrentLevelName;
        public List<CharacterSaveData> characterData = new();
        public QuestManagerSaveData questData = null;
        public List<DoorSaveData> doorData = new();
        public TutorialSaveData tutorialData = null;
        public List<ChallengeSaveData> challengeData = new();
        /// <summary>The inventory slots in use; copied from the inventory on save, restored on load.</summary>
        public List<ItemSaveData> inventoryData = new();
        /// <summary>Items placed in levels (scene-scoped ids): which ones were picked up.</summary>
        public List<WorldItemSaveData> worldItemData = new();
    }
}
