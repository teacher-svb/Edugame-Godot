using System.Linq;
using Godot;

namespace TnT.EduGame.Dungeon
{
    /// <summary>Rooms at least one filter accepts (empty: no rooms).</summary>
    [GlobalClass, Tool]
    public partial class AnyOfFilter : RoomFilter
    {
        [Export] public Godot.Collections.Array<RoomFilter> Filters { get; set; } = new();

        public override bool Matches(RoomInfo room, DungeonRoute route) =>
            Filters.Where(f => f != null).Any(f => f.Matches(room, route));
    }
}
