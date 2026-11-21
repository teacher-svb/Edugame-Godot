using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godoodads.Dungeon.Core;
using TnT.EduGame.Question;

namespace TnT.EduGame.Dungeon
{
    /// <summary>
    /// What encounters plan against (card dungeon-pop-6a, Builder): the graph and route, rooms by filter,
    /// free cells and doors that are handed out only once, a separate replayable random stream per purpose,
    /// and the placements planned so far. Same graph, route, free-floor check and seed → same answers.
    /// </summary>
    public sealed class EncounterContext
    {
        readonly Func<Vector3I, bool> _isFreeFloor;
        readonly int _seed;
        readonly Dictionary<int, List<Vector3I>> _freeCells = new();
        readonly HashSet<Vector3I> _takenCells = new();
        readonly HashSet<Vector3I> _takenDoors = new();
        readonly List<Placement> _placements = new();

        /// <param name="isFreeFloor">Whether something may stand on a cell (null: every cell but doors and their ways in).</param>
        public EncounterContext(DungeonGraphData graph, DungeonRoute route, Func<Vector3I, bool> isFreeFloor, long seed)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            _isFreeFloor = isFreeFloor;
            _seed = unchecked((int)(seed ^ (seed >> 32)));
        }

        public DungeonGraphData Graph { get; }
        public DungeonRoute Route { get; }
        public IReadOnlyList<Placement> Placements => _placements;

        /// <summary>A random stream for one purpose (e.g. "key_lock:3"): the same purpose always gives the same stream.</summary>
        public Random RandomFor(string purpose) => ChallengeRound.RandomFor(_seed, "dungeon:" + purpose);

        /// <summary>The reachable rooms the filter accepts (all of them for null), by room index.</summary>
        public IReadOnlyList<int> RoomsMatching(RoomFilter filter) =>
            Route.Rooms.Where(r => r.IsReachable && (filter?.Matches(r, Route) ?? true)).Select(r => r.Room).ToList();

        /// <summary>The room's free cells that nothing has taken yet.</summary>
        public IReadOnlyList<Vector3I> FreeCells(int room) => CellsOf(room).Where(c => !_takenCells.Contains(c)).ToList();

        /// <summary>Takes a random free cell of the room; false when the room is full.</summary>
        public bool TryTakeCell(int room, Random rng, out Vector3I cell)
        {
            if (!RoomCellPicker.TryPick(CellsOf(room), _takenCells, rng, out cell))
                return false;
            _takenCells.Add(cell);
            return true;
        }

        public bool IsDoorTaken(Vector3I doorCell) => _takenDoors.Contains(doorCell);

        /// <summary>Takes a door cell (e.g. for a lock); false when something already has it.</summary>
        public bool TryTakeDoor(Vector3I doorCell) => _takenDoors.Add(doorCell);

        /// <summary>Adds a placement and returns its index (a clue's <see cref="Placement.Owner"/>).</summary>
        public int Add(Placement placement)
        {
            _placements.Add(placement);
            return _placements.Count - 1;
        }

        List<Vector3I> CellsOf(int room)
        {
            if (!_freeCells.TryGetValue(room, out var cells))
                _freeCells[room] = cells = RoomCellPicker.FreeCells(Graph, room, _isFreeFloor);
            return cells;
        }
    }
}
