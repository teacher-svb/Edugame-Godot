using System;
using Godot;
using TnT.Systems.Persistence;

namespace TnT.EduGame.Question
{
    // Seed of a challenge round the player hasn't solved yet, so a retry (also
    // after save/load) shows the exact same values.
    [GlobalClass]
    public partial class ChallengeSaveData : Resource, ISaveable
    {
        [Export] public string Id { get; set; } = Guid.NewGuid().ToString();
        [Export] public bool IsNew { get; set; }
        [Export] public bool HasPendingSeed { get; set; }
        [Export] public int PendingSeed { get; set; }
    }
}
