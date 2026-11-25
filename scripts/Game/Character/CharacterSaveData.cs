using System;
using Godot;
using TnT.Systems.Persistence;

namespace TnT.EduGame.Characters
{
    // plain C# (not a Godot Resource): Newtonsoft writes only these fields (Vector3 as X/Y/Z)
    public class CharacterSaveData : ISaveable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool IsNew { get; set; }
        public Vector3 position;
        public string characterId;
    }
}
