using Godot;

namespace TnT.EduGame.Dungeon
{
    /// <summary>Rooms with one neighbour (or, unticked, rooms with more).</summary>
    [GlobalClass, Tool]
    public partial class DeadEndFilter : RoomFilter
    {
        [Export] public bool DeadEnd { get; set; } = true;

        public override bool Matches(RoomInfo room, DungeonRoute route) => room.IsDeadEnd == DeadEnd;
    }
}
