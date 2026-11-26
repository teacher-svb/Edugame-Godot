using System;
using TnT.Systems.Persistence;
using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.QuestSystem;
using TnT.EduGame.GameStates;
using TnT.EduGame.Challenges;
using TnT.EduGame.UI;
using TnT.EduGame.Inventory;

namespace TnT.EduGame
{
    public partial class SaveLoadManager : SaveLoadSystem<MyGameData>
    {
        public static SaveLoadManager Instance { get; private set; }
        public override MyGameData GameData { get; set; }

        public override void _Ready()
        {
            _gameName = "test";
            base._Ready();
            StateManagerGame.Instance.SceneLoaded += OnSceneLoaded;
            Instance = this;
            GameData = new MyGameData { Name = _gameName, CurrentLevelName = _startScene };
        }
        
        private void OnSceneLoaded(string scenePath)
        {
            GameData.CurrentLevelName = scenePath;

            // quests last: their Bound event runs the listeners' catch-ups and resumes, which must see the bound
            // scene (characters at their saved positions, doors, challenge seeds) and not be overwritten by it
            Bind<Character3D, CharacterSaveData>(GameData.characterData, null);
            Bind<PlayerTutorial, TutorialSaveData>(ref GameData.tutorialData);
            BindEntities(null);
            Bind<QuestManager, QuestManagerSaveData>(ref GameData.questData);

            StateManagerGame.Instance.StartPlay();
        }

        /// <summary>
        /// Binds the saved data of nodes added after the scene loaded (doors and challenges placed by a dungeon
        /// populator, say): call it once they are in the tree. Their PersistentId (node path) must be the same on
        /// every run. Binding a node again (spawned before the scene finished loading) adds nothing.
        /// </summary>
        public void BindSubtree(Node root)
        {
            if (root == null)
                return;
            Bind<Character3D, CharacterSaveData>(GameData.characterData, root);
            BindEntities(root);
        }

        // Doors, challenges and world items, bound per subtree: one save entry per PersistentId. Add new saveable
        // types here (characters are bound separately, before the quests).
        void BindEntities(Node root)
        {
            Bind<Door, DoorSaveData>(GameData.doorData, root);
            Bind<Doors.CodeDoor, DoorSaveData>(GameData.doorData, root);
            Bind<Doors.StructureDoor, DoorSaveData>(GameData.doorData, root);
            Bind<MathChallengeTrigger, ChallengeSaveData>(GameData.challengeData, root);
            Bind<Item, WorldItemSaveData>(GameData.worldItemData, root);   // a picked-up item removes itself
        }

        protected void LoadScene(string scenePath)
        {
            StateManagerGame.Instance.LoadScene(scenePath);   // no target: the player keeps the saved position
        }

        public override void NewGame()
        {
            GameData = new MyGameData
            {
                Name = _gameName,
                CurrentLevelName = _startScene,
            };
            QuestManager.Instance?.Restore(null);   // before the level's _Ready: no quest state from the running session
            InventoryController.Instance?.model?.Restore(null);
            LoadScene(GameData.CurrentLevelName);
        }

        /// <summary>
        /// False while a quest reaction runs that a load wouldn't replay (e.g. the bridge lowering before its
        /// objective completes): a save then would store the objective halfway (finding 5.1). The pause menu
        /// greys out Save; see <see cref="QuestStepListener.BlockingReactionsChanged"/>.
        /// </summary>
        public bool CanSave => !QuestStepListener.AnyBlockingReaction;

        public override bool SaveGame() => SaveGame(null);

        /// <summary>
        /// Saves unless a quest reaction runs (<see cref="CanSave"/>); false (with a warning) when refused.
        /// <paramref name="autosaveIn"/>: the listener whose reaction asks for this save (<see cref="SaveGameStep"/>),
        /// whose own reaction doesn't count.
        /// </summary>
        public bool SaveGame(QuestStepListener autosaveIn)
        {
            int running = QuestStepListener.BlockingReactionsExcept(autosaveIn);
            if (running > 0)
            {
                GD.PushWarning($"SaveLoadManager: not saved, {running} quest reaction(s) running");
                return false;
            }
            // the inventory lives in the managers scene, not in a level: copied into the save data here
            var inventory = InventoryController.Instance?.model;
            if (inventory != null)
                GameData.inventoryData = inventory.GetSaveData();
            return base.SaveGame();
        }

        public override bool LoadGame(string gameName)
        {
            if (!dataService.TryLoad(gameName, out var loaded))
                return false;   // logged; the running game stays as it was
            if (String.IsNullOrWhiteSpace(loaded.CurrentLevelName))
                loaded.CurrentLevelName = _startScene;
            if (!ResourceLoader.Exists(loaded.CurrentLevelName))
            {
                // check before anything is replaced: a failed scene load keeps the running level
                GD.PushError($"Save '{gameName}': level '{loaded.CurrentLevelName}' not found; nothing loaded");
                return false;
            }
            GameData = loaded;
            GameData.characterData ??= new();
            GameData.doorData ??= new();
            GameData.challengeData ??= new();
            GameData.inventoryData ??= new();
            GameData.worldItemData ??= new();
            QuestManager.Instance?.Restore(GameData.questData);   // before the level's _Ready, see QuestManager.Restore
            InventoryController.Instance?.model?.Restore(GameData.inventoryData);

            LoadScene(GameData.CurrentLevelName);
            return true;
        }
    }
}
