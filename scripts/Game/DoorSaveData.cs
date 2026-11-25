using System;
using TnT.Systems.Persistence;

namespace TnT.EduGame
{
    // plain C# (not a Godot Resource): Newtonsoft writes only these fields
    public class DoorSaveData : ISaveable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool IsNew { get; set; }
        public bool IsOpen { get; set; }
    }
}
