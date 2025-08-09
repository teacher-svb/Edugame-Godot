using System;
using System.Collections.Generic;
using TnT.Extensions;
using TnT.Systems.Persistence;
using Godot;

namespace TnT.EduGame
{
    [Serializable]
    public partial class MyGameData : GameData
    {
        public string CurrentLevelName;
        // [SerializeField]
        // public Character.CharacterSaveData playerData = null;
        // [SerializeField]
        // public List<Character.CharacterSaveData> characterData = new();
        // [SerializeField]
        // public QuestManager.QuestManagerSaveData questData = null;
        // public List<Door.DoorSaveData> doorData = new();
    }

    public partial class SaveLoadManager : SaveLoadSystem<MyGameData>
    {
        public static SaveLoadManager Instance { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
        }
        
        // protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        // {
        //     if (scene.name == "_startScene") return;

        //     // Bind<Character, Character.CharacterSaveData>(ref GameData.playerData);
        //     Bind<Character, Character.CharacterSaveData>(ref GameData.characterData);
        //     Bind<QuestManager, QuestManager.QuestManagerSaveData>(ref GameData.questData);
        //     Bind<Door, Door.DoorSaveData>(ref GameData.doorData);

        //     GameData.CurrentLevelName = scene.name;
        // }

        protected void LoadScene(string sceneName)
        {
            // var player = GetTree().FindAnyObjectByType<Player>();
            // StateManagerGame.Instance.LoadScene(sceneName, player.transform.position);
        }

        public override void NewGame()
        {
            GameData = new MyGameData
            {
                Name = _gameName,
                CurrentLevelName = _startSceneName
            };
            LoadScene(GameData.CurrentLevelName);
        }

        public override void LoadGame(string gameName)
        {
            GameData = dataService.Load(gameName);

            GD.Print(GameData.CurrentLevelName);

            if (String.IsNullOrWhiteSpace(GameData.CurrentLevelName))
            {
                GameData.CurrentLevelName = _startSceneName;
            }
            LoadScene(GameData.CurrentLevelName);
        }
    }
}