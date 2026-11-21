using Godot;

namespace TnT.EduGame.Dungeon
{
    /// <summary>Rooms with one of the ticked roles on the route.</summary>
    [GlobalClass, Tool]
    public partial class RoleFilter : RoomFilter
    {
        [Export] public bool Start { get; set; }
        [Export] public bool Goal { get; set; }
        [Export] public bool Path { get; set; }
        [Export] public bool Side { get; set; }

        public override bool Matches(RoomInfo room, DungeonRoute route) => room.Role switch
        {
            RoomRole.Start => Start,
            RoomRole.Goal => Goal,
            RoomRole.Path => Path,
            _ => Side,
        };
    }
}
