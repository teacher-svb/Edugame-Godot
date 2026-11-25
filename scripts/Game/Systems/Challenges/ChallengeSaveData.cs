using System;
using TnT.Systems.Persistence;

namespace TnT.EduGame.Challenges
{
    // Seed of a challenge round the player hasn't solved yet, so a retry (also
    // after save/load) shows the exact same values. With KeepSolvedSeed (a door's
    // challenge) it also stays after the round is solved, for its clues.
    // Plain C# (not a Godot Resource): Newtonsoft writes only these fields.
    public class ChallengeSaveData : ISaveable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool IsNew { get; set; }
        public bool HasPendingSeed { get; set; }
        public int PendingSeed { get; set; }
    }
}
