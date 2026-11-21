using System.Collections.Generic;

namespace TnT.EduGame.Dungeon
{
    /// <summary>Where a room lies relative to the route from the start to the goal.</summary>
    public enum RoomRole { Start, Goal, Path, Side }

    /// <summary>
    /// One room of a <see cref="DungeonRoute"/>. <see cref="PathIndex"/> is the position in
    /// <see cref="DungeonRoute.PathRooms"/> of the path room this room is on (path rooms, start and goal)
    /// or hangs off (side rooms: the nearest path room, the lowest index on a tie); -1 when the room
    /// can't be reached. <see cref="Depth"/> is the number of corridors from that path room (0 on the path).
    /// </summary>
    public sealed record RoomInfo(int Room, RoomRole Role, int PathIndex, int Depth, int HopsFromStart, bool IsDeadEnd)
    {
        public bool IsReachable => PathIndex >= 0;
    }

    /// <summary>
    /// The route through a dungeon (card dungeon-pop-5): the start and goal rooms, the main path between
    /// them (the graph's shortest way) and every room's place relative to it. Plain data, from
    /// <see cref="RoutePlanner"/>.
    /// </summary>
    public sealed class DungeonRoute
    {
        public DungeonRoute(int start, int goal, IReadOnlyList<int> pathRooms, IReadOnlyList<int> pathCorridors, IReadOnlyList<RoomInfo> rooms)
        {
            Start = start;
            Goal = goal;
            PathRooms = pathRooms;
            PathCorridors = pathCorridors;
            Rooms = rooms;
        }

        public int Start { get; }
        public int Goal { get; }

        /// <summary>The rooms from the start to the goal, both included.</summary>
        public IReadOnlyList<int> PathRooms { get; }

        /// <summary>The corridors between the path rooms in walking order: corridor i joins PathRooms[i] and PathRooms[i + 1].</summary>
        public IReadOnlyList<int> PathCorridors { get; }

        /// <summary>Per room index.</summary>
        public IReadOnlyList<RoomInfo> Rooms { get; }

        /// <summary>The number of corridors on the main path (its steps).</summary>
        public int Steps => PathCorridors.Count;
    }
}
