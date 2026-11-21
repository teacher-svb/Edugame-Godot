using Godot;

namespace TnT.EduGame.Dungeon
{
    /// <summary>
    /// When an encounter is planned: gates first (they split the route into sections; a gate places what
    /// belongs to it, such as a lock's clues, itself), fillers last.
    /// </summary>
    public enum EncounterPass { Gate, Filler }

    /// <summary>
    /// Where a recipe asks an encounter to go: a step of the main path (the corridor between path rooms
    /// <c>Step</c> and <c>Step + 1</c>), a room, or anywhere its filter allows (both -1).
    /// </summary>
    public readonly record struct EncounterTarget(int Step = -1, int Room = -1)
    {
        public static EncounterTarget Any => new(-1, -1);
        public bool IsAny => Step < 0 && Room < 0;
    }

    /// <summary>
    /// A kind of thing the player meets along the route (card dungeon-pop-6a, Strategy): a lock with its
    /// clues, a chest, the goal platform. A new kind is a new subclass, picked in a recipe in the inspector;
    /// nothing registers it. Encounters never place anything themselves: <see cref="Plan"/> asks the
    /// <see cref="EncounterContext"/> for rooms, free cells and doors and adds <see cref="Placement"/>s to it.
    /// </summary>
    [GlobalClass, Tool]
    public abstract partial class DungeonEncounter : Resource
    {
        /// <summary>Names the encounter in placements and node names, e.g. "key_lock".</summary>
        [Export] public string Id { get; set; } = "";

        /// <summary>Chance of being drawn from a recipe's pool, relative to the other pool entries.</summary>
        [Export(PropertyHint.Range, "0,10,0.1,or_greater")] public float Weight { get; set; } = 1f;

        /// <summary>The rooms it may use (all reachable rooms when empty).</summary>
        [Export] public RoomFilter Rooms { get; set; }

        public abstract EncounterPass Pass { get; }

        /// <summary>Plans one occurrence at <paramref name="target"/>; false when it found no place (nothing is added then).</summary>
        public abstract bool Plan(EncounterContext context, EncounterTarget target);
    }
}
