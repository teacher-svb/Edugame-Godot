using System;
using TnT.Extensions;
using TnT.Systems.Persistence;
using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.QuestSystem;
using TnT.EduGame.GameStates;
using TnT.EduGame.Challenges;

namespace TnT.EduGame
{
    public partial class SaveLoadManager : SaveLoadSystem<MyGameData>
    {
        public static SaveLoadManager Instance { get; private set; }
        [Export] public override MyGameData GameData { get; set; }

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

            // characters before quests (quest steps can move them), doors and challenges after, as before
            Bind<Character3D, CharacterSaveData>(GameData.characterData, null);
            Bind<QuestManager, QuestManagerSaveData>(ref GameData.questData);
            Bind<PlayerTutorial, TutorialSaveData>(ref GameData.tutorialData);
            BindEntities(null);

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

        // Doors and challenges, bound per subtree: one save entry per PersistentId. Add new saveable types here
        // (characters are bound separately, before the quests).
        void BindEntities(Node root)
        {
            Bind<Door, DoorSaveData>(GameData.doorData, root);
            Bind<Doors.CodeDoor, DoorSaveData>(GameData.doorData, root);
            Bind<MathChallengeTrigger, ChallengeSaveData>(GameData.challengeData, root);
        }

        protected void LoadScene(string scenePath)
        {
            StateManagerGame.Instance.LoadScene(scenePath, Vector3.Zero);
        }

        public override void NewGame()
        {
            GameData = new MyGameData
            {
                Name = _gameName,
                CurrentLevelName = _startScene,
            };
            QuestManager.Instance?.Restore(null);   // before the level's _Ready: no quest state from the running session
            LoadScene(GameData.CurrentLevelName);
        }

        public override void LoadGame(string gameName)
        {
            GameData = dataService.Load(gameName);
            QuestManager.Instance?.Restore(GameData.questData);   // before the level's _Ready, see QuestManager.Restore

            if (String.IsNullOrWhiteSpace(GameData.CurrentLevelName))
            {
                GameData.CurrentLevelName = _startScene;
            }
            LoadScene(GameData.CurrentLevelName);
        }
    }
}
