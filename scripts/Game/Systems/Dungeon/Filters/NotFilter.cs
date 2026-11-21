using Godot;

namespace TnT.EduGame.Dungeon
{
    /// <summary>Rooms the inner filter rejects (empty: no rooms).</summary>
    [GlobalClass, Tool]
    public partial class NotFilter : RoomFilter
    {
        [Export] public RoomFilter Filter { get; set; }

        public override bool Matches(RoomInfo room, DungeonRoute route) => Filter != null && !Filter.Matches(room, route);
    }
}
