using System.Linq;
using Godot;

namespace TnT.EduGame.Dungeon
{
    /// <summary>
    /// A thing in a free cell of a room (card dungeon-pop-6b, filler pass): a chest, the goal platform, a
    /// decoration. The room: the one asked for, else path room <c>Step</c> when a step is asked for, else a
    /// seeded pick among the rooms <see cref="DungeonEncounter.Rooms"/> accepts that still have a free cell.
    /// </summary>
    [GlobalClass, Tool]
    public partial class ObjectEncounter : DungeonEncounter
    {
        [Export] public PackedScene Scene { get; set; }

        public override EncounterPass Pass => EncounterPass.Filler;

        public override bool Plan(EncounterContext context, EncounterTarget target)
        {
            var route = context.Route;
            var rng = context.RandomFor($"object:{Id}:{context.Placements.Count(p => p.Encounter == this)}");

            var rooms = target.Room >= 0 ? new[] { target.Room }
                : target.Step >= 0 ? (target.Step < route.PathRooms.Count ? new[] { route.PathRooms[target.Step] } : new int[0])
                : context.RoomsMatching(Rooms).OrderBy(_ => rng.Next()).ToArray();

            foreach (int room in rooms)
                if (context.TryTakeCell(room, rng, out var cell))
                {
                    context.Add(new Placement(PlacementKind.Object, this, room, cell, rng.Next()));
                    return true;
                }
            return false;
        }
    }
}
