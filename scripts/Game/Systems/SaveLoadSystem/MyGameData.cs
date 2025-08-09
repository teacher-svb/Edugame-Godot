using System;
using System.Collections.Generic;
using TnT.Extensions;
using TnT.Systems.Persistence;
using Godot;

namespace TnT.EduGame
{
    [GlobalClass]
    public partial class MyGameData : GameData
    {
        [Export] public string CurrentLevelName;
        // [SerializeField]
        // public Character.CharacterSaveData playerData = null;
        // [SerializeField]
        // public List<Character.CharacterSaveData> characterData = new();
        // [SerializeField]
        // public QuestManager.QuestManagerSaveData questData = null;
        // public List<Door.DoorSaveData> doorData = new();
    }
}