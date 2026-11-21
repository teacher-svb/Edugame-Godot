using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godoodads.Dungeon.Core;

namespace TnT.EduGame.Dungeon
{
    /// <summary>
    /// The cells of a room where something can stand (card dungeon-pop-6a): its footprint minus its door
    /// cells and the cell just inside each door (the way in stays clear), kept only where <c>isFreeFloor</c>
    /// says so. The wall ring counts: with 4 m cells a wall tile is mostly floor along a wall, and a 3x3 room
    /// has no other room left (its centre is the way in). So <c>isFreeFloor</c> decides which tiles can hold
    /// something (plain floors and walls, say, not pits or pillars). GridMap cells on the room's bottom
    /// layer. Pure C#: the caller decides what free floor is.
    /// </summary>
    public static class RoomCellPicker
    {
        /// <summary>The free cells of the room, in a fixed order (by Z, then X). <paramref name="isFreeFloor"/> null keeps every cell but the doors and their ways in.</summary>
        public static List<Vector3I> FreeCells(DungeonGraphData graph, int room, Func<Vector3I, bool> isFreeFloor = null)
        {
            var info = graph.Rooms[room];
            var blocked = new HashSet<Vector3I>(info.Doors.SelectMany(d => new[] { graph.Doors[d].Cell, InsideDoor(graph.Doors[d]) }));
            var cells = new List<Vector3I>();
            for (int z = info.Position.Z; z < info.Position.Z + info.Size.Z; z++)
                for (int x = info.Position.X; x < info.Position.X + info.Size.X; x++)
                {
                    var cell = new Vector3I(x, info.Position.Y, z);
                    if (!blocked.Contains(cell) && (isFreeFloor?.Invoke(cell) ?? true))
                        cells.Add(cell);
                }
            return cells;
        }

        /// <summary>One step out of the room through a door side, as a GridMap offset (godoodungeon's PosY is +Z).</summary>
        public static Vector3I Outward(DoorSide side)
        {
            var step = side.Offset();
            return new Vector3I(step.X, 0, step.Y);
        }

        /// <summary>The room cell just inside a door.</summary>
        public static Vector3I InsideDoor(GraphDoor door) => door.Cell - Outward(door.Side);

        /// <summary>A random cell of <paramref name="cells"/> not in <paramref name="taken"/>; false when none is left.</summary>
        public static bool TryPick(IReadOnlyList<Vector3I> cells, ICollection<Vector3I> taken, Random rng, out Vector3I cell)
        {
            var left = cells.Where(c => !taken.Contains(c)).ToList();
            if (left.Count == 0)
            {
                cell = default;
                return false;
            }
            cell = left[rng.Next(left.Count)];
            return true;
        }
    }
}
