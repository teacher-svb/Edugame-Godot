using System;
using System.Collections.Generic;
using TnT.Extensions;
using TnT.Systems.Persistence;
using Godot;
using TnT.EduGame.Characters;
using TnT.EduGame.QuestSystem;

namespace TnT.EduGame
{
    [GlobalClass]
    public partial class MyGameData : GameData
    {
        [Export] 
        public string CurrentLevelName;
        public List<CharacterSaveData> characterData = new();
        public QuestManagerSaveData questData = null;
        public List<DoorSaveData> doorData = new();
        public TutorialSaveData tutorialData = null;
    }
}