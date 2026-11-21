using System;
using System.Collections.Generic;
using System.Linq;
using Godoodads.Dungeon.Core;

namespace TnT.EduGame.Dungeon
{
    /// <summary>
    /// Plans the route through a generated dungeon from its graph alone (card dungeon-pop-5): the start
    /// room, the goal room, the main path between them, and where every other room hangs off it. Pure C#
    /// over godoodungeon's <see cref="DungeonGraphData"/>; the addon itself is not changed.
    /// <list type="bullet">
    /// <item>A room whose role (a room box's <c>Data["role"]</c>) is <see cref="StartRole"/> / <see cref="GoalRole"/>
    /// is the start / goal (the lowest index when several are).</item>
    /// <item>Neither given: the start is one end of the graph's diameter (which end comes from the seed), the
    /// goal the other. Only one given: the other is the room farthest from it.</item>
    /// <item>The main path is the graph's shortest way; corridor junctions that make a shortcut are not
    /// edges and are ignored (accepted, it's a kids' game).</item>
    /// </list>
    /// Same graph, roles and seed → same route.
    /// </summary>
    public static class RoutePlanner
    {
        public const string StartRole = "start";
        public const string GoalRole = "goal";

        /// <param name="roomRoles">Per room index, its role from the room box (null or empty for none); may be null or shorter.</param>
        public static DungeonRoute Plan(DungeonGraphData graph, IReadOnlyList<string> roomRoles, long seed)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            int count = graph.Rooms.Count;
            if (count == 0)
                throw new ArgumentException("The dungeon graph has no rooms.", nameof(graph));

            var (start, goal) = PickEnds(graph, roomRoles, seed);
            var pathRooms = graph.ShortestPath(start, goal);
            var pathCorridors = graph.ShortestPathCorridors(start, goal);
            if (pathRooms.Length == 0)
                throw new InvalidOperationException($"No way from the start (room {start}) to the goal (room {goal}).");

            var fromStart = graph.HopDistances(start);
            // hops from every path room, to hang each side room off its nearest one
            var fromPath = pathRooms.Select(graph.HopDistances).ToArray();
            var onPath = new Dictionary<int, int>();
            for (int i = 0; i < pathRooms.Length; i++)
                onPath.TryAdd(pathRooms[i], i);

            var rooms = new RoomInfo[count];
            for (int r = 0; r < count; r++)
            {
                bool deadEnd = graph.Neighbours(r).Length == 1;
                if (onPath.TryGetValue(r, out int index))
                {
                    var role = r == start ? RoomRole.Start : r == goal ? RoomRole.Goal : RoomRole.Path;
                    rooms[r] = new RoomInfo(r, role, index, 0, fromStart[r], deadEnd);
                    continue;
                }

                int best = -1, bestDepth = -1;
                for (int i = 0; i < pathRooms.Length; i++)
                {
                    int hops = fromPath[i][r];
                    if (hops >= 0 && (best < 0 || hops < bestDepth))
                    {
                        best = i;
                        bestDepth = hops;
                    }
                }
                rooms[r] = new RoomInfo(r, RoomRole.Side, best, bestDepth, fromStart[r], deadEnd);
            }

            return new DungeonRoute(start, goal, pathRooms, pathCorridors, rooms);
        }

        static (int Start, int Goal) PickEnds(DungeonGraphData graph, IReadOnlyList<string> roomRoles, long seed)
        {
            int start = RoomWithRole(roomRoles, StartRole, graph.Rooms.Count);
            int goal = RoomWithRole(roomRoles, GoalRole, graph.Rooms.Count);

            if (start >= 0 && goal >= 0)
                return (start, goal);
            if (start >= 0)
                return (start, graph.FarthestRoom(start));
            if (goal >= 0)
                return (graph.FarthestRoom(goal), goal);

            var (a, b) = graph.Diameter();
            if (a < 0)
                return (0, 0);   // a single room: start and goal are the same
            // which end is the start varies with the seed, so regenerating keeps some variety
            return (seed & 1) == 0 ? (a, b) : (b, a);
        }

        static int RoomWithRole(IReadOnlyList<string> roomRoles, string role, int roomCount)
        {
            if (roomRoles == null)
                return -1;
            for (int r = 0; r < Math.Min(roomRoles.Count, roomCount); r++)
                if (string.Equals(roomRoles[r], role, StringComparison.OrdinalIgnoreCase))
                    return r;
            return -1;
        }
    }
}
