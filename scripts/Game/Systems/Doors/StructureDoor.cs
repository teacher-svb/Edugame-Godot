using System.Linq;
using Godot;
using TnT.Extensions;
using TnT.Systems.Persistence;

namespace TnT.EduGame.Doors
{
    /// <summary>
    /// A door of a village structure (the structure GLBs in <c>assets/3D/village/structures/</c>), opened and closed by
    /// quests: <c>CallMethodStep</c> with <see cref="Open"/>, <see cref="Close"/> or <see cref="Toggle"/>. Closed at the
    /// start. Each leaf has its own collider in the GLB (<c>&lt;Leaf&gt;_Collision-convcolonly</c>), a child of the
    /// leaf, so it swings with it: a closed door blocks its doorway.
    /// </summary>
    /// <remarks>
    /// The swing is the GLB's <c>&lt;Doorway&gt;_Open</c> / <c>_Close</c> animation (e.g. <c>School_Door_Open</c>). It
    /// plays on an <see cref="AnimationPlayer"/> of this node, with copies of those two animations, not on the
    /// structure's own player: the wizard tower's player runs <c>WizardTower_Turn-loop</c>, which a door animation
    /// played there would stop. The open state is saved (<see cref="DoorSaveData"/> under a scene-scoped id, bound by
    /// <c>SaveLoadManager</c>) and restored without the swing; opening an open door (a quest catch-up after a load)
    /// does nothing.
    /// </remarks>
    [GlobalClass]
    public partial class StructureDoor : Node, IBind<DoorSaveData>
    {
        /// <summary>The structure's instance (the GLB scene root, e.g. <c>NavigationRegion3D/school</c>).</summary>
        [Export] public Node3D Structure { get; set; }

        /// <summary>The doorway's name in the GLB: the leaf name without <c>_Leaf0</c>/<c>_Leaf1</c>, e.g. <c>School_Door</c>.</summary>
        [Export] public string Doorway { get; set; } = "";

        public string PersistentId => PersistentIds.SceneScoped(this);   // "<level scene>#<path in the level>", read at bind time
        public bool IsOpen { get; private set; }

        DoorSaveData _saveData;
        AnimationPlayer _player;

        string OpenAnim => "door/" + Doorway + "_Open";
        string CloseAnim => "door/" + Doorway + "_Close";

        public override void _Ready()
        {
            _player = BuildPlayer();
            if (IsOpen)
                Snap(true);   // bound before _Ready
        }

        // an AnimationPlayer under this node, rooted at the structure, with copies of the doorway's two animations
        AnimationPlayer BuildPlayer()
        {
            var source = Structure?.FindAnyObjectByType<AnimationPlayer>(includeSelf: true);
            if (source == null || !source.HasAnimation(Doorway + "_Open") || !source.HasAnimation(Doorway + "_Close"))
            {
                GD.PushError($"{Name}: no animations '{Doorway}_Open' / '_Close' on {Structure?.Name ?? "(no structure)"}");
                return null;
            }
            // Godot's glTF import gives every animation a track for every animated node of the file (the GLB's own
            // door animations only move their leaves), so the copies keep only this doorway's leaf tracks, and the
            // structure's own animations (the wizard tower's turn) lose them: else both players write the leaves
            var lib = new AnimationLibrary();
            foreach (var suffix in new[] { "_Open", "_Close" })
            {
                var a = (Animation)source.GetAnimation(Doorway + suffix).Duplicate();
                a.LoopMode = Animation.LoopModeEnum.None;
                RemoveTracks(a, keepLeaves: true);
                lib.AddAnimation(Doorway + suffix, a);
            }
            StripLeavesFrom(source);
            var player = new AnimationPlayer { Name = "DoorPlayer" };
            AddChild(player);
            // the source's track paths are relative to its root node (the structure's scene root)
            player.RootNode = player.GetPathTo(source.GetNode(source.RootNode));
            player.AddAnimationLibrary("door", lib);
            return player;
        }

        // a track's node is one of this doorway's leaves: "School_Door_Leaf0", "WizardTower/WizardTower_Door", ...
        bool IsLeafTrack(Animation a, int track)
        {
            var path = a.TrackGetPath(track);
            if (path.GetNameCount() == 0)
                return false;
            string node = path.GetName(path.GetNameCount() - 1);
            return node == Doorway || node.StartsWith(Doorway + "_Leaf");
        }

        // keepLeaves: removes every other track; else removes the leaf tracks
        void RemoveTracks(Animation a, bool keepLeaves)
        {
            for (int t = a.GetTrackCount() - 1; t >= 0; t--)
                if (IsLeafTrack(a, t) != keepLeaves)
                    a.RemoveTrack(t);
        }

        // the structure's player keeps its other animations (e.g. WizardTower_Turn) without this door's leaves; its
        // libraries are replaced by copies for this player only, so the imported resources stay as they are
        void StripLeavesFrom(AnimationPlayer source)
        {
            foreach (var libName in source.GetAnimationLibraryList())
            {
                var lib = source.GetAnimationLibrary(libName);
                bool touches = lib.GetAnimationList().Any(n => !IsDoorAnimation(n) && HasLeafTrack(lib.GetAnimation(n)));
                if (!touches)
                    continue;
                var copy = new AnimationLibrary();
                foreach (var n in lib.GetAnimationList())
                {
                    var a = lib.GetAnimation(n);
                    if (!IsDoorAnimation(n) && HasLeafTrack(a))
                    {
                        a = (Animation)a.Duplicate();
                        RemoveTracks(a, keepLeaves: false);
                    }
                    copy.AddAnimation(n, a);
                }
                // keep what it plays: replacing the library stops the player
                string playing = source.IsPlaying() ? source.CurrentAnimation : null;
                double pos = playing != null ? source.CurrentAnimationPosition : 0;
                source.RemoveAnimationLibrary(libName);
                source.AddAnimationLibrary(libName, copy);
                if (playing != null)
                {
                    source.Play(playing);
                    source.Seek(pos, update: true);
                }
            }
        }

        static bool IsDoorAnimation(StringName n) => n.ToString().EndsWith("_Open") || n.ToString().EndsWith("_Close") || n == "RESET";

        bool HasLeafTrack(Animation a) => Enumerable.Range(0, a.GetTrackCount()).Any(t => IsLeafTrack(a, t));

        public void Bind(DoorSaveData data)
        {
            _saveData = data;
            if (!data.IsNew && data.IsOpen)
            {
                IsOpen = true;
                if (IsNodeReady())
                    Snap(true);
            }
        }

        /// <summary>Swings the door open (nothing when it is open already).</summary>
        public void Open() => Swing(true);

        /// <summary>Swings the door shut (nothing when it is closed already).</summary>
        public void Close() => Swing(false);

        public void Toggle() => Swing(!IsOpen);

        void Swing(bool open)
        {
            if (open == IsOpen)
                return;
            IsOpen = open;
            if (_saveData != null)
                _saveData.IsOpen = open;
            if (_player == null)
                return;
            // a swing still running is replaced: the new one starts from its own first pose (fully open or shut)
            _player.Play(open ? OpenAnim : CloseAnim);
        }

        // the end pose at once (loading a save)
        void Snap(bool open)
        {
            if (_player == null)
                return;
            _player.Play(open ? OpenAnim : CloseAnim);
            _player.Seek(_player.CurrentAnimationLength, update: true);
        }
    }
}
