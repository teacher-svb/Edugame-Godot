using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame.Platforming
{
    /// <summary>
    /// Keeps the player safe in a platforming level: remembers the last room or landing cell the player stood on,
    /// and puts the player back there after a fall below <see cref="KillY"/> or a hit from a <see cref="ChallengeHazard"/>.
    /// Put one in each level that has challenge bridges; hazards find it through <see cref="Instance"/>.
    /// </summary>
    [GlobalClass]
    public partial class ChamberSafety : Node
    {
        public static ChamberSafety Instance { get; private set; }

        /// <summary>The level's GridMap; safe cells are found by their MeshLibrary item names.</summary>
        [Export] public GridMap Grid { get; set; }

        /// <summary>Falling below this height (world Y) counts as a fall.</summary>
        [Export] public float KillY { get; set; } = -2.5f;

        /// <summary>Cells whose item name starts with one of these are safe respawn points.</summary>
        [Export] public string[] SafePrefixes { get; set; } = { "room_", "landing_", "dungeon_door", "door_", "stairs_" };

        /// <summary>Seconds after a respawn during which hazards cannot hit again.</summary>
        [Export] public float Grace { get; set; } = 0.75f;

        /// <summary>Emitted after the player was put back on a safe cell.</summary>
        [Signal] public delegate void RespawnedEventHandler();

        Vector3 _lastSafe;
        bool _hasSafe;
        double _graceLeft;

        public override void _EnterTree() => Instance = this;

        public override void _ExitTree()
        {
            if (Instance == this) Instance = null;
        }

        public static Character3D PlayerBody => Player.Instance?.GetParentOrNull<Character3D>();

        public static bool IsPlayer(Node3D body) => body != null && body == PlayerBody;

        public override void _PhysicsProcess(double delta)
        {
            if (_graceLeft > 0) _graceLeft -= delta;
            var body = PlayerBody;
            if (body == null || Grid == null) return;

            if (!_hasSafe)
            {
                _lastSafe = body.GlobalPosition;     // the start position until the first safe cell
                _hasSafe = true;
            }

            if (body.GlobalPosition.Y < KillY)
            {
                Respawn();
                return;
            }

            if (body.IsOnFloor() && IsSafeCell(body.GlobalPosition, out var centre))
                _lastSafe = centre;
        }

        bool IsSafeCell(Vector3 worldPos, out Vector3 centre)
        {
            var cell = Grid.LocalToMap(Grid.ToLocal(worldPos));
            cell.Y = 0;
            centre = Vector3.Zero;
            int item = Grid.GetCellItem(cell);
            if (item == GridMap.InvalidCellItem || Grid.MeshLibrary == null) return false;
            string name = Grid.MeshLibrary.GetItemName(item);
            foreach (var prefix in SafePrefixes)
            {
                if (!name.StartsWith(prefix)) continue;
                var c = Grid.ToGlobal(Grid.MapToLocal(cell));
                centre = new Vector3(c.X, Grid.GlobalPosition.Y + 0.1f, c.Z);
                return true;
            }
            return false;
        }

        /// <summary>A hazard hit the player: respawn, unless a respawn just happened.</summary>
        public void Hit()
        {
            if (_graceLeft > 0) return;
            Respawn();
        }

        /// <summary>Puts the player back on the last safe cell, standing still.</summary>
        public void Respawn()
        {
            var body = PlayerBody;
            if (body == null || !_hasSafe) return;
            body.Velocity = Vector3.Zero;
            body.GlobalPosition = _lastSafe;
            _graceLeft = Grace;
            EmitSignal(SignalName.Respawned);
        }
    }
}
