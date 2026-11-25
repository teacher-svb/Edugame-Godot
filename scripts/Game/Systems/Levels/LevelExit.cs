using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.GameStates;
using TnT.EduGame.Characters;

namespace TnT.EduGame.Levels
{
    /// <summary>
    /// The player walking into this area loads <see cref="TargetScene"/> and arrives at its <see cref="SpawnPoint"/>
    /// <see cref="TargetSpawn"/> (a node path relative to the target scene's root; empty: the level's default spawn
    /// point). With no target scene, the exit teleports the player to a spawn point in the current level instead
    /// (through <see cref="StateManagerGame.LoadLocation"/>, with the fade; it can be used again). In the inspector,
    /// <c>Spawn Point</c> is a dropdown of the spawn points in the target scene (or in the edited scene). A quest opens
    /// a closed exit with
    /// <see cref="Open"/> (e.g. a <c>CallMethodStep</c> under a <c>QuestsCompletedListener</c>, which also runs on
    /// <c>_Ready</c>, so the open state needs no save data). A player already inside when the level starts (arrived
    /// on top of it) is ignored until they step out.
    /// </summary>
    [Tool, GlobalClass]
    public partial class LevelExit : Area3D
    {
        string _targetScene = "";
        /// <summary>Empty: a teleport within the current level.</summary>
        [Export(PropertyHint.File, "*.tscn,*.scn")]
        public string TargetScene
        {
            get => _targetScene;
            set { _targetScene = value ?? ""; RefreshEditor(); }
        }

        // Saved as a string, not a NodePath: the editor rewrites a node's NodePath properties (relative to the node)
        // when the node is moved or renamed, which would break a path into another scene.
        [Export] string _targetSpawnPath = "";
        /// <summary>Edited through the <c>Spawn Point</c> dropdown (see <see cref="_GetPropertyList"/>).</summary>
        public NodePath TargetSpawn
        {
            get => new(_targetSpawnPath);
            set { _targetSpawnPath = value?.ToString() ?? ""; RefreshEditor(); }
        }

        bool _enabled = true;
        /// <summary>A closed exit doesn't react to the player (its collision, if any, is up to the level).</summary>
        [Export]
        public bool Enabled
        {
            get => _enabled;
            set { _enabled = value; if (IsInsideTree()) SetDeferred(Area3D.PropertyName.Monitoring, value); else Monitoring = value; UpdateVisual(); }
        }

        /// <summary>Optional: shown in the game only while the exit is open (e.g. a portal). Always shown in the editor.</summary>
        [Export] public Node3D Visual { get; set; }

        void UpdateVisual()
        {
            if (Visual != null && !Engine.IsEditorHint())
                Visual.Visible = _enabled;
        }

        [Signal] public delegate void UsedEventHandler();

        // physics frames after the level started in which an entering player counts as already inside
        const int SettleFrames = 5;
        int _frames;
        bool _playerInsideAtStart;
        bool _used;

        public override void _Ready()
        {
            Monitoring = _enabled;
            UpdateVisual();
            if (Engine.IsEditorHint())
                return;
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_frames < SettleFrames)
                _frames++;
        }

        public void Open() => Enabled = true;
        public void Close() => Enabled = false;

        void OnBodyEntered(Node3D body)
        {
            if (!Player.IsPlayer(body))
                return;
            if (_frames < SettleFrames)
            {
                _playerInsideAtStart = true;
                return;
            }
            if (_playerInsideAtStart || _used)
                return;

            if (TargetScene == "")
            {
                Teleport();
                return;
            }
            _used = true;
            EmitSignal(SignalName.Used);
            StateManagerGame.Instance.LoadScene(TargetScene, TargetSpawn);
        }

        void Teleport()
        {
            var spawn = SpawnPoint.Find(GetTree(), TargetSpawn, out bool missing);
            if (missing)
                GD.PushWarning($"{GetPath()}: no spawn point at '{TargetSpawn}' in this level, using the default one");
            if (spawn == null)
            {
                GD.PushWarning($"{GetPath()}: no spawn point to teleport to");
                return;
            }
            // until the player has left this area: two collision shapes entered at once teleport only once
            _used = true;
            EmitSignal(SignalName.Used);
            StateManagerGame.Instance.LoadLocation(spawn.GlobalPosition);
        }

        void OnBodyExited(Node3D body)
        {
            if (!Player.IsPlayer(body))
                return;
            _playerInsideAtStart = false;
            if (TargetScene == "")
                _used = false; // a teleport can be used again
        }

        // ---- Editor: the Spawn Point dropdown and warnings ----

        static readonly StringName SpawnPick = "spawn_point";
        const string DefaultOption = "(default)";
        const string MissingSuffix = " (missing)";

        void RefreshEditor()
        {
            if (!Engine.IsEditorHint())
                return;
            NotifyPropertyListChanged();
            UpdateConfigurationWarnings();
        }

        /// <summary>The spawn points in the target scene, or in the edited scene for a teleport within the level.</summary>
        List<(string Path, bool IsDefault)> AvailableSpawns()
        {
            if (TargetScene != "")
                return SpawnPoint.ListIn(TargetScene);
            // GetTree().EditedSceneRoot, not EditorInterface: game code must not use the editor assembly
            var root = (IsInsideTree() ? GetTree().EditedSceneRoot : null) ?? Owner;
            if (root == null)
                return [];
            return root.FindChildren("*", "Marker3D", true, false).OfType<SpawnPoint>()
                .Select(p => (root.GetPathTo(p).ToString(), p.Default)).ToList();
        }

        List<string> SpawnOptions(List<(string Path, bool IsDefault)> spawns)
        {
            var options = new List<string> { DefaultOption };
            options.AddRange(spawns.Select(s => s.Path));
            var current = TargetSpawn.ToString();
            if (current != "" && !options.Contains(current))
                options.Add(current + MissingSuffix);
            return options;
        }

        public override void _ValidateProperty(Godot.Collections.Dictionary property)
        {
            // the saved path is edited through the dropdown
            if (property["name"].AsStringName() == PropertyName._targetSpawnPath)
                property["usage"] = (int)PropertyUsageFlags.Storage;
        }

        public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
        {
            if (!Engine.IsEditorHint())
                return null;
            return
            [
                new()
                {
                    { "name", SpawnPick },
                    { "type", (int)Variant.Type.String },
                    { "usage", (int)PropertyUsageFlags.Editor },
                    { "hint", (int)PropertyHint.Enum },
                    { "hint_string", string.Join(",", SpawnOptions(AvailableSpawns())) },
                },
            ];
        }

        public override Variant _Get(StringName property)
        {
            if (property != SpawnPick)
                return default;
            var current = TargetSpawn.ToString();
            if (current == "")
                return DefaultOption;
            return AvailableSpawns().Any(s => s.Path == current) ? current : current + MissingSuffix;
        }

        public override bool _Set(StringName property, Variant value)
        {
            if (property != SpawnPick)
                return false;
            var picked = value.AsString();
            if (picked.EndsWith(MissingSuffix))
                picked = picked[..^MissingSuffix.Length];
            TargetSpawn = picked == DefaultOption ? new NodePath() : new NodePath(picked);
            return true;
        }

        public override string[] _GetConfigurationWarnings()
        {
            var warnings = new List<string>();
            if (TargetScene != "" && !ResourceLoader.Exists(TargetScene))
                return [$"Target Scene '{TargetScene}' doesn't exist."];

            var spawns = AvailableSpawns();
            var current = TargetSpawn.ToString();
            if (current != "" && !spawns.Any(s => s.Path == current))
                warnings.Add($"The target scene has no spawn point at '{current}' (renamed or moved?); the player would arrive at its default one.");
            if (!spawns.Any(s => s.IsDefault) && (current == "" || !spawns.Any(s => s.Path == current)))
                warnings.Add(TargetScene == "" ? "No spawn point picked, and this level has no default one." : "The target scene has no default spawn point.");
            return [.. warnings];
        }
    }
}
