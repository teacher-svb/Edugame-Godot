using Godot;

namespace TnT.EduGame.Dungeon
{
    /// <summary>Rooms between <see cref="Min"/> and <see cref="Max"/> corridors from the start (Max -1: no limit).</summary>
    [GlobalClass, Tool]
    public partial class HopsFromStartFilter : RoomFilter
    {
        [Export(PropertyHint.Range, "0,50,1,or_greater")] public int Min { get; set; }
        [Export(PropertyHint.Range, "-1,50,1,or_greater")] public int Max { get; set; } = -1;

        public override bool Matches(RoomInfo room, DungeonRoute route) =>
            room.HopsFromStart >= Min && (Max < 0 || room.HopsFromStart <= Max);
    }
}
