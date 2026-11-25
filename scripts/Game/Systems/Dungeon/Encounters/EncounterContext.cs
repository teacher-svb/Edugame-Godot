using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godoodads.Dungeon.Core;
using TnT.EduGame.Challenges;

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
        readonly HashSet<Vector3I> _avoidedCells = new();
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

        /// <summary>The room's free cells that nothing has taken yet (avoided cells don't count).</summary>
        public IReadOnlyList<Vector3I> FreeCells(int room) =>
            CellsOf(room).Where(c => !_takenCells.Contains(c) && !_avoidedCells.Contains(c)).ToList();

        /// <summary>Takes a random free cell of the room, never an avoided one; false when the room is full.</summary>
        public bool TryTakeCell(int room, Random rng, out Vector3I cell)
        {
            var cells = CellsOf(room);
            if (!RoomCellPicker.TryPick(cells, _takenCells, rng, out cell))
                return false;
            // drawn as if avoided cells were free, and drawn again only when the pick is one, from a stream of its
            // own: a plan that never lands on an avoided cell stays the same as without them, and one that does
            // only moves that cell (the caller's stream, e.g. the lock's code, is not touched)
            if (_avoidedCells.Contains(cell)
                && !RoomCellPicker.TryPick(cells, new HashSet<Vector3I>(_takenCells.Concat(_avoidedCells)),
                    RandomFor($"avoid:{room}:{cell}"), out cell))
                return false;
            _takenCells.Add(cell);
            return true;
        }

        /// <summary>Keeps cells free of everything planned, e.g. the goal room's fixed set (<see cref="GoalAnchor"/>).</summary>
        public void Reserve(IEnumerable<Vector3I> cells)
        {
            foreach (var cell in cells ?? Array.Empty<Vector3I>())
                _takenCells.Add(cell);
        }

        /// <summary>
        /// Keeps cells free of everything planned, like <see cref="Reserve"/>, without changing the plans that don't
        /// pick them: used for the player's spawn in the start room (finding 6.1), so a dungeon keeps its layout for a
        /// seed unless something was planned onto the spawn.
        /// </summary>
        public void Avoid(IEnumerable<Vector3I> cells)
        {
            foreach (var cell in cells ?? Array.Empty<Vector3I>())
                _avoidedCells.Add(cell);
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
