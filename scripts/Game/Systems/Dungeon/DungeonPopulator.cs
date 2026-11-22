using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godoodads.Dungeon;
using TnT.EduGame.Doors;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.EduGame.Dungeon
{
    /// <summary>
    /// Fills a generated dungeon at runtime (card dungeon-pop-7b): plans the route (<see cref="RoutePlanner"/>)
    /// and the <see cref="Recipe"/>'s encounters from the dungeon graph, then instances a scene per placement
    /// under itself: doors on their door cells turned to the door, clues and objects on free cells facing the
    /// camera. Independent of where the dungeon comes from: <see cref="_Ready"/> reads the graph stored on
    /// <see cref="Map"/>; a runtime generator would call <see cref="Populate"/> with the graph it built.
    /// Node names are fixed (<c>Room04/Lock_0</c>), so the spawned nodes' path-based save ids are the same on
    /// every run; their saved data is bound with <see cref="SaveLoadManager.BindSubtree"/>.
    /// </summary>
    [GlobalClass]
    public partial class DungeonPopulator : Node3D
    {
        [Export] public GridMap Map { get; set; }
        [Export] public DungeonRecipe Recipe { get; set; }

        /// <summary>0: the seed the dungeon was generated with (its settings on <see cref="Map"/>).</summary>
        [Export] public long Seed { get; set; }

        /// <summary>MeshLibrary items something may stand on.</summary>
        [Export] public string[] FreeFloorTiles { get; set; } = { "room_floor", "room_floor_b", "room_circle", "room_wall" };

        /// <summary>Put here at the start room's centre (a new game; a saved position is bound later and wins).</summary>
        [Export] public Node3D Player { get; set; }

        /// <summary>Moved to the start room's centre, so a <see cref="Levels.LevelExit"/> into this level lands there.</summary>
        [Export] public Levels.SpawnPoint StartSpawn { get; set; }

        /// <summary>
        /// The goal room's fixed set (Spark Test platform, wizard, exit), built in the level around its origin: moved
        /// to the <see cref="GoalAnchor"/> cell and turned so its +Z points out through the entrance. Its cell and the
        /// one behind it are kept free of locks, clues and objects.
        /// </summary>
        [Export] public Node3D GoalRoom { get; set; }

        /// <summary>MeshLibrary items the goal set may stand on (plain floor: no walls, pillars).</summary>
        [Export] public string[] GoalFloorTiles { get; set; } = { "room_floor", "room_floor_b", "room_circle" };

        DungeonGraph _graph;

        public DungeonRoute Route { get; private set; }
        public IReadOnlyList<Placement> Placements { get; private set; } = Array.Empty<Placement>();
        public GoalAnchor? Goal { get; private set; }

        public override void _Ready()
        {
            var graph = Map == null ? null : DungeonGraph.StoredOn(Map);
            if (graph == null)
            {
                GD.PushWarning($"{Name}: no dungeon graph on {(Map == null ? "(no Map)" : Map.Name)}; generate the dungeon first");
                return;
            }
            Populate(graph);
        }

        /// <summary>Replaces everything this populator placed with a fresh plan for <paramref name="graph"/>.</summary>
        public void Populate(DungeonGraph graph)
        {
            foreach (var child in GetChildren())
            {
                RemoveChild(child);
                child.Free();   // now: the new nodes reuse the names
            }
            _graph = graph;
            if (Recipe == null || graph == null || Map == null)
                return;

            long seed = Seed != 0 ? Seed : DungeonSettings.StoredOn(Map)?.Seed ?? 0;
            var roles = graph.Rooms.Select(RoleOf).ToArray();
            Route = RoutePlanner.Plan(graph.CoreGraph, roles, seed);

            var goalFloor = new HashSet<string>(GoalFloorTiles ?? Array.Empty<string>());
            Goal = GoalRoom == null ? null : GoalAnchorPicker.Pick(graph.CoreGraph, Route, cell => goalFloor.Contains(ItemName(cell)));
            if (GoalRoom != null && Goal == null)
                GD.PushWarning($"{Name}: no floor cell for {GoalRoom.Name} in the goal room (room {Route.Goal})");
            var reserved = Goal is { } g ? new[] { g.Cell, g.Behind } : null;

            var free = new HashSet<string>(FreeFloorTiles ?? Array.Empty<string>());
            var skipped = new List<string>();
            Placements = Recipe.Plan(graph.CoreGraph, Route, cell => free.Contains(ItemName(cell)), seed, skipped, reserved).Placements;
            foreach (var line in skipped)
                GD.PushWarning($"{Name}: slot skipped: {line}");

            var spawned = new Node3D[Placements.Count];
            var counts = new Dictionary<(int, PlacementKind), int>();
            for (int i = 0; i < Placements.Count; i++)
            {
                var p = Placements[i];
                var scene = SceneFor(p);
                if (scene == null)
                {
                    GD.PushWarning($"{Name}: {p.Encounter.Id} has no scene for its {p.Kind}");
                    continue;
                }
                var node = scene.Instantiate<Node3D>();
                counts.TryGetValue((p.Room, p.Kind), out int n);
                counts[(p.Room, p.Kind)] = n + 1;
                node.Name = $"{p.Kind}_{n}";

                // the door's challenge gets the lock's seed before anything reads it; its clues point at it
                if (p.Kind == PlacementKind.Clue && node is CodeClue clue && p.Owner >= 0 && spawned[p.Owner] != null)
                    clue.Source = TriggerIn(spawned[p.Owner]);

                // placed before it enters the tree: physics bodies inside (a door's AnimatableBody3D leaves)
                // don't follow a parent that is moved afterwards
                var parent = RoomNode(p.Room);
                node.Transform = parent.GlobalTransform.AffineInverse() * TransformFor(p);
                parent.AddChild(node);
                if (p.Kind == PlacementKind.Lock && TriggerIn(node) is { } trigger)
                    trigger.Seed = p.Seed;
                spawned[i] = node;
            }

            var start = graph.RoomCentre(Map, Route.Start) + Vector3.Up * 0.1f;
            if (Player != null)
                Player.GlobalPosition = start;
            if (StartSpawn != null)
                StartSpawn.GlobalPosition = start;
            if (GoalRoom != null && Goal is { } goal)
                GoalRoom.GlobalTransform = Map.GlobalTransform * new Transform3D(
                    Basis.LookingAt(new Vector3(goal.Facing.X, 0, goal.Facing.Z), Vector3.Up, useModelFront: true),
                    Map.MapToLocal(goal.Cell));

            SaveLoadManager.Instance?.BindSubtree(this);
        }

        /// <summary>
        /// A room's role ("start", "goal"): from its room box when it has one, so a role set on the box after the
        /// last Generate counts too (Generate is what copies the box's Data into the graph); else the graph's copy.
        /// </summary>
        string RoleOf(DungeonRoom room)
        {
            var data = room.Box.IsEmpty ? null : Map.GetNodeOrNull<DungeonRoomBox>(room.Box)?.Data;
            data ??= room.Data;
            return data != null && data.TryGetValue("role", out var role) ? role.AsString() : "";
        }

        string ItemName(Vector3I cell)
        {
            int item = Map.GetCellItem(cell);
            return item < 0 || Map.MeshLibrary == null ? "" : Map.MeshLibrary.GetItemName(item);
        }

        static PackedScene SceneFor(Placement p) => p.Encounter switch
        {
            LockEncounter lockEncounter => p.Kind == PlacementKind.Lock ? lockEncounter.DoorScene : lockEncounter.ClueScene,
            ObjectEncounter objectEncounter => objectEncounter.Scene,
            _ => null,
        };

        Node3D RoomNode(int room)
        {
            var name = $"Room{room:00}";
            if (GetNodeOrNull<Node3D>(name) is { } existing)
                return existing;
            var node = new Node3D { Name = name };
            AddChild(node);
            return node;
        }

        static MathChallengeTrigger TriggerIn(Node node) =>
            node is CodeDoor door ? door.Challenge : node.FindObjectsByType<MathChallengeTrigger>().FirstOrDefault();

        /// <summary>
        /// Doors are built N-S with the outside to the north (-Z): turn -Z to the door direction. Clues and
        /// objects face +Z, towards the game's camera (it looks north from the south), so a clue is always
        /// readable; facing away from a wall would show an east/west wall's board edge-on and a south wall's
        /// from behind.
        /// </summary>
        Transform3D TransformFor(Placement p)
        {
            var basis = p.Kind == PlacementKind.Lock
                ? Basis.LookingAt(new Vector3(p.DoorDirection.X, 0, p.DoorDirection.Z), Vector3.Up)
                : Basis.Identity;
            return Map.GlobalTransform * new Transform3D(basis, Map.MapToLocal(p.Cell));
        }

    }
}
