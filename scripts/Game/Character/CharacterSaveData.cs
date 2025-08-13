using System;
using Godot;
using TnT.Systems.Persistence;

namespace TnT.EduGame.Characters
{
    [GlobalClass]
    public partial class CharacterSaveData : Resource, ISaveable
    {
        [Export] public string Id { get; set; } = Guid.NewGuid().ToString();
        [Export] public bool IsNew { get; set; }
        [Export] public Vector2 position;
        [Export] public string characterId;
    }
}