using Godot;

namespace TnT.EduGame.Dungeon
{
    public enum PlacementKind { Lock, Clue, Object }

    /// <summary>
    /// One thing the encounter planner puts in the dungeon (card dungeon-pop-6a): plain data that the
    /// populator turns into a scene. <see cref="Cell"/> is a GridMap cell: the door cell for a lock, a free
    /// floor cell otherwise. <see cref="Seed"/> seeds its challenge; a lock's clues share the lock's seed.
    /// </summary>
    public sealed record Placement(PlacementKind Kind, DungeonEncounter Encounter, int Room, Vector3I Cell, int Seed)
    {
        /// <summary>Locks: one step out of <see cref="Room"/> through the door, to turn the N-S built door scene.</summary>
        public Vector3I DoorDirection { get; init; }

        /// <summary>Clues: the index of their lock in the planned placements, else -1.</summary>
        public int Owner { get; init; } = -1;
    }
}
