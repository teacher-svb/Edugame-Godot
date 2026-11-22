using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godoodads.Dungeon.Core;

namespace TnT.EduGame.Dungeon
{
    /// <summary>
    /// Where the goal room's fixed set goes (card act1-chamber-goal-rooms): the Spark Test platform, the wizard and
    /// the exit. <see cref="Cell"/> holds the platform, <see cref="Behind"/> the wizard and the exit (one step
    /// further from the entrance). <see cref="Facing"/> is one step out through the entrance door, as a GridMap
    /// offset: the set's +Z points that way, so the wizard faces whoever walks in.
    /// </summary>
    public readonly record struct GoalAnchor(Vector3I Cell, Vector3I Behind, Vector3I Facing);

    /// <summary>
    /// Picks the <see cref="GoalAnchor"/> in the goal room of a route: the inner floor cell farthest from the entrance
    /// door (the far end of the room), whose cell behind isn't a door. Cells just inside a door (the ways in) only
    /// when nothing else fits, e.g. a 3x3 room. Pure C#: the caller says which cells are floor (no walls, pillars).
    /// </summary>
    public static class GoalAnchorPicker
    {
        /// <summary>The anchor; null when the goal room has no floor cell at all.</summary>
        public static GoalAnchor? Pick(DungeonGraphData graph, DungeonRoute route, Func<Vector3I, bool> isGoalFloor)
        {
            if (graph == null || route == null || route.Goal < 0 || route.Goal >= graph.Rooms.Count)
                return null;

            var room = graph.Rooms[route.Goal];
            var entrance = Entrance(graph, route);
            var facing = entrance != null ? RoomCellPicker.Outward(entrance.Side) : new Vector3I(0, 0, 1);
            var from = entrance?.Cell ?? graph.Rooms[route.Goal].Position;

            var doors = new HashSet<Vector3I>(room.Doors.Select(d => graph.Doors[d].Cell));
            var waysIn = new HashSet<Vector3I>(room.Doors.Select(d => RoomCellPicker.InsideDoor(graph.Doors[d])));

            var cells = new List<Vector3I>();
            for (int z = room.Position.Z; z < room.Position.Z + room.Size.Z; z++)
                for (int x = room.Position.X; x < room.Position.X + room.Size.X; x++)
                {
                    var cell = new Vector3I(x, room.Position.Y, z);
                    if (!doors.Contains(cell) && !doors.Contains(cell - facing) && (isGoalFloor?.Invoke(cell) ?? true))
                        cells.Add(cell);
                }
            if (cells.Count == 0)
                return null;

            // farthest from the entrance; ways in last; then the fixed cell order, so the same dungeon gives the same pick
            var best = cells
                .OrderBy(c => waysIn.Contains(c))
                .ThenByDescending(c => Math.Abs(c.X - from.X) + Math.Abs(c.Z - from.Z))
                .First();
            return new GoalAnchor(best, best - facing, facing);
        }

        /// <summary>The goal room's door of the last corridor on the main path; null when the path has none (start is the goal).</summary>
        public static GraphDoor Entrance(DungeonGraphData graph, DungeonRoute route)
        {
            if (route.Steps == 0)
                return null;
            var corridor = graph.Corridors[route.PathCorridors[^1]];
            int door = corridor.RoomA == route.Goal ? corridor.DoorA : corridor.RoomB == route.Goal ? corridor.DoorB : -1;
            return door >= 0 && door < graph.Doors.Count ? graph.Doors[door] : null;
        }
    }
}
