using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godoodads.Dungeon.Core;

namespace TnT.EduGame.Dungeon
{
    /// <summary>Where a lock's clues go: always somewhere the player reaches before the lock.</summary>
    public enum CluePlacement
    {
        /// <summary>The path room just before the lock (an earlier path room when it is full).</summary>
        PathRoomBefore,
        /// <summary>A side room hanging off a path room before the lock (the path room before when there is none).</summary>
        SideRoomBefore,
    }

    /// <summary>
    /// A locked door on the main path with its clues before it (card dungeon-pop-6b, gate pass). The door
    /// goes on a corridor of the path, on the door of the next path room (the one the lock leads into, which
    /// <see cref="DungeonEncounter.Rooms"/> filters). A door cell shared by several corridors locks all of
    /// them. With no step asked for, it prefers a corridor whose lock cuts the goal off from the start (a
    /// lock on a loop that can be walked round only when there is nothing else). It places its own clues
    /// (Composite), with the lock's seed, so door and clues show the same values. Corridor junctions that make
    /// a way round are ignored (accepted, kids' game).
    /// </summary>
    [GlobalClass, Tool]
    public partial class LockEncounter : DungeonEncounter
    {
        /// <summary>The door, built N-S (outside to the north, -Z); e.g. <c>doors/room_door_key.tscn</c>.</summary>
        [Export] public PackedScene DoorScene { get; set; }

        /// <summary>A clue, e.g. <c>doors/code_clue_board.tscn</c>.</summary>
        [Export] public PackedScene ClueScene { get; set; }

        [Export(PropertyHint.Range, "0,4,1")] public int ClueCount { get; set; } = 1;

        [Export] public CluePlacement CluePlacement { get; set; } = CluePlacement.PathRoomBefore;

        public override EncounterPass Pass => EncounterPass.Gate;

        public override bool Plan(EncounterContext context, EncounterTarget target)
        {
            var route = context.Route;
            var rng = context.RandomFor($"lock:{Id}:{context.Placements.Count(p => p.Encounter == this)}");

            var candidates = new List<(int Step, GraphDoor Door, int DoorIndex, bool Cuts)>();
            var steps = target.Step >= 0 ? new[] { target.Step } : Enumerable.Range(0, route.Steps);
            foreach (int step in steps)
            {
                if (step < 0 || step >= route.Steps)
                    continue;
                int next = route.PathRooms[step + 1];
                if (target.Room >= 0 && target.Room != next)
                    continue;
                if (Rooms != null && !Rooms.Matches(route.Rooms[next], route))
                    continue;
                int doorIndex = DoorInto(context.Graph, route.PathCorridors[step], next);
                if (doorIndex < 0)
                    continue;
                var door = context.Graph.Doors[doorIndex];
                if (context.IsDoorTaken(door.Cell))
                    continue;
                candidates.Add((step, door, doorIndex, CutsGoalOff(context.Graph, route, door.Cell)));
            }
            if (candidates.Count == 0)
                return false;

            // locks that really block first; then a seeded pick, so regenerating varies
            var pool = candidates.Any(c => c.Cuts) ? candidates.Where(c => c.Cuts).ToList() : candidates;
            foreach (var choice in pool.OrderBy(_ => rng.Next()).ToList())
            {
                var clueCells = PlanClues(context, choice.Step, choice.Door.Cell, rng);
                if (clueCells == null)
                    continue;   // no room for the clues before this lock: try another

                context.TryTakeDoor(choice.Door.Cell);
                int seed = rng.Next();
                int lockIndex = context.Add(new Placement(PlacementKind.Lock, this, choice.Door.Room, choice.Door.Cell, seed)
                {
                    Corridor = route.PathCorridors[choice.Step],
                    DoorDirection = RoomCellPicker.Outward(choice.Door.Side),
                });
                foreach (var (room, cell) in clueCells)
                    context.Add(new Placement(PlacementKind.Clue, this, room, cell, seed) { Owner = lockIndex });
                return true;
            }
            return false;
        }

        /// <summary>The door of <paramref name="corridor"/> in <paramref name="room"/>, as an index in the graph's doors; -1 if it has none.</summary>
        static int DoorInto(DungeonGraphData graph, int corridor, int room)
        {
            var c = graph.Corridors[corridor];
            int door = c.RoomA == room ? c.DoorA : c.RoomB == room ? c.DoorB : -1;
            return door >= 0 && door < graph.Doors.Count ? door : -1;
        }

        /// <summary>The corridors a lock on this door cell closes: every corridor with an end at that door.</summary>
        static HashSet<int> ClosedBy(DungeonGraphData graph, Vector3I doorCell)
        {
            var closed = new HashSet<int>();
            foreach (var c in graph.Corridors)
                if ((c.DoorA >= 0 && graph.Doors[c.DoorA].Cell == doorCell) || (c.DoorB >= 0 && graph.Doors[c.DoorB].Cell == doorCell))
                    closed.Add(c.Index);
            return closed;
        }

        /// <summary>The rooms the player reaches from the start with the corridors behind this door closed.</summary>
        static bool[] ReachableWithout(DungeonGraphData graph, int start, HashSet<int> closed)
        {
            var seen = new bool[graph.Rooms.Count];
            var queue = new Queue<int>();
            seen[start] = true;
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                int at = queue.Dequeue();
                foreach (int corridor in graph.Rooms[at].Corridors)
                {
                    if (closed.Contains(corridor))
                        continue;
                    int next = graph.Corridors[corridor].Other(at);
                    if (!seen[next])
                    {
                        seen[next] = true;
                        queue.Enqueue(next);
                    }
                }
            }
            return seen;
        }

        static bool CutsGoalOff(DungeonGraphData graph, DungeonRoute route, Vector3I doorCell) =>
            !ReachableWithout(graph, route.Start, ClosedBy(graph, doorCell))[route.Goal];

        /// <summary>
        /// Picks and takes the cells for the clues of a lock at <paramref name="step"/>, or null (nothing taken)
        /// when there is no room for all of them before the lock.
        /// </summary>
        List<(int Room, Vector3I Cell)> PlanClues(EncounterContext context, int step, Vector3I doorCell, Random rng)
        {
            var result = new List<(int, Vector3I)>();
            if (ClueCount <= 0)
                return result;

            var route = context.Route;
            var before = ReachableWithout(context.Graph, route.Start, ClosedBy(context.Graph, doorCell));
            // path rooms before the lock, nearest first
            var pathRooms = Enumerable.Range(0, step + 1).Reverse().Select(i => route.PathRooms[i]).Where(r => before[r]).ToList();
            var rooms = new List<int>();
            if (CluePlacement == CluePlacement.SideRoomBefore)
                rooms.AddRange(route.Rooms
                    .Where(r => r.Role == RoomRole.Side && r.IsReachable && r.PathIndex <= step && before[r.Room])
                    .Select(r => r.Room).OrderBy(_ => rng.Next()));
            rooms.AddRange(pathRooms);

            // all clues must fit before anything is taken
            int needed = ClueCount;
            var plan = new List<(int Room, int Count)>();
            foreach (int room in rooms)
            {
                int take = Math.Min(needed, context.FreeCells(room).Count);
                if (take > 0)
                {
                    plan.Add((room, take));
                    needed -= take;
                }
                if (needed == 0)
                    break;
            }
            if (needed > 0)
                return null;

            foreach (var (room, count) in plan)
                for (int i = 0; i < count; i++)
                {
                    context.TryTakeCell(room, rng, out var cell);
                    result.Add((room, cell));
                }
            return result;
        }
    }
}
