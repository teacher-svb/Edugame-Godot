using System;
using Godot;
using TnT.Systems.Persistence;

namespace TnT.EduGame
{
    [GlobalClass]
    public partial class TutorialSaveData : Resource, ISaveable
    {
        [Export] public string Id { get; set; } = Guid.NewGuid().ToString();
        [Export] public bool IsNew { get; set; }
        [Export] public bool IsCompleted { get; set; }
    }
}
