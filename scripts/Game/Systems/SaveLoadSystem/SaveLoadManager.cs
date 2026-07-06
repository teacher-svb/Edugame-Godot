using System;
using TnT.Extensions;
using TnT.Systems.Persistence;
using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.QuestSystem;
using TnT.EduGame.GameState;

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
            Instance = this;
            GameData = new MyGameData { Name = _gameName, CurrentLevelName = _startScene };
        }
        
        protected override void OnSceneLoaded(string scenePath)
        {
            GD.Print("SaveLoadManager OnSceneLoaded");
            GameData.CurrentLevelName = scenePath;

            Bind<Character3D, CharacterSaveData>(ref GameData.characterData);
            Bind<QuestManager, QuestManagerSaveData>(ref GameData.questData);
            Bind<Door, DoorSaveData>(ref GameData.doorData);
            Bind<PlayerTutorial, TutorialSaveData>(ref GameData.tutorialData);

            StateManagerGame.Instance.StartPlay();
        }

        protected void LoadScene(string scenePath)
        {
            StateManagerGame.Instance.LoadScene(scenePath, Vector3.Zero);
        }

        public override void NewGame()
        {
            GD.Print("SaveLoadManager newgame");
            GameData = new MyGameData
            {
                Name = _gameName,
                CurrentLevelName = _startScene,
            };
            LoadScene(GameData.CurrentLevelName);
        }

        public override void LoadGame(string gameName)
        {
            GD.Print("SaveLoadManager LoadGame");
            GameData = dataService.Load(gameName);

            GD.Print(GameData.CurrentLevelName);

            if (String.IsNullOrWhiteSpace(GameData.CurrentLevelName))
            {
                GameData.CurrentLevelName = _startScene;
            }
            LoadScene(GameData.CurrentLevelName);
        }
    }
}
