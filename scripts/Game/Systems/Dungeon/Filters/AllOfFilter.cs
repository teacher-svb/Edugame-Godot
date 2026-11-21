using System.Linq;
using Godot;

namespace TnT.EduGame.Dungeon
{
    /// <summary>Rooms every filter accepts (empty: all rooms).</summary>
    [GlobalClass, Tool]
    public partial class AllOfFilter : RoomFilter
    {
        [Export] public Godot.Collections.Array<RoomFilter> Filters { get; set; } = new();

        public override bool Matches(RoomInfo room, DungeonRoute route) =>
            Filters.Where(f => f != null).All(f => f.Matches(room, route));
    }
}
