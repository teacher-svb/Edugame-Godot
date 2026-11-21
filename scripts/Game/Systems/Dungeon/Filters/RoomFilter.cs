using Godot;

namespace TnT.EduGame.Dungeon
{
    /// <summary>
    /// Which rooms an encounter may use (card dungeon-pop-6a, Specification): small rules that combine
    /// with <see cref="AllOfFilter"/> / <see cref="AnyOfFilter"/> / <see cref="NotFilter"/> in the inspector,
    /// e.g. "a side room that is a dead end, at least 2 rooms from the start". Unreachable rooms never get here.
    /// </summary>
    [GlobalClass, Tool]
    public abstract partial class RoomFilter : Resource
    {
        public abstract bool Matches(RoomInfo room, DungeonRoute route);
    }
}
